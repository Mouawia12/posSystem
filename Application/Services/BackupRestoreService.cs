using Microsoft.Data.Sqlite;
using Shared.Helpers;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Application.Services
{
    public sealed class BackupRestoreService : IBackupRestoreService
    {
        private const string EncryptionKeyEnvVar = "POS_BACKUP_ENCRYPTION_KEY";
        private static readonly byte[] EncryptedHeader = Encoding.ASCII.GetBytes("PSE1");

        public async Task<string> CreateBackupAsync(string? targetFilePath = null, CancellationToken cancellationToken = default)
        {
            var sourcePath = AppPaths.GetDatabasePath();
            if (!File.Exists(sourcePath))
            {
                throw new FileNotFoundException("Database file was not found.", sourcePath);
            }

            var encryptionSecret = GetBackupEncryptionSecret();
            var targetPath = string.IsNullOrWhiteSpace(targetFilePath)
                ? Path.Combine(
                    AppPaths.GetBackupDirectory(),
                    encryptionSecret is null
                        ? $"possystem-backup-{DateTime.UtcNow:yyyyMMdd-HHmmss}.db"
                        : $"possystem-backup-{DateTime.UtcNow:yyyyMMdd-HHmmss}.db.enc")
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

            var rawBackupPath = encryptionSecret is null
                ? targetPath
                : Path.Combine(targetDirectory, $".backup-raw-{Guid.NewGuid():N}.db");

            using var source = new SqliteConnection($"Data Source={sourcePath};Mode=ReadOnly;");
            using var destination = new SqliteConnection($"Data Source={rawBackupPath};Mode=ReadWriteCreate;");
            await source.OpenAsync(cancellationToken);
            await destination.OpenAsync(cancellationToken);
            source.BackupDatabase(destination);

            var valid = await VerifyDatabasePlainAsync(rawBackupPath, cancellationToken);
            if (!valid)
            {
                throw new InvalidOperationException("Backup file failed integrity verification.");
            }

            if (encryptionSecret is not null)
            {
                EncryptFile(rawBackupPath, targetPath, encryptionSecret);
                File.Delete(rawBackupPath);
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

            var workingSourcePath = sourcePath;
            var shouldCleanupWorkingSource = false;

            if (IsEncryptedBackupFile(sourcePath))
            {
                var encryptionSecret = GetBackupEncryptionSecret()
                    ?? throw new InvalidOperationException(
                        $"Backup is encrypted. Set {EncryptionKeyEnvVar} before restore.");

                workingSourcePath = Path.Combine(Path.GetTempPath(), $"possystem-restore-{Guid.NewGuid():N}.db");
                DecryptFile(sourcePath, workingSourcePath, encryptionSecret);
                shouldCleanupWorkingSource = true;
            }

            var validSource = await VerifyDatabasePlainAsync(workingSourcePath, cancellationToken);
            if (!validSource)
            {
                if (shouldCleanupWorkingSource && File.Exists(workingSourcePath))
                {
                    File.Delete(workingSourcePath);
                }

                throw new InvalidOperationException("Backup source is corrupted.");
            }

            var targetPath = AppPaths.GetDatabasePath();
            using var source = new SqliteConnection($"Data Source={workingSourcePath};Mode=ReadOnly;");
            using var destination = new SqliteConnection($"Data Source={targetPath};Mode=ReadWriteCreate;");
            await source.OpenAsync(cancellationToken);
            await destination.OpenAsync(cancellationToken);
            source.BackupDatabase(destination);

            var validTarget = await VerifyDatabasePlainAsync(targetPath, cancellationToken);
            if (shouldCleanupWorkingSource && File.Exists(workingSourcePath))
            {
                File.Delete(workingSourcePath);
            }

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

            if (IsEncryptedBackupFile(filePath))
            {
                var encryptionSecret = GetBackupEncryptionSecret();
                if (encryptionSecret is null)
                {
                    return false;
                }

                var tempPath = Path.Combine(Path.GetTempPath(), $"possystem-verify-{Guid.NewGuid():N}.db");
                try
                {
                    DecryptFile(filePath, tempPath, encryptionSecret);
                    return await VerifyDatabasePlainAsync(tempPath, cancellationToken);
                }
                catch (CryptographicException)
                {
                    return false;
                }
                finally
                {
                    if (File.Exists(tempPath))
                    {
                        File.Delete(tempPath);
                    }
                }
            }

            return await VerifyDatabasePlainAsync(filePath, cancellationToken);
        }

        private static string? GetBackupEncryptionSecret()
        {
            var raw = Environment.GetEnvironmentVariable(EncryptionKeyEnvVar);
            return string.IsNullOrWhiteSpace(raw) ? null : raw.Trim();
        }

        private static bool IsEncryptedBackupFile(string path)
        {
            using var stream = File.OpenRead(path);
            if (stream.Length < EncryptedHeader.Length)
            {
                return false;
            }

            Span<byte> header = stackalloc byte[4];
            stream.ReadExactly(header);
            return header.SequenceEqual(EncryptedHeader);
        }

        private static void EncryptFile(string inputPath, string outputPath, string secret)
        {
            var salt = RandomNumberGenerator.GetBytes(16);
            var iv = RandomNumberGenerator.GetBytes(16);
            var key = Rfc2898DeriveBytes.Pbkdf2(secret, salt, 100_000, HashAlgorithmName.SHA256, 32);

            using var input = File.OpenRead(inputPath);
            using var output = File.Create(outputPath);
            output.Write(EncryptedHeader, 0, EncryptedHeader.Length);
            output.Write(salt, 0, salt.Length);
            output.Write(iv, 0, iv.Length);

            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var crypto = new CryptoStream(output, aes.CreateEncryptor(), CryptoStreamMode.Write);
            input.CopyTo(crypto);
            crypto.FlushFinalBlock();
        }

        private static void DecryptFile(string inputPath, string outputPath, string secret)
        {
            using var input = File.OpenRead(inputPath);
            Span<byte> header = stackalloc byte[4];
            input.ReadExactly(header);
            if (!header.SequenceEqual(EncryptedHeader))
            {
                throw new CryptographicException("Unsupported encrypted backup format.");
            }

            var salt = new byte[16];
            var iv = new byte[16];
            input.ReadExactly(salt);
            input.ReadExactly(iv);

            var key = Rfc2898DeriveBytes.Pbkdf2(secret, salt, 100_000, HashAlgorithmName.SHA256, 32);
            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var crypto = new CryptoStream(input, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using var output = File.Create(outputPath);
            crypto.CopyTo(output);
        }

        private static async Task<bool> VerifyDatabasePlainAsync(string filePath, CancellationToken cancellationToken)
        {
            using var connection = new SqliteConnection($"Data Source={filePath};Mode=ReadOnly;");
            await connection.OpenAsync(cancellationToken);

            using var command = connection.CreateCommand();
            command.CommandText = "PRAGMA integrity_check;";
            var result = (await command.ExecuteScalarAsync(cancellationToken))?.ToString();
            return string.Equals(result, "ok", StringComparison.OrdinalIgnoreCase);
        }
    }
}
