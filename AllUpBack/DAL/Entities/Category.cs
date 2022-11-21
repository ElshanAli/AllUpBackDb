using System.ComponentModel.DataAnnotations.Schema;

namespace AllUpBack.DAL.Entities
{
    public class Category : Entity
    {
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public bool IsMain { get; set; }
        public Category Parent { get; set; }
        public int? ParentId { get; set; }
        [ForeignKey("ParentId")]
        public ICollection<Category> Children { get; set; }
        public ICollection<ProductCategory> ProductCategories { get; set; }
    }
}
