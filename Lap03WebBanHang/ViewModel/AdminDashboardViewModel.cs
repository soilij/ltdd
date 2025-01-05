using Lap03WebBanHang.Models;

namespace Lap03WebBanHang.ViewModel
{
    public class AdminDashboardViewModel
    {
        public IEnumerable<Product> Products { get; set; }
        public IEnumerable<Order> Orders { get; set; }
    }
}
