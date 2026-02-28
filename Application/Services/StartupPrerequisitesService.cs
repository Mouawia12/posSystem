using Application.DTOs;
using Core.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Shared.Helpers;
using System.IO;

namespace Application.Services
{
    public sealed class StartupPrerequisitesService : IStartupPrerequisitesService
    {
        private const string FirstRunMarkerFile = ".first-run.completed";
        private readonly IDbContextFactory<PosDbContext> _dbContextFactory;

        public StartupPrerequisitesService(IDbContextFactory<PosDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<PrerequisiteCheckResultDto> ValidateAndPrepareAsync(CancellationToken cancellationToken = default)
        {
            var messages = new List<string>();

            try
            {
                var appDataDir = AppPaths.GetAppDataDirectory();
                var backupDir = AppPaths.GetBackupDirectory();
                var logPath = AppPaths.GetLogPath();
                var markerPath = Path.Combine(appDataDir, FirstRunMarkerFile);
                var isFirstRun = !File.Exists(markerPath);

                ValidateWritableDirectory(appDataDir);
                ValidateWritableDirectory(backupDir);
                ValidateWritableFile(logPath);

                await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
                _ = await db.Database.CanConnectAsync(cancellationToken);

                var hasSettings = await db.Settings.AnyAsync(cancellationToken);
                if (!hasSettings)
                {
                    await db.Settings.AddAsync(new Setting
                    {
                        CompanyName = "My Retail Store",
                        InvoicePrefix = "INV",
                        NextInvoiceNumber = 1,
                        Currency = "USD"
                    }, cancellationToken);
                    await db.SaveChangesAsync(cancellationToken);
                    messages.Add("Default settings were initialized.");
                }

                if (isFirstRun)
                {
                    File.WriteAllText(markerPath, DateTime.UtcNow.ToString("u"));
                    messages.Add("First-run bootstrap completed.");
                }
                else
                {
                    messages.Add("Prerequisites verified.");
                }

                return new PrerequisiteCheckResultDto
                {
                    Passed = true,
                    IsFirstRun = isFirstRun,
                    Messages = messages
                };
            }
            catch (Exception ex)
            {
                messages.Add($"Prerequisite check failed: {ex.Message}");
                return new PrerequisiteCheckResultDto
                {
                    Passed = false,
                    IsFirstRun = false,
                    Messages = messages
                };
            }
        }

        private static void ValidateWritableDirectory(string directoryPath)
        {
            var probeFile = Path.Combine(directoryPath, $".probe-{Guid.NewGuid():N}.tmp");
            File.WriteAllText(probeFile, "probe");
            File.Delete(probeFile);
        }

        private static void ValidateWritableFile(string filePath)
        {
            File.AppendAllText(filePath, string.Empty);
        }
    }
}
