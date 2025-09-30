using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetabaseMigrator.Core.Models
{
    public enum MigrationActions
    {
        None = 0,
        New,
        Skip,
        Override
    }

}
