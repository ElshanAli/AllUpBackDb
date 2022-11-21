using AllUpBack.Areas.Admin.Data;
using AllUpBack.Areas.Admin.ViewModels;
using AllUpBack.DAL;
using AllUpBack.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AllUpBack.Areas.Admin.Controllers
{
    public class LanguagesController : BaseController
    {
        private readonly AppDbContext _dbContext;

        public LanguagesController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IActionResult> Index()
        {
            var languages = await _dbContext.Languages
                .Where(x => !x.IsDeleted).ToListAsync();

            return View(languages);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LanguageCreateViewModel model)
        {
            if (!ModelState.IsValid) return View();

            if (!model.Image.IsImage())
            {
                ModelState.AddModelError("", "Must be selected image");
                return View();
            }


            if (!model.Image.IsAllowedSize(7))
            {
                ModelState.AddModelError("", "Image size can be max 10 mb");
                return View();
            }

            var unicalname = await model.Image.Generatefile(Constants.FlagPath);

            await _dbContext.Languages.AddAsync(new DAL.Entities.Language
            {
                ImageUrl = unicalname,
                Name = model.Name,
                IsoCode = model.IsoCode,

            });

            await _dbContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

        }
    }
}
