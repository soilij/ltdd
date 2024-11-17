using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Lap03WebBanHang.Models;
using System.Threading.Tasks;
using Lap03WebBanHang.DataAccess;
using System.Linq; // Thêm using này để sử dụng LINQ

namespace Lap03WebBanHang.Controllers
{
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }


        // GET: /Order/Index
        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders.ToListAsync();
            return View(orders);
        }

        [HttpPost] // Sử dụng phương thức POST cho form
        public async Task<IActionResult> Detail(int orderId)
        {
            var orderDetails = await _context.OrderDetails
                .Where(od => od.OrderId == orderId)
                .Include(od => od.Product) // Bổ sung để nạp thông tin sản phẩm liên quan
                .ToListAsync();

            return View(orderDetails);
        }
    }
}
