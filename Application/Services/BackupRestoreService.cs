using Microsoft.Data.Sqlite;
using Shared.Helpers;
using System.IO;

namespace Application.Services
{
    public sealed class BackupRestoreService : IBackupRestoreService
    {
        public async Task<string> CreateBackupAsync(string? targetFilePath = null, CancellationToken cancellationToken = default)
        {
            var sourcePath = AppPaths.GetDatabasePath();
            if (!File.Exists(sourcePath))
            {
                throw new FileNotFoundException("Database file was not found.", sourcePath);
            }

            var targetPath = string.IsNullOrWhiteSpace(targetFilePath)
                ? Path.Combine(AppPaths.GetBackupDirectory(), $"possystem-backup-{DateTime.UtcNow:yyyyMMdd-HHmmss}.db")
                : targetFilePath.Trim();

            var targetDirectory = Path.GetDirectoryName(targetPath);
            if (string.IsNullOrWhiteSpace(targetDirectory))
            {
                throw new InvalidOperationException("Backup target directory is invalid.");
            }

            Directory.CreateDirectory(targetDirectory);
            if (File.Exists(targetPath))
            {
                File.Delete(targetPath);
            }

            using var source = new SqliteConnection($"Data Source={sourcePath};Mode=ReadOnly;");
            using var destination = new SqliteConnection($"Data Source={targetPath};Mode=ReadWriteCreate;");
            await source.OpenAsync(cancellationToken);
            await destination.OpenAsync(cancellationToken);
            source.BackupDatabase(destination);

            var valid = await VerifyDatabaseAsync(targetPath, cancellationToken);
            if (!valid)
            {
                throw new InvalidOperationException("Backup file failed integrity verification.");
            }

            return targetPath;
        }

        public async Task RestoreBackupAsync(string sourceFilePath, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(sourceFilePath))
            {
                throw new InvalidOperationException("Backup source path is required.");
            }

            var sourcePath = sourceFilePath.Trim();
            if (!File.Exists(sourcePath))
            {
                throw new FileNotFoundException("Backup file was not found.", sourcePath);
            }

            var validSource = await VerifyDatabaseAsync(sourcePath, cancellationToken);
            if (!validSource)
            {
                throw new InvalidOperationException("Backup source is corrupted.");
            }

            var targetPath = AppPaths.GetDatabasePath();
            using var source = new SqliteConnection($"Data Source={sourcePath};Mode=ReadOnly;");
            using var destination = new SqliteConnection($"Data Source={targetPath};Mode=ReadWriteCreate;");
            await source.OpenAsync(cancellationToken);
            await destination.OpenAsync(cancellationToken);
            source.BackupDatabase(destination);

            var validTarget = await VerifyDatabaseAsync(targetPath, cancellationToken);
            if (!validTarget)
            {
                throw new InvalidOperationException("Restored database failed integrity verification.");
            }
        }

        public async Task<bool> VerifyDatabaseAsync(string filePath, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                return false;
            }

            using var connection = new SqliteConnection($"Data Source={filePath};Mode=ReadOnly;");
            await connection.OpenAsync(cancellationToken);

            using var command = connection.CreateCommand();
            command.CommandText = "PRAGMA integrity_check;";
            var result = (await command.ExecuteScalarAsync(cancellationToken))?.ToString();
            return string.Equals(result, "ok", StringComparison.OrdinalIgnoreCase);
        }
    }
}
