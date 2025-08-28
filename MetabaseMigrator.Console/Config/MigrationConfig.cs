using System;

namespace MetabaseMigrator.Console.Config
{
    /// <summary>
    /// Main configuration class for Metabase migration
    /// </summary>
    public class MigrationConfig
    {
        public string SourceUrl { get; set; } = string.Empty;
        public string SourceAPIToken { get; set; } = string.Empty;
        public string TargetUrl { get; set; } = string.Empty;
        public string TargetAPIToken { get; set; } = string.Empty;
        //public string SourceUsername { get; set; } = string.Empty;
        //public string SourcePassword { get; set; } = string.Empty;
        //public string TargetUsername { get; set; } = string.Empty;
        //public string TargetPassword { get; set; } = string.Empty;
        public int TimeoutSeconds { get; set; } = 30;
        public bool EnableLogging { get; set; } = true;
        public string LogLevel { get; set; } = "Info";
        public int RetryAttempts { get; set; } = 3;
        public int RetryDelaySeconds { get; set; } = 5;
        public bool SkipExistingCards { get; set; } = false;
        public bool SkipExistingCollections { get; set; } = false;

        /// <summary>
        /// Validates the configuration settings
        /// </summary>
        public ValidationResult Validate()
        {
            var result = new ValidationResult();

            if (string.IsNullOrWhiteSpace(SourceUrl))
                result.AddError("SourceUrl is required");
            else if (!Uri.TryCreate(SourceUrl, UriKind.Absolute, out _))
                result.AddError("SourceUrl must be a valid URL");

            if (string.IsNullOrWhiteSpace(TargetUrl))
                result.AddError("TargetUrl is required");
            else if (!Uri.TryCreate(TargetUrl, UriKind.Absolute, out _))
                result.AddError("TargetUrl must be a valid URL");

            if (TimeoutSeconds <= 0)
                result.AddError("TimeoutSeconds must be greater than 0");

            if (RetryAttempts < 0)
                result.AddError("RetryAttempts cannot be negative");

            if (RetryDelaySeconds < 0)
                result.AddError("RetryDelaySeconds cannot be negative");

            var validLogLevels = new[] { "Debug", "Info", "Warning", "Error" };
            if (!Array.Exists(validLogLevels, level => level.Equals(LogLevel, StringComparison.OrdinalIgnoreCase)))
                result.AddError($"LogLevel must be one of: {string.Join(", ", validLogLevels)}");

            return result;
        }
    }

    /// <summary>
    /// Validation result class
    /// </summary>
    public class ValidationResult
    {
        private readonly List<string> _errors = new List<string>();

        public bool IsValid => _errors.Count == 0;
        public IReadOnlyList<string> Errors => _errors;

        public void AddError(string error)
        {
            _errors.Add(error);
        }

        public string GetErrorsAsString()
        {
            return string.Join(Environment.NewLine, _errors);
        }
    }
}