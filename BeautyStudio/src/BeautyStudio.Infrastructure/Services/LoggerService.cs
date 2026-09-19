using BeautyStudio.Domain.Interfaces;

namespace BeautyStudio.Infrastructure.Services;

public class LoggerService : ILoggerService
{
    private readonly string _logPath;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public LoggerService(string logPath)
    {
        _logPath = Path.Combine(logPath, "Logs");
        if (!Directory.Exists(_logPath))
        {
            Directory.CreateDirectory(_logPath);
        }
    }

    public void LogInfo(string message)
    {
        WriteLog(message, "INFO");
    }

    public void LogError(string message, Exception? ex = null)
    {
        var fullMessage = ex != null ? $"{message}: {ex.Message}\nStack Trace: {ex.StackTrace}" : message;
        WriteLog(fullMessage, "ERROR");
    }

    public void LogWarning(string message)
    {
        WriteLog(message, "WARNING");
    }

    public async Task WriteLogAsync(string message, string level = "INFO")
    {
        await _lock.WaitAsync();
        try
        {
            var logEntry = $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";
            var logFile = Path.Combine(_logPath, $"app_{DateTime.UtcNow:yyyyMMdd}.log");
            await File.AppendAllTextAsync(logFile, logEntry + Environment.NewLine);
        }
        finally
        {
            _lock.Release();
        }
    }

    private void WriteLog(string message, string level)
    {
        WriteLogAsync(message, level).GetAwaiter().GetResult();
    }
}
