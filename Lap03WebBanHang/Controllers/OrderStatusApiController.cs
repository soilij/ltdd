using Lap03WebBanHang.DataAccess;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Lap03WebBanHang.API
{
    [ApiController]
    [Route("api/orders")]
    public class OrderStatusApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OrderStatusApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // API: Cập nhật trạng thái đơn hàng
        [HttpPost("update-status")]
        public async Task<IActionResult> UpdateOrderStatus([FromBody] UpdateOrderStatusRequest request)
        {
            // Tìm đơn hàng theo ID
            var order = await _context.Orders
                .Include(o => o.OrderDetails) // Bao gồm chi tiết đơn hàng
                .FirstOrDefaultAsync(o => o.Id == request.OrderId);

            if (order == null)
            {
                return BadRequest(new { success = false, message = "Không tìm thấy đơn hàng!" });
            }

            // Nếu trạng thái mới là 'canceled', hoàn trả số lượng vào kho
            if (request.Status.ToLower() == "canceled")
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
            order.DeliveryStatus = request.Status;

            try
            {
                // Lưu thay đổi vào cơ sở dữ liệu
                await _context.SaveChangesAsync();
                return Ok(new { success = true, message = "Cập nhật trạng thái thành công!" });
            }
            catch
            {
                return StatusCode(500, new { success = false, message = "Có lỗi xảy ra khi cập nhật!" });
            }
        }
    }

    // Lớp DTO để nhận dữ liệu từ yêu cầu
    public class UpdateOrderStatusRequest
    {
        public int OrderId { get; set; }
        public string Status { get; set; }
    }
}
