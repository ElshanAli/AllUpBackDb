using AllUpBack.DAL.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AllUpBack.Areas.Admin.ViewModels
{
    public class ProductUpdateViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Discount { get; set; }
        public int Rate { get; set; }
        public int ExTax { get; set; }
        public string Brand { get; set; }
        public string? RemoveImageIds { get; set; }
        public ICollection<ProductImage>? ProductImages { get; set; }
        public IFormFile[] Images { get; set; }
        public List<SelectListItem>? ParentCategories { get; set; }
        public int ParentCategoryId { get; set; }
        public List<SelectListItem>? ChildCategories { get; set; }
        public int ChildCategoryId { get; set; }

    }
}
