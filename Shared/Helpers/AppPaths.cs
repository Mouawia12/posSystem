using System;
using System.IO;

namespace Shared.Helpers
{
    public static class AppPaths
    {
        private const string AppFolderName = "posSystem";

        public static string GetAppDataDirectory()
        {
            var path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                AppFolderName);

            Directory.CreateDirectory(path);
            return path;
        }

        public static string GetDatabasePath()
        {
            return Path.Combine(GetAppDataDirectory(), "possystem.db");
        }

        public static string GetBackupDirectory()
        {
            var path = Path.Combine(GetAppDataDirectory(), "backups");
            Directory.CreateDirectory(path);
            return path;
        }

        public static string GetLogPath()
        {
            return Path.Combine(GetAppDataDirectory(), "app.log");
        }
    }
}
