using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using Metabase.Web.Models;
using MetabaseMigrator.Core.Config;
using MetabaseMigrator.Core.Context;
using MetabaseMigrator.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Metabase.Web.Controllers
{
    public class DashboardController : Controller
    {
        private readonly List<MetabaseInstance> _instances;
        private readonly IMigrationServiceFactory _migrationServiceFactory;
        private readonly MigrationContext _migrationContext;
   

        public DashboardController(IMigrationServiceFactory factory, MigrationContext migrationContext, IOptions<List<MetabaseInstance>> options)
        {
            _instances = options.Value;
            _migrationServiceFactory = factory;
            _migrationContext = migrationContext;
        }

        
        public IActionResult SelectInstances()
        {
            var vm = new SelectInstancesViewModel {
                Sources = _instances,
                Targets = _instances
            };

            return View(vm);
        }

        
        [HttpPost]
        public IActionResult SelectInstances(string sourceApiUrl, string targetApiUrl)
        {
            
            var source = _instances.FirstOrDefault(s => s.Url == sourceApiUrl);
            var target = _instances.FirstOrDefault(t => t.Url == targetApiUrl);

            if (source == null || target == null)
            {
                TempData["Error"] = "Invalid source or target selection.";
                return RedirectToAction("SelectInstances");
            }


            var config = new MigrationConfig
            {
                SourceUrl = source.Url,
                SourceAPIToken = source.Token,
                TargetUrl = target.Url,
                TargetAPIToken = target.Token,
            };

            var service = _migrationServiceFactory.Create(config);
            _migrationContext.Initialize(config, service);


            return RedirectToAction("ListDashboards");
        }

        public async Task<IActionResult> ListDashboards() {

            if (_migrationContext == null) {

                return RedirectToAction("SelectInstances");
            }

            var sourceDashboards = await _migrationContext.Service.ListSourceDashboardsAsync();
            var targetDashboards = await _migrationContext.Service.ListTargetDashboardsAsync();

            var vm = new DashboardListViewModel
            {
                SourceDashboards = sourceDashboards,
                TargetDashboards = targetDashboards
            };

           return View(vm);
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
