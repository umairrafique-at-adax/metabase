using System;
using System.IO;
using System.Threading.Tasks;
using MetabaseMigrator.Console.Config;
using MetabaseMigrator.Console.Services;
using MetabaseMigrator.Services;

namespace MetabaseMigrator
{
    public class Program
    {
        static async Task<int> Main(string[] args)
        {
            System.Console.WriteLine("=== Metabase Dashboard Migrator ===");
            System.Console.WriteLine("Version 1.0\n");

            try
            {
                // Parse command line arguments
                var options = ParseArguments(args);

                // Load configuration
                var config = LoadConfiguration(options);

                // Validate configuration
                var validation = config.Validate();
                if (!validation.IsValid)
                {
                    System.Console.WriteLine("❌ Configuration validation failed:");
                    System.Console.WriteLine(validation.GetErrorsAsString());
                    return 1;
                }

                // Setup logging
                var logFilePath = options.EnableFileLogging
                    ? LoggerService.CreateLogFilePath()
                    : null;
                var logger = new LoggerService(config, logFilePath);

                if (logFilePath != null)
                {
                    logger.LogInfo($"Logging to file: {logFilePath}");
                }

                // Handle special commands
                if (options.TestConnections)
                {
                    return await TestConnections(config, logger);
                }

                if (options.CreateTemplate)
                {
                    ConfigManager.CreateTemplateFile();
                    return 0;
                }

                // Get dashboard name
                string dashboardName;
                if (!string.IsNullOrEmpty(options.DashboardName))
                {
                    dashboardName = options.DashboardName;
                }
                else
                {
                    PrintHelp();


                    bool _hasListedDashboards = false;
                    var mgService = new MigrationService(config, logger);
                    while (true)
                    {
                        
                        System.Console.WriteLine($"\n[{PlatformName(config.SourceUrl)} > {PlatformName(config.TargetUrl)}]");
                        System.Console.WriteLine("Type LS for source and LT for target environment dashboards");
                        var input = System.Console.ReadLine()?.Trim() ?? string.Empty;


                        switch (input)
                        {
                            case "HELP":
                                PrintHelp();
                                break;

                            case "LS":
                                await mgService.ListSourceDashboardAsync();
                                _hasListedDashboards = true;
                                break;

                            case "LT":
                                await mgService.ListTargetDashboardAsync();
                                _hasListedDashboards = true;
                                break;

                            case "DRYCOPY":
                                if (!_hasListedDashboards)
                                {
                                    mgService.PrintError("Please run LS or LT first before attempting DRYCOPY.");
                                }
                                else
                                {
                                    await mgService.DryCopy();
                                }
                                break;

                            case "COPY":
                                if (!_hasListedDashboards)
                                {
                                    mgService.PrintError("Please run LS or LT first before attempting COPY.");
                                }
                                else
                                {
                                    await mgService.Copy();
                                }
                                break;

                            case "EXIT":
                                return 0;

                            default:
                                mgService.PrintError("Unknown command. Type HELP for a list of commands.");
                                break;
                        }
                    }



                    //System.Console.Write("Enter dashboard name to migrate: ");
                    //dashboardName = System.Console.ReadLine()?.Trim() ?? string.Empty;
                }

                //if (string.IsNullOrEmpty(dashboardName))
                //{
                //    logger.LogError("Dashboard name cannot be empty");
                //    return 1;
                //}

                //// Execute migration
                //logger.LogInfo($"Starting migration for dashboard: {dashboardName}");
                //logger.LogInfo($"Source: {config.SourceUrl}");
                //logger.LogInfo($"Target: {config.TargetUrl}");

                //using var migrationService = new MigrationService(config, logger);
                //var success = await migrationService.MigrateDashboardAsync(dashboardName);
                var success = false;
                return success ? 0 : 1;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"❌ Fatal error: {ex.Message}");
                System.Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return 1;
            }
            finally
            {
                System.Console.WriteLine("\nPress any key to exit...");

                System.Console.ReadKey();
            }
        }

        private static void PrintHelp()
        {
            System.Console.WriteLine("Available Commands:");
            System.Console.WriteLine("  HELP     - Show available commands");
            System.Console.WriteLine("  LS       - List dashboards from Source");
            System.Console.WriteLine("  LT       - List dashboards from Target");
            System.Console.WriteLine("  DRYCOPY  - Simulate dashboard migration (requires LS or LT first)");
            System.Console.WriteLine("  COPY     - Perform actual dashboard migration (requires LS or LT first)");
            System.Console.WriteLine("  EXIT     - Exit the tool");
        }
        private static CommandLineOptions ParseArguments(string[] args)
        {
            var options = new CommandLineOptions();

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i].ToLowerInvariant())
                {
                    case "--config":
                    case "-c":
                        if (i + 1 < args.Length)
                            options.ConfigPath = args[++i];
                        break;

                    case "--env":
                    case "-e":
                        options.UseEnvironmentVariables = true;
                        break;

                    case "--priority":
                    case "-p":
                        options.UsePriorityLoading = true;
                        break;

                    case "--dashboard":
                    case "-d":
                        if (i + 1 < args.Length)
                            options.DashboardName = args[++i];
                        break;

                    case "--test":
                    case "-t":
                        options.TestConnections = true;
                        break;

                    case "--template":
                        options.CreateTemplate = true;
                        break;

                    case "--log-file":
                    case "-l":
                        options.EnableFileLogging = true;
                        break;

                    case "--help":
                    case "-h":
                        ShowHelp();
                        Environment.Exit(0);
                        break;
                }
            }

            return options;
        }

        private static MigrationConfig LoadConfiguration(CommandLineOptions options)
        {
            System.Console.WriteLine("Loading configuration...");

            MigrationConfig config;

            if (options.UseEnvironmentVariables)
            {
                System.Console.WriteLine("Using environment variables");
                config = ConfigManager.LoadFromEnvironmentVariables();
            }
            else if (options.UsePriorityLoading)
            {
                System.Console.WriteLine($"Using priority loading (file + env override): {options.ConfigPath}");
                config = ConfigManager.LoadWithPriority(options.ConfigPath);
            }
            else
            {
                System.Console.WriteLine($"Using configuration file: {options.ConfigPath}");
                config = ConfigManager.LoadFromFile(options.ConfigPath);
            }

            System.Console.WriteLine("Configuration loaded successfully\n");
            return config;
        }

        private static async Task<int> TestConnections(MigrationConfig config, LoggerService logger)
        {
            logger.LogInfo("Testing connections to Metabase instances...");

            using var migrationService = new MigrationService(config, logger);
            var result = await migrationService.TestConnectionsAsync();

            return result ? 0 : 1;
        }

        private static void ShowHelp()
        {
            System.Console.WriteLine(@"
                Metabase Dashboard Migrator - Usage:

                MetabaseMigrator [options]

                OPTIONS:
                  --config, -c <path>     Specify configuration file path (default: appsettings.json)
                  --env, -e              Load configuration from environment variables only
                  --priority, -p         Load from file with environment variable overrides
                  --dashboard, -d <name>  Dashboard name to migrate (skip interactive prompt)
                  --test, -t             Test connections to both Metabase instances
                  --template             Create a configuration template file
                  --log-file, -l         Enable logging to file
                  --help, -h             Show this help message

                EXAMPLES:
                  MetabaseMigrator
                  MetabaseMigrator --config myconfig.json --dashboard ""Sales Dashboard""
                  MetabaseMigrator --env --test
                  MetabaseMigrator --priority --log-file
                  MetabaseMigrator --template

                ENVIRONMENT VARIABLES:
                  SOURCE_METABASE_URL     Source Metabase URL
                  TARGET_METABASE_URL     Target Metabase URL
                  SOURCE_USERNAME         Source username
                  SOURCE_PASSWORD         Source password
                  TARGET_USERNAME         Target username
                  TARGET_PASSWORD         Target password
                  TIMEOUT_SECONDS         HTTP timeout (default: 30)
                  ENABLE_LOGGING          Enable logging (true/false)
                  LOG_LEVEL              Log level (Debug/Info/Warning/Error)
                  RETRY_ATTEMPTS          Number of retry attempts
                  RETRY_DELAY_SECONDS     Delay between retries
                ");
        }
        private static string PlatformName(string url)
        {
            Uri uri = new Uri(url);
            string host = uri.Host;
            string subdomain = host.Split('.')[0];

            return subdomain;
        }
    }


    /// <summary>
    /// Command line options
    /// </summary>
    public class CommandLineOptions
    {
        public string ConfigPath { get; set; } = "appsettings.json";
        public bool UseEnvironmentVariables { get; set; } = false;
        public bool UsePriorityLoading { get; set; } = false;
        public string DashboardName { get; set; } = string.Empty;
        public bool TestConnections { get; set; } = false;
        public bool CreateTemplate { get; set; } = false;
        public bool EnableFileLogging { get; set; } = false;
    }
}