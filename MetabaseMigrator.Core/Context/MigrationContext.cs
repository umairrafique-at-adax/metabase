using MetabaseMigrator.Core.Config;
using MetabaseMigrator.Core.Services;

namespace MetabaseMigrator.Core.Context
{
    public class MigrationContext
    {
        public MigrationConfig? Config { get; private set; }
        public MigrationService? Service { get; private set; }


        public void Initialize(MigrationConfig config, MigrationService service) { 
            Config = config;
            Service = service;
        }

        public void Reset() { 
            Config = null;
            Service = null;
        }

    }
}
