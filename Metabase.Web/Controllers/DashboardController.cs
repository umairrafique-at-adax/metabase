using Metabase.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Metabase.Web.Controllers
{
    public class DashboardController : Controller
    {
        private readonly List<MetabaseInstance> _instances;

        public DashboardController(IOptions<List<MetabaseInstance>> options)
        {
            _instances = options.Value;
        }

        // GET: /Dashboard/SelectInstances
        public IActionResult SelectInstances()
        {
            // Send same list for both dropdowns
            ViewBag.Sources = _instances;
            ViewBag.Targets = _instances;
            return View();
        }

        // POST: /Dashboard/SelectInstances
        [HttpPost]
        public IActionResult SelectInstances(string sourceApiUrl, string targetApiUrl)
        {
            // Find chosen instances
            var source = _instances.FirstOrDefault(s => s.BaseUrl == sourceApiUrl);
            var target = _instances.FirstOrDefault(t => t.BaseUrl == targetApiUrl);

            if (source == null || target == null)
            {
                TempData["Error"] = "Invalid source or target selection.";
                return RedirectToAction("SelectInstances");
            }

            // Later: call your console app migrator logic here
            // using source.ApiKey and target.ApiKey

            return RedirectToAction("ListDashboards",
                new { sourceUrl = source.BaseUrl, targetUrl = target.BaseUrl });
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
