using Lap03WebBanHang.DataAccess;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Lap03WebBanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UpdateOrderStatusController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UpdateOrderStatusController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Admin/Orders
        public async Task<IActionResult> Orders(int page = 1, int pageSize = 10)
        {
            // Đếm tổng số đơn hàng
            var totalOrders = await _context.Orders.CountAsync();

            // Tính tổng số trang
            var totalPages = (int)System.Math.Ceiling(totalOrders / (double)pageSize);

            // Lấy dữ liệu đơn hàng cho trang hiện tại
            var orders = await _context.Orders
                .OrderByDescending(o => o.Id) // Sắp xếp theo ID giảm dần
                .Skip((page - 1) * pageSize) // Bỏ qua các bản ghi trước đó
                .Take(pageSize) // Lấy số lượng bản ghi cần thiết
                .ToListAsync();

            // Truyền dữ liệu vào ViewBag để hiển thị phân trang
            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = page;

            return View(orders);
        }

        // POST: /Admin/UpdateOrderStatus
        // POST: /Admin/UpdateOrderStatus
        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(int id, string status)
        {
            // Tìm đơn hàng theo ID
            var order = await _context.Orders
                .Include(o => o.OrderDetails) // Bao gồm chi tiết đơn hàng
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return Json(new { success = false, message = "Không tìm thấy đơn hàng!" });
            }

            // Nếu trạng thái mới là 'canceled', hoàn trả số lượng vào kho
            if (status.ToLower() == "canceled")
            {
                foreach (var detail in order.OrderDetails)
                {
                    var warehouseItem = await _context.Warehouse.FirstOrDefaultAsync(w => w.ProductId == detail.ProductId);
                    if (warehouseItem != null)
                    {
                        warehouseItem.QuantityInStock += detail.Quantity; // Cộng lại số lượng
                    }
                }
            }

            // Cập nhật trạng thái giao hàng
            order.DeliveryStatus = status;

            try
            {
                // Lưu thay đổi vào cơ sở dữ liệu
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Cập nhật thành công!" });
            }
            catch
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật!" });
            }
        }
    }
}
