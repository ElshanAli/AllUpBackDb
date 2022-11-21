using Microsoft.AspNetCore.Mvc;

namespace AllUpBack.Controllers
{
    public class CategoriesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
