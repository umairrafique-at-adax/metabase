using System;
using System.IO;
using MetabaseMigrator.Core.Config;

namespace MetabaseMigrator.Core.Services
{
    /// <summary>
    /// Logging levels for the application
    /// </summary>
    public enum LogLevel
    {
        Debug = 0,
        Info = 1,
        Warning = 2,
        Error = 3
    }

    /// <summary>
    /// Provides logging functionality with different levels and output options
    /// </summary>
    public class LoggerService
    {
        private readonly bool _enableLogging;
        private readonly LogLevel _minLogLevel;
        private readonly string? _logFilePath;

        public LoggerService(MigrationConfig config, string? logFilePath = null)
        {
            _enableLogging = config.EnableLogging;
            _minLogLevel = ParseLogLevel(config.LogLevel);
            _logFilePath = logFilePath;

            if (!string.IsNullOrEmpty(_logFilePath))
            {
                EnsureLogDirectoryExists();
            }
        }

        /// <summary>
        /// Log debug information
        /// </summary>
        public void LogDebug(string message)
        {
            Log(LogLevel.Debug, message, "DEBUG");
        }

        /// <summary>
        /// Log informational messages
        /// </summary>
        public void LogInfo(string message)
        {
            Log(LogLevel.Info, message, "INFO");
        }

        /// <summary>
        /// Log warning messages
        /// </summary>
        public void LogWarning(string message)
        {
            Log(LogLevel.Warning, message, "WARN", ConsoleColor.Yellow);
        }

        /// <summary>
        /// Log error messages
        /// </summary>
        public void LogError(string message)
        {
            Log(LogLevel.Error, message, "ERROR", ConsoleColor.Red);
        }

        /// <summary>
        /// Log error with exception details
        /// </summary>
        public void LogError(string message, Exception ex)
        {
            var fullMessage = $"{message}: {ex.Message}";
            if (_minLogLevel <= LogLevel.Debug)
            {
                fullMessage += $"\nStackTrace: {ex.StackTrace}";
            }
            Log(LogLevel.Error, fullMessage, "ERROR", ConsoleColor.Red);
        }

        /// <summary>
        /// Log success messages (always shown if logging is enabled)
        /// </summary>
        public void LogSuccess(string message)
        {
            Log(LogLevel.Info, $"✓ {message}", "SUCCESS", ConsoleColor.Green);
        }

        /// <summary>
        /// Log failure messages (always shown if logging is enabled)
        /// </summary>
        public void LogFailure(string message)
        {
            Log(LogLevel.Error, $"✗ {message}", "FAILURE", ConsoleColor.Red);
        }

        /// <summary>
        /// Log step information with step numbers
        /// </summary>
        public void LogStep(int stepNumber, string stepName, string message)
        {
            Log(LogLevel.Info, $"[Step {stepNumber}] {stepName}: {message}", "STEP", ConsoleColor.Cyan);
        }

        /// <summary>
        /// Log progress information
        /// </summary>
        public void LogProgress(string message)
        {
            Log(LogLevel.Info, $"→ {message}", "PROGRESS", ConsoleColor.Blue);
        }

        private void Log(LogLevel level, string message, string levelName, ConsoleColor? color = null)
        {
            if (!_enableLogging || level < _minLogLevel)
                return;

            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var formattedMessage = $"[{timestamp}] [{levelName}] {message}";

            // Console output with color
            if (color.HasValue)
            {
                var originalColor = System.Console.ForegroundColor;
                System.Console.ForegroundColor = color.Value;
                System.Console.WriteLine(formattedMessage);
                System.Console.ForegroundColor = originalColor;
            }
            else
            {
                System.Console.WriteLine(formattedMessage);
            }

            // File output (if configured)
            if (!string.IsNullOrEmpty(_logFilePath))
            {
                try
                {
                    File.AppendAllText(_logFilePath, formattedMessage + Environment.NewLine);
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"Warning: Failed to write to log file: {ex.Message}");
                }
            }
        }

        private LogLevel ParseLogLevel(string logLevel)
        {
            return logLevel.ToUpperInvariant() switch
            {
                "DEBUG" => LogLevel.Debug,
                "INFO" => LogLevel.Info,
                "WARNING" or "WARN" => LogLevel.Warning,
                "ERROR" => LogLevel.Error,
                _ => LogLevel.Info
            };
        }

        private void EnsureLogDirectoryExists()
        {
            if (!string.IsNullOrEmpty(_logFilePath))
            {
                var directory = Path.GetDirectoryName(_logFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
            }
        }

        /// <summary>
        /// Create a log file path with timestamp
        /// </summary>
        public static string CreateLogFilePath(string basePath = "logs", string prefix = "migration")
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var fileName = $"{prefix}_{timestamp}.log";
            return Path.Combine(basePath, fileName);
        }
    }
}