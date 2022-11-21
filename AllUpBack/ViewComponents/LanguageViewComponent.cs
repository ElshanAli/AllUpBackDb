using AllUpBack.DAL;
using AllUpBack.ViewModels;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AllUpBack.ViewComponents
{
    public class LanguageViewComponent : ViewComponent
    {
        private readonly AppDbContext _dbcontext;

        public LanguageViewComponent(AppDbContext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var culture = Request.Cookies[CookieRequestCultureProvider.DefaultCookieName];
            var isoCode = culture?.Substring(culture.IndexOf("uic=") + 4) ?? "en-US";
            var language = await _dbcontext.Languages.ToListAsync();
            var selectedLanguage = language.FirstOrDefault(x => x.IsoCode.ToLower().Equals(isoCode.ToLower()));
            var model = new LanguageViewModel
            {
                Languages = language,
                SelectedLanguage = selectedLanguage

            };
            return View(model);
        }
    }
}
