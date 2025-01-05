using Lap03WebBanHang.Models;

namespace Lap03WebBanHang.ViewModel
{
    public class HomeViewModels
    {
        public List<Category> Categories { get; set; } = new List<Category>();
        public List<Product> Products { get; set; } = new List<Product>();
    }
}
