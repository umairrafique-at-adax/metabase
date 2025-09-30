using MetabaseMigrator.Core.Models;

namespace Metabase.Web.Models
{
    public class DashboardListViewModel
    {
        public List<MetabaseDashboard> SourceDashboards { get; set; }
        public List<MetabaseDashboard> TargetDashboards { get; set; }


    }
}
