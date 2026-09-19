namespace BeautyStudio.Domain.Configuration;

public class StorageSettings
{
    public string BasePath { get; set; } = "data";
    public int MaxFileSizeMB { get; set; } = 10;
    public string Encoding { get; set; } = "UTF-8";
    public bool BackupEnabled { get; set; } = true;
    public string BackupPath { get; set; } = "data/Backups";
}

public class FileRotationSettings
{
    public int MaxFileSizeMB { get; set; } = 10;
    public string FileNamingPattern { get; set; } = "{entity}_{index}.json";
}
