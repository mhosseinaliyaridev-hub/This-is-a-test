using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using BeautyStudio.Domain.Configuration;
using BeautyStudio.Domain.Interfaces;

namespace BeautyStudio.Infrastructure.Services;

public class FileStorageService : IFileStorageService, IDisposable
{
    private readonly StorageSettings _settings;
    private readonly ILoggerService _logger;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();
    private readonly JsonSerializerOptions _jsonOptions;
    private bool _disposed;

    public FileStorageService(StorageSettings settings, ILoggerService logger)
    {
        _settings = settings;
        _logger = logger;
        
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        EnsureBaseDirectoryExists();
    }

    private void EnsureBaseDirectoryExists()
    {
        if (!Directory.Exists(_settings.BasePath))
        {
            Directory.CreateDirectory(_settings.BasePath);
        }
    }

    private string GetFilePath(string fileName)
    {
        // Prevent path traversal attacks
        var sanitizedFileName = Path.GetFileName(fileName);
        return Path.Combine(_settings.BasePath, sanitizedFileName);
    }

    private async Task<SemaphoreSlim> GetLockAsync(string fileName)
    {
        return _locks.GetOrAdd(fileName, _ => new SemaphoreSlim(1, 1));
    }

    public async Task<T?> ReadFileAsync<T>(string fileName) where T : class
    {
        var filePath = GetFilePath(fileName);
        
        if (!File.Exists(filePath))
        {
            _logger.LogWarning($"File not found: {filePath}");
            return null;
        }

        var semaphore = await GetLockAsync(fileName);
        await semaphore.WaitAsync();

        try
        {
            var json = await File.ReadAllTextAsync(filePath, Encoding.UTF8);
            return JsonSerializer.Deserialize<T>(json, _jsonOptions);
        }
        catch (JsonException ex)
        {
            _logger.LogError($"Invalid JSON in file {filePath}", ex);
            throw new InvalidOperationException($"Invalid JSON format in file: {fileName}", ex);
        }
        catch (IOException ex)
        {
            _logger.LogError($"IO error reading file {filePath}", ex);
            throw;
        }
        finally
        {
            semaphore.Release();
        }
    }

    public async Task<List<T>> ReadAllFilesAsync<T>(string entityName) where T : class
    {
        var files = await GetAllFilesAsync(entityName);
        var allItems = new List<T>();

        foreach (var file in files)
        {
            try
            {
                var data = await ReadFileAsync<T>(file);
                if (data is ICollection<T> collection)
                {
                    allItems.AddRange(collection);
                }
                else if (data is T singleItem)
                {
                    allItems.Add(singleItem);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error reading file {file}", ex);
            }
        }

        return allItems;
    }

    public async Task<bool> WriteFileAsync<T>(string fileName, T data) where T : class
    {
        var filePath = GetFilePath(fileName);
        var semaphore = await GetLockAsync(fileName);
        await semaphore.WaitAsync();

        try
        {
            // Create backup if enabled
            if (_settings.BackupEnabled && File.Exists(filePath))
            {
                await CreateBackupInternalAsync(fileName);
            }

            // Check file size before writing
            var json = JsonSerializer.Serialize(data, _jsonOptions);
            var fileSizeBytes = Encoding.UTF8.GetByteCount(json);
            var maxFileSizeBytes = _settings.MaxFileSizeMB * 1024 * 1024;

            if (fileSizeBytes > maxFileSizeBytes)
            {
                _logger.LogWarning($"Data exceeds max file size. File rotation needed for {fileName}");
                return false;
            }

            // Check if current file exists and would exceed limit
            if (File.Exists(filePath))
            {
                var currentFileSize = new FileInfo(filePath).Length;
                if (currentFileSize + fileSizeBytes > maxFileSizeBytes)
                {
                    _logger.LogInfo($"File {fileName} reached size limit. Creating new file.");
                    return await WriteToNewRotatedFileAsync(entityNameFromFileName(fileName), data);
                }
            }

            await File.WriteAllTextAsync(filePath, json, Encoding.UTF8);
            _logger.LogInfo($"Successfully wrote to file: {filePath}");
            return true;
        }
        catch (IOException ex)
        {
            _logger.LogError($"IO error writing to file {filePath}", ex);
            throw;
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError($"Permission denied for file {filePath}", ex);
            throw;
        }
        finally
        {
            semaphore.Release();
        }
    }

    public async Task<bool> AppendToFileAsync<T>(string fileName, T item) where T : class
    {
        var filePath = GetFilePath(fileName);
        var semaphore = await GetLockAsync(fileName);
        await semaphore.WaitAsync();

        try
        {
            List<T> existingData;

            // Read existing data or create new list
            if (File.Exists(filePath))
            {
                var json = await File.ReadAllTextAsync(filePath, Encoding.UTF8);
                existingData = JsonSerializer.Deserialize<List<T>>(json, _jsonOptions) ?? new List<T>();
            }
            else
            {
                existingData = new List<T>();
            }

            // Add new item
            existingData.Add(item);

            // Check file size
            var json = JsonSerializer.Serialize(existingData, _jsonOptions);
            var fileSizeBytes = Encoding.UTF8.GetByteCount(json);
            var maxFileSizeBytes = _settings.MaxFileSizeMB * 1024 * 1024;

            if (fileSizeBytes > maxFileSizeBytes)
            {
                _logger.LogInfo($"File {fileName} reached size limit during append. Creating new file.");
                
                // Remove the last item (the one we just added) from existing data
                existingData.RemoveAt(existingData.Count - 1);
                
                // Save remaining items back to current file
                if (existingData.Count > 0)
                {
                    var remainingJson = JsonSerializer.Serialize(existingData, _jsonOptions);
                    await File.WriteAllTextAsync(filePath, remainingJson, Encoding.UTF8);
                }

                // Write new item to rotated file
                return await WriteToNewRotatedFileAsync(entityNameFromFileName(fileName), item);
            }

            // Create backup if enabled
            if (_settings.BackupEnabled && File.Exists(filePath))
            {
                await CreateBackupInternalAsync(fileName);
            }

            // Write updated data
            await File.WriteAllTextAsync(filePath, json, Encoding.UTF8);
            _logger.LogInfo($"Successfully appended to file: {filePath}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error appending to file {filePath}", ex);
            throw;
        }
        finally
        {
            semaphore.Release();
        }
    }

    private string entityNameFromFileName(string fileName)
    {
        // Extract entity name from filename like "Appointments_001.json" -> "Appointments"
        var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
        var parts = nameWithoutExtension.Split('_');
        return parts.Length > 1 ? string.Join("_", parts.Take(parts.Length - 1)) : nameWithoutExtension;
    }

    private async Task<bool> WriteToNewRotatedFileAsync<T>(string entityName, T data) where T : class
    {
        try
        {
            // Find the next available file index
            var files = await GetAllFilesAsync(entityName);
            var maxIndex = 0;

            foreach (var file in files)
            {
                var nameWithoutExtension = Path.GetFileNameWithoutExtension(file);
                var parts = nameWithoutExtension.Split('_');
                if (parts.Length >= 2 && int.TryParse(parts[^1], out var index))
                {
                    maxIndex = Math.Max(maxIndex, index);
                }
            }

            var newIndex = maxIndex + 1;
            var newFileName = $"{entityName}_{newIndex:D3}.json";
            
            _logger.LogInfo($"Creating new rotated file: {newFileName}");
            
            return await WriteFileAsync(newFileName, new List<T> { data! });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error creating rotated file", ex);
            throw;
        }
    }

    public async Task<bool> FileExistsAsync(string fileName)
    {
        var filePath = GetFilePath(fileName);
        return await Task.FromResult(File.Exists(filePath));
    }

    public async Task<long> GetFileSizeAsync(string fileName)
    {
        var filePath = GetFilePath(fileName);
        
        if (!File.Exists(filePath))
        {
            return 0;
        }

        return await Task.FromResult(new FileInfo(filePath).Length);
    }

    public async Task<string[]> GetAllFilesAsync(string entityName)
    {
        try
        {
            var directoryInfo = new DirectoryInfo(_settings.BasePath);
            var files = directoryInfo.GetFiles($"{entityName}_*.json")
                .OrderBy(f => f.Name)
                .Select(f => f.Name)
                .ToArray();

            // Also check for the base file without index
            var baseFile = Path.Combine(_settings.BasePath, $"{entityName}.json");
            if (File.Exists(baseFile))
            {
                files = new[] { $"{entityName}.json" }.Concat(files).ToArray();
            }

            return files;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting files for entity {entityName}", ex);
            return Array.Empty<string>();
        }
    }

    public async Task<bool> CreateBackupAsync(string fileName)
    {
        var semaphore = await GetLockAsync(fileName);
        await semaphore.WaitAsync();

        try
        {
            return await CreateBackupInternalAsync(fileName);
        }
        finally
        {
            semaphore.Release();
        }
    }

    private async Task<bool> CreateBackupInternalAsync(string fileName)
    {
        var filePath = GetFilePath(fileName);
        
        if (!File.Exists(filePath))
        {
            return false;
        }

        try
        {
            // Ensure backup directory exists
            if (!Directory.Exists(_settings.BackupPath))
            {
                Directory.CreateDirectory(_settings.BackupPath);
            }

            // Create backup with timestamp
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var backupFileName = $"{Path.GetFileNameWithoutExtension(fileName)}_{timestamp}{Path.GetExtension(fileName)}";
            var backupPath = Path.Combine(_settings.BackupPath, backupFileName);

            await File.CopyAsync(filePath, backupPath, overwrite: true);
            _logger.LogInfo($"Created backup: {backupPath}");
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error creating backup for {fileName}", ex);
            return false;
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                foreach (var semaphore in _locks.Values)
                {
                    semaphore.Dispose();
                }
                _locks.Clear();
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
