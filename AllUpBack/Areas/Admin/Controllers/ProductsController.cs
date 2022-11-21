using AllUpBack.Areas.Admin.Data;
using AllUpBack.Areas.Admin.Services;
using AllUpBack.Areas.Admin.ViewModels;
using AllUpBack.DAL;
using AllUpBack.DAL.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging;

namespace AllUpBack.Areas.Admin.Controllers
{
    public class ProductsController : BaseController
    {
        private readonly AppDbContext _dbContext;
        private readonly CategoryService _categoryService;
        public ProductsController(AppDbContext dbContext, CategoryService categoryService)
        {
            _dbContext = dbContext;
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _dbContext.Products
                .Include(x => x.ProductCategories)
                .ThenInclude(x=>x.Category).ToListAsync();

            return View(products);
        }

        public async Task<IActionResult> Create()
        {
            var model = await _categoryService.GetCategories();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductCreateViewModel model)
        {
            var viewModel = await _categoryService.GetCategories();

            if(!ModelState.IsValid) return View(viewModel);

            var createdProduct = new Product
            {
                Name = model.Name,
                Description = model.Description,
                Barand = model.Brand,
                ExTax = model.ExTax,
                Price = model.Price,
                Discount = model.Discount,
                Rate = model.Rate,
                ProductCategories = new List<ProductCategory>(),
                ProductImages = new List<ProductImage>()
            };

            var productImages = new List<ProductImage>();

            foreach (var image in model.Images)
            {
                if (!image.IsImage())
                {
                    ModelState.AddModelError("", "Must be selected image");
                    return View(viewModel);
                }


                if (!image.IsAllowedSize(7))
                {
                    ModelState.AddModelError("", "Image size can be max 10 mb");
                    return View(viewModel);
                }

                var unicalname = await image.Generatefile(Constants.ProductPath);
                productImages.Add(new ProductImage
                {
                    Name = unicalname,
                    ProductId = createdProduct.Id
                });
            }

            createdProduct.ProductImages.AddRange(productImages);

            var parentCategory = await _dbContext.Categories
                .Where(c => !c.IsDeleted && c.IsMain && c.Id == model.ParentCategoryId)
                .Include(c => c.Children)
                .FirstOrDefaultAsync();

            var childCategory = parentCategory.Children
                .FirstOrDefault(c => c.Id == model.ChildCategoryId);

            var productcategories = new List<ProductCategory>
            {
                new ProductCategory
                {
                    CategoryId = parentCategory.Id,
                    ProductId = createdProduct.Id
                },

                new ProductCategory
                {
                    CategoryId = childCategory.Id,
                    ProductId = createdProduct.Id
                }
            };

            createdProduct.ProductCategories.AddRange(productcategories);

            await _dbContext.Products.AddAsync(createdProduct);

            await _dbContext.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        } 

        public async Task<IActionResult> ChildCategories(int parentCategoryId)
        {
            var parentCategory = await _dbContext.Categories
                .Where(c => !c.IsDeleted && c.IsMain && c.Id == parentCategoryId)
                .Include(c => c.Children)
                .FirstOrDefaultAsync();

            var childCategoriesSelectListItems = new List<SelectListItem>();

            parentCategory?.Children.ToList()
                .ForEach(c => childCategoriesSelectListItems
          .Add(new SelectListItem(c.Name, c.Id.ToString())));

            return Json(childCategoriesSelectListItems);
        }
    }
}
