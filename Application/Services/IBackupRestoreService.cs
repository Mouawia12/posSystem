namespace Application.Services
{
    public interface IBackupRestoreService
    {
        Task<string> CreateBackupAsync(string? targetFilePath = null, CancellationToken cancellationToken = default);
        Task RestoreBackupAsync(string sourceFilePath, CancellationToken cancellationToken = default);
        Task<bool> VerifyDatabaseAsync(string filePath, CancellationToken cancellationToken = default);
    }
}
