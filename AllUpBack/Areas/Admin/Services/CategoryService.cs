using AllUpBack.Areas.Admin.ViewModels;
using AllUpBack.DAL;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AllUpBack.Areas.Admin.Services
{
    public class CategoryService
    {
        private readonly AppDbContext _dbContext;

        public CategoryService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ProductCreateViewModel> GetCategories()
        {
            var categories = await _dbContext.Categories
                .Where(c => !c.IsDeleted && c.IsMain)
                .Include(c => c.Children)
                .ToListAsync();

            var parentCategoriesSelectListItems = new List<SelectListItem>();
            var childCategoriesSelectListItems = new List<SelectListItem>();

            categories.ForEach(c => parentCategoriesSelectListItems
            .Add(new SelectListItem(c.Name, c.Id.ToString())));

            categories[0].Children.ToList()
                .ForEach(c => childCategoriesSelectListItems
          .Add(new SelectListItem(c.Name, c.Id.ToString())));

            var model = new ProductCreateViewModel
            {
                ParentCategories = parentCategoriesSelectListItems,
                ChildCategories = childCategoriesSelectListItems
            };
            return model;
        }
    }
}
