using AllUpBack.Areas.Admin.Data;
using AllUpBack.Areas.Admin.Services;
using AllUpBack.Areas.Admin.ViewModels;
using AllUpBack.DAL;
using AllUpBack.DAL.Entities;
using AllUpBack.Data;
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
                .ThenInclude(x => x.Category).ToListAsync();

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

            if (!ModelState.IsValid) return View(viewModel);

            var product = new Product
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
                    ProductId = product.Id
                });
            }

            product.ProductImages.AddRange(productImages);

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
                    ProductId = product.Id,
                    CategoryId = parentCategory.Id
                },

                new ProductCategory
                {
                    ProductId = product.Id,
                    CategoryId = childCategory.Id
                }
            };

            product.ProductCategories.AddRange(productcategories);

            await _dbContext.Products.AddAsync(product);

            await _dbContext.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Update(int? id)
        {
            if (id is null) return BadRequest();
            var product = await _dbContext.Products
                .Where(p => p.Id == id)
                .Include(p => p.ProductImages)
                .Include(p => p.ProductCategories)
                .ThenInclude(c => c.Category)
                .FirstOrDefaultAsync();

            if (product is null) return NotFound();

            var parentCategory = product.ProductCategories
                .Where(n => n.Category.IsMain)
                .First();

            var childCategory = product.ProductCategories
                .Where(n => !n.Category.IsMain)
                .First();

            var categories = await _dbContext.Categories
                .Where(n => !n.IsDeleted && n.IsMain)
                .Include(n => n.Children)
                .ToListAsync();

            var parentCategories = new List<SelectListItem>();
            var childCategories = new List<SelectListItem>();

            categories
                .ForEach(c => parentCategories
            .Add(new SelectListItem(c.Name, c.Id.ToString())));

            parentCategory.Category.Children
                .ToList()
                .ForEach(c => childCategories
                .Add(new SelectListItem(c.Name, c.Id.ToString())));

            var productUpdateViewModel = new ProductUpdateViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Discount = product.Discount,
                Brand = product.Barand,
                Description = product.Description,
                Rate = product.Rate,
                ExTax = product.ExTax,
                Price = product.Price,
                ProductImages = product.ProductImages,
                ParentCategoryId = parentCategory.CategoryId,
                ParentCategories = parentCategories,
                ChildCategoryId = childCategory.CategoryId,
                ChildCategories = childCategories
            };
            return View(productUpdateViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int? id, ProductUpdateViewModel model)
        {

            if (id is null) return BadRequest();
            var product = await _dbContext.Products
                .Where(p => p.Id == id)
                .Include(p => p.ProductImages)
                .Include(p => p.ProductCategories)
                .ThenInclude(c => c.Category)
                .FirstOrDefaultAsync();

            if (product is null) return NotFound();

            if (!ModelState.IsValid)
            {
                    var errorList = ModelState.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                      );
                return Ok(errorList);
            }
            var productImages = new List<ProductImage>();

            foreach (var image in model.Images)
            {
                if (!image.IsImage())
                {
                    return Json("Must be selected image");
                }

                if (!image.IsAllowedSize(7))
                {
                    return Json("Image size can be max 10 mb");
                }

                var unicalname = await image.Generatefile(Constants.ProductPath);
                productImages.Add(new ProductImage
                {
                    Name = unicalname,
                    ProductId = product.Id
                });
            }

            product.ProductImages.AddRange(productImages);

            if (model.RemoveImageIds is not null)
            {
                RemoveImageIds(model.RemoveImageIds);
            }

            var productcategories = new List<ProductCategory>
            {
                new ProductCategory
                {
                    ProductId = product.Id,
                    CategoryId = model.ParentCategoryId
                },

                new ProductCategory
                {
                    ProductId = product.Id,
                    CategoryId = model.ChildCategoryId
                }
            };

            product.ProductCategories = productcategories;

            product.Name = model.Name;
            product.Description = model.Description;
            product.Rate = model.Rate;
            product.Price = model.Price;
            product.Barand = model.Brand;
            product.ExTax = model.ExTax;
            product.Discount = model.Discount;

            await _dbContext.SaveChangesAsync();

            return Ok(model);
        }

        private async void RemoveImageIds(string imageIds)
        {

            var removedImageIds = imageIds
                .Split(",")
                .ToList()
                .Select(imageId => Int32.Parse(imageId));

            var productImages = await _dbContext.ProductImages.Where(prIm => removedImageIds.Contains(prIm.Id)).ToListAsync();

            _dbContext.ProductImages.RemoveRange(productImages);

            foreach (var productImage in productImages)
            {
                var imagePath = Path.Combine(Constants.ProductPath, productImage.Name);

                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }
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
