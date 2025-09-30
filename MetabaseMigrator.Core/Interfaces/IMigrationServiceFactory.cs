using MetabaseMigrator.Core.Config;
using MetabaseMigrator.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetabaseMigrator.Core.Interfaces
{
    public interface IMigrationServiceFactory
    {
        MigrationService Create(MigrationConfig config);


    }
}
