using System;
using System.IO;
using System.Text;

namespace Shared.Helpers
{
    public static class AppLogger
    {
        private static readonly object Sync = new();

        public static void Info(string message) => Write("INFO", message);
        public static void Warn(string message) => Write("WARN", message);
        public static void Error(string message, Exception? exception = null) => Write("ERROR", message, exception);

        private static void Write(string level, string message, Exception? exception = null)
        {
            try
            {
                var line = BuildLine(level, message, exception);
                lock (Sync)
                {
                    File.AppendAllText(AppPaths.GetLogPath(), line, Encoding.UTF8);
                }
            }
            catch
            {
                // Never throw from logger.
            }
        }

        private static string BuildLine(string level, string message, Exception? exception)
        {
            var builder = new StringBuilder();
            builder.Append(DateTime.UtcNow.ToString("u"));
            builder.Append(' ');
            builder.Append(level);
            builder.Append(": ");
            builder.Append(message);

            if (exception is not null)
            {
                builder.AppendLine();
                builder.Append(exception);
            }

            builder.AppendLine();
            return builder.ToString();
        }
    }
}
