using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace MetabaseMigrator.Console.Config
{
    /// <summary>
    /// Manages loading and saving configuration from various sources
    /// </summary>
    public static class ConfigManager
    {
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        /// <summary>
        /// Load configuration from JSON file
        /// </summary>
        public static MigrationConfig LoadFromFile(string configPath = "appsettings.json")
        {
            try  
            {
                if (File.Exists(configPath))
                {
                    var json = File.ReadAllText(configPath);
                    var config = JsonSerializer.Deserialize<MigrationConfig>(json, JsonOptions);
                    return config ?? CreateDefaultConfig();
                }
                else
                {
                    System.Console.WriteLine($"Config file not found: {configPath}");
                    System.Console.WriteLine("Creating default configuration file...");

                    var defaultConfig = CreateDefaultConfig();
                    SaveToFile(defaultConfig, configPath);

                    System.Console.WriteLine($"✓ Created default config file: {configPath}");
                    System.Console.WriteLine("Please update the configuration with your actual values before running again.");

                    return defaultConfig;
                }
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"Invalid JSON in config file '{configPath}': {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error loading config file '{configPath}': {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Load configuration from environment variables
        /// </summary>
        public static MigrationConfig LoadFromEnvironmentVariables()
        {
            return new MigrationConfig
            {
                SourceUrl = GetEnvironmentVariable("SOURCE_METABASE_URL"),
                TargetUrl = GetEnvironmentVariable("TARGET_METABASE_URL"),
                TimeoutSeconds = GetEnvironmentVariableAsInt("TIMEOUT_SECONDS", 30),
                EnableLogging = GetEnvironmentVariableAsBool("ENABLE_LOGGING", true),
                LogLevel = GetEnvironmentVariable("LOG_LEVEL", "Info"),
                RetryAttempts = GetEnvironmentVariableAsInt("RETRY_ATTEMPTS", 3),
                RetryDelaySeconds = GetEnvironmentVariableAsInt("RETRY_DELAY_SECONDS", 5),
                SkipExistingCards = GetEnvironmentVariableAsBool("SKIP_EXISTING_CARDS", false),
                SkipExistingCollections = GetEnvironmentVariableAsBool("SKIP_EXISTING_COLLECTIONS", false)
            };
        }

        /// <summary>
        /// Load configuration with priority: Environment Variables > File > Default
        /// </summary>
        public static MigrationConfig LoadWithPriority(string configPath = "appsettings.json")
        {
            var config = LoadFromFile(configPath);

            // Override with environment variables if they exist
            OverrideWithEnvironmentVariables(config);

            return config;
        }

        /// <summary>
        /// Save configuration to file
        /// </summary>
        public static void SaveToFile(MigrationConfig config, string configPath = "appsettings.json")
        {
            try
            {
                var directory = Path.GetDirectoryName(configPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var json = JsonSerializer.Serialize(config, JsonOptions);
                File.WriteAllText(configPath, json);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error saving config to '{configPath}': {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Create a template configuration file with comments
        /// </summary>
        public static void CreateTemplateFile(string configPath = "appsettings.template.json")
        {
            var template = @"{
              // Metabase Migration Configuration Template
              // Copy this file to appsettings.json and update with your values
  
              ""sourceUrl"": ""https://source-metabase.example.com"",
              ""targetUrl"": ""https://target-metabase.example.com"",
  
              // Authentication credentials
              ""sourceUsername"": ""source-user@example.com"",
              ""sourcePassword"": ""your-source-password"",
              ""targetUsername"": ""target-user@example.com"",
              ""targetPassword"": ""your-target-password"",
  
              // Connection settings
              ""timeoutSeconds"": 30,
              ""retryAttempts"": 3,
              ""retryDelaySeconds"": 5,
  
              // Logging configuration
              ""enableLogging"": true,
              ""logLevel"": ""Info"", // Debug, Info, Warning, Error
  
              // Migration behavior
              ""skipExistingCards"": false,
              ""skipExistingCollections"": false
            }";

            File.WriteAllText(configPath, template);
            System.Console.WriteLine($"✓ Created template file: {configPath}");
        }

        private static MigrationConfig CreateDefaultConfig()
        {
            return new MigrationConfig
            {
                SourceUrl = "https://source-metabase.example.com",
                TargetUrl = "https://target-metabase.example.com",
                TimeoutSeconds = 30,
                EnableLogging = true,
                LogLevel = "Info",
                RetryAttempts = 3,
                RetryDelaySeconds = 5,
                SkipExistingCards = false,
                SkipExistingCollections = false
            };
        }

        private static void OverrideWithEnvironmentVariables(MigrationConfig config)
        {
            var sourceUrl = Environment.GetEnvironmentVariable("SOURCE_METABASE_URL");
            if (!string.IsNullOrEmpty(sourceUrl)) config.SourceUrl = sourceUrl;

            var targetUrl = Environment.GetEnvironmentVariable("TARGET_METABASE_URL");
            if (!string.IsNullOrEmpty(targetUrl)) config.TargetUrl = targetUrl;

            var timeoutStr = Environment.GetEnvironmentVariable("TIMEOUT_SECONDS");
            if (int.TryParse(timeoutStr, out var timeout)) config.TimeoutSeconds = timeout;

            var loggingStr = Environment.GetEnvironmentVariable("ENABLE_LOGGING");
            if (bool.TryParse(loggingStr, out var logging)) config.EnableLogging = logging;

            var logLevel = Environment.GetEnvironmentVariable("LOG_LEVEL");
            if (!string.IsNullOrEmpty(logLevel)) config.LogLevel = logLevel;
        }

        private static string GetEnvironmentVariable(string key, string defaultValue = "")
        {
            return Environment.GetEnvironmentVariable(key) ?? defaultValue;
        }

        private static int GetEnvironmentVariableAsInt(string key, int defaultValue)
        {
            var value = Environment.GetEnvironmentVariable(key);
            return int.TryParse(value, out var result) ? result : defaultValue;
        }

        private static bool GetEnvironmentVariableAsBool(string key, bool defaultValue)
        {
            var value = Environment.GetEnvironmentVariable(key);
            return bool.TryParse(value, out var result) ? result : defaultValue;
        }
    }
}