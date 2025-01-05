using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Lap03WebBanHang.Models;
using Lap03WebBanHang.DataAccess;
using System.Linq;
using System.Threading.Tasks;

namespace Lap03WebBanHang.Areas.Api.Controllers
{
    [Route("api/order-history")]
    [ApiController]
    public class OrderHistoryApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OrderHistoryApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Lấy toàn bộ lịch sử đơn hàng, với tùy chọn lọc theo trạng thái
        [HttpGet]
        public async Task<IActionResult> GetOrderHistory(string? status)
        {
            // Khởi tạo truy vấn lấy đơn hàng
            var ordersQuery = _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .OrderByDescending(o => o.OrderDate)
                .AsQueryable();

            // Nếu có tham số status, lọc theo trạng thái đơn hàng
            if (!string.IsNullOrEmpty(status))
            {
                ordersQuery = ordersQuery.Where(o => o.DeliveryStatus == status);
            }

            // Lấy danh sách đơn hàng
            var orders = await ordersQuery.ToListAsync();

            if (orders == null || !orders.Any())
            {
                return NotFound(new { message = "No orders found" });
            }

            // Trả về dữ liệu lịch sử giao dịch
            var orderHistory = orders.Select(o => new
            {
                OrderId = o.Id,
                OrderDate = o.OrderDate.ToString("dd-MM-yyyy"),
                TotalAmount = o.TotalPrice,
                OrderDetails = o.OrderDetails.Select(od => new
                {
                    ProductName = od.Product.Name,
                    Quantity = od.Quantity,
                    UnitPrice = od.Product.Price,
                    TotalItemPrice = od.Quantity * od.Product.Price
                })
            });

            return Ok(orderHistory); // Trả về lịch sử giao dịch dưới dạng JSON
        }

        // Lấy chi tiết đơn hàng theo id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderDetails(int id)
        {
            // Lấy đơn hàng chi tiết theo id
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound(new { message = "Order not found" });
            }

            // Trả về chi tiết đơn hàng
            var orderDetails = new
            {
                OrderId = order.Id,
                OrderDate = order.OrderDate.ToString("dd-MM-yyyy"),
                TotalAmount = order.TotalPrice,
                OrderDetails = order.OrderDetails.Select(od => new
                {
                    ProductName = od.Product.Name,
                    Quantity = od.Quantity,
                    UnitPrice = od.Product.Price,
                    TotalItemPrice = od.Quantity * od.Product.Price
                })
            };

            return Ok(orderDetails); // Trả về chi tiết đơn hàng dưới dạng JSON
        }
    }
}
