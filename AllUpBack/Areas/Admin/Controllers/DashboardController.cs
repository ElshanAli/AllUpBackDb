using Microsoft.AspNetCore.Mvc;

namespace AllUpBack.Areas.Admin.Controllers
{
    public class DashboardController : BaseController
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
