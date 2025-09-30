using MetabaseMigrator.Core.Config;
using MetabaseMigrator.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetabaseMigrator.Core.Services
{
    public class MigrationServiceFactory:IMigrationServiceFactory
    {
        public MigrationService Create(MigrationConfig config) { 
        
            var logger = new LoggerService(config);
            return new MigrationService(config,logger);
            
        }

    }
}
