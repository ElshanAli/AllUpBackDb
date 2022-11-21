using AllUpBack.Areas.Admin.ViewModels;
using AllUpBack.DAL;
using AllUpBack.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AllUpBack.Controllers
{
    public class ShopController : Controller
    {
        private readonly AppDbContext _dbContext;

        public ShopController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IActionResult> Index(int? categoryId)
        {
            var mainCategories = await _dbContext.Categories
                .Where(c => !c.IsDeleted && c.IsMain)
                .Include(c => c.Children
                .Where(c => !c.IsDeleted)).ToListAsync();

            var selectedCategory = mainCategories.FirstOrDefault();

            if(categoryId is not null)
            {
                selectedCategory = mainCategories.FirstOrDefault(c => c.Id == categoryId);
                selectedCategory ??= mainCategories.SelectMany(c => c.Children).FirstOrDefault(c => c.Id == categoryId);

                if (selectedCategory is null) return NotFound();
               
            }

            var model = new ShopViewModel
            {
                SelectedCategory = selectedCategory,
                Categories = mainCategories,
            };

            return View(model);
        }
    }
}
