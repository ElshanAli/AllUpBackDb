using AllUpBack.Areas.Admin.Data;
using AllUpBack.Areas.Admin.ViewModels;
using AllUpBack.DAL;
using AllUpBack.DAL.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AllUpBack.Areas.Admin.Controllers
{
    public class CategoriesController : BaseController
    {
        private readonly AppDbContext _dbContext;

        public CategoriesController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _dbContext.Categories.ToListAsync();
            return View(categories);
        }
   
        public async Task<IActionResult> Create()
        {
            var categories = await _dbContext.Categories
                .Where(x => !x.IsDeleted && x.IsMain).ToListAsync();
            var categoryListItem = new List<SelectListItem>
            {
                new SelectListItem("Select Parent Category", "0")
            };

            categories.ForEach(x => categoryListItem.Add(new SelectListItem(x.Name, x.Id.ToString())));
           

            var model = new CategoryCreateViewModel()
            {
                ParentCategories = categoryListItem
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryCreateViewModel model)
        {
            var parenCategories = await _dbContext.Categories
                .Where(x => !x.IsDeleted && x.IsMain)
                .Include(x => x.Children).ToListAsync();
            var categoryListItem = new List<SelectListItem>
            {
                new SelectListItem("Select Parent Category", "0")
            };

            parenCategories.ForEach(x => categoryListItem.Add(new SelectListItem(x.Name, x.Id.ToString())));


            var viewModel = new CategoryCreateViewModel()
            {
                ParentCategories = categoryListItem
            };

            if (!ModelState.IsValid) return View(viewModel);

            var createdCategory = new Category();

            if (model.IsMain)
            {
                
                if (!model.Image.IsImage())
                {
                    ModelState.AddModelError("", "Must be selected image");
                    return View(viewModel);
                }


                if (!model.Image.IsAllowedSize(7))
                {
                    ModelState.AddModelError("", "Image size can be max 10 mb");
                    return View(viewModel);
                }

                if (parenCategories.Any(x => x.Name.ToLower().Equals(model.Name.ToLower())))
                {
                    ModelState.AddModelError("", "There is a category with this name");
                    return View(viewModel);
                }
                var unicalname = await model.Image.Generatefile(Constants.CategoryPath);
                createdCategory.ImageUrl = unicalname;
            }
            else
            {
                if (model.ParentId == 0)
                {
                    ModelState.AddModelError("", "Must be choose Parent Category");
                    return View(viewModel);
                }

                var parentCategory = parenCategories.FirstOrDefault(c => c.Id == model.ParentId);

                if (parentCategory.Children.Any(x => x.Name.ToLower().Equals(model.Name.ToLower())))
                {
                    ModelState.AddModelError("", "There is a subcategory with this name");
                    return View(viewModel);
                }
                createdCategory.ImageUrl = "";

                createdCategory.ParentId = model.ParentId;
            }
            createdCategory.Name = model.Name;
            createdCategory.IsMain = model.IsMain;
            createdCategory.IsDeleted = false;

            await _dbContext.Categories.AddAsync(createdCategory);
            await _dbContext.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
