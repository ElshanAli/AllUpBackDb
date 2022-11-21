using AllUpBack.DAL.Entities;

namespace AllUpBack.ViewModels
{
    public class ShopViewModel
    {
        public Category SelectedCategory { get; set; }
        public List<Category> Categories { get; set; }
    }
}
