using Microsoft.AspNetCore.Mvc;

namespace Metabase.Web.Controllers
{
    public class MetabaseController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
