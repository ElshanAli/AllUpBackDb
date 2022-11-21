using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace AllUpBack.Controllers
{
    public class LanguagesController : Controller
    {
        public IActionResult ChangeLanguage(string culture)
        {
            Response.Cookies.Append(CookieRequestCultureProvider.DefaultCookieName, 
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions() { Expires = DateTimeOffset.UtcNow.AddMonths(1) });

            return Redirect(Request.Headers["Referer"].ToString());
        }
    }
}
