using MetabaseMigrator.Core.Config;
using MetabaseMigrator.Core.Services;

namespace MetabaseMigrator.Console
{
    public class InteractiveConsole
    {
        private readonly MigrationService _migrationService;
        private readonly MigrationConfig _config;

        public InteractiveConsole(MigrationService migrationService, MigrationConfig config)
        {
            _migrationService = migrationService;
            _config = config;
        }

        public async Task RunAsync()
        {
            bool hasListedDashboards = false;

            while (true)
            {
                System.Console.WriteLine($"\n[{PlatformName(_config.SourceUrl)} > {PlatformName(_config.TargetUrl)}]");
                System.Console.WriteLine("Type HELP for commands.");

                var input = (System.Console.ReadLine() ?? string.Empty).Trim().ToUpperInvariant();

                switch (input)
                {
                    case "HELP":
                        PrintHelp();
                        break;

                    case "LS":
                        await _migrationService.ListSourceDashboardAsync();
                        hasListedDashboards = true;
                        break;

                    case "LT":
                        await _migrationService.ListTargetDashboardAsync();
                        hasListedDashboards = true;
                        break;

                    case "DRYCOPY":
                        if (!hasListedDashboards)
                            _migrationService.PrintError("Run LS or LT first before DRYCOPY.");
                        else
                            await _migrationService.DryCopy();
                        break;

                    case "COPY":
                        if (!hasListedDashboards)
                            _migrationService.PrintError("Run LS or LT first before COPY.");
                        else
                            await _migrationService.Copy();
                        break;

                    case "EXIT":
                        return;

                    default:
                        _migrationService.PrintError("Unknown command. Type HELP for available commands.");
                        break;
                }
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

        private static string PlatformName(string url)
        {
            Uri uri = new Uri(url);
            string host = uri.Host;
            return host.Split('.')[0]; // subdomain
        }
    }
}
