using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Lap03WebBanHang.DataAccess;
using Lap03WebBanHang.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Lap03WebBanHang.Repositories;
using Microsoft.AspNetCore.Authorization;

namespace Lap03WebBanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AllowAnonymous] // Nếu bạn muốn yêu cầu người dùng phải đăng nhập, thay [AllowAnonymous] bằng [Authorize]
    [Route("api/admin/stats")]
    [ApiController]
    public class StatsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IProductRepository _productRepository;

        public StatsApiController(ApplicationDbContext context, IProductRepository productRepository)
        {
            _context = context;
            _productRepository = productRepository;
        }

        // API thống kê doanh thu trong khoảng thời gian
        [HttpGet("revenue")]
        public async Task<IActionResult> GetRevenueData(DateTime startDate, DateTime endDate)
        {
            try
            {
                // Kiểm tra nếu startDate lớn hơn endDate
                if (startDate.CompareTo(endDate) > 0)
                {
                    return BadRequest(new { error = "Ngày bắt đầu không được lớn hơn ngày kết thúc." });
                }

                // Lấy các đơn hàng hoàn thành trong khoảng thời gian
                var orders = await _context.Orders
                    .Where(o => o.DeliveryStatus == "Completed"
                             && o.OrderDate >= startDate
                             && o.OrderDate <= endDate)
                    .ToListAsync();

                // Kiểm tra nếu không có đơn hàng nào
                if (!orders.Any())
                {
                    return Ok(new { message = "Không có hóa đơn nào trong khoảng thời gian này." });
                }

                // Tính doanh thu theo ngày
                var revenueData = orders
                    .GroupBy(o => o.OrderDate.Date)
                    .Select(g => new
                    {
                        Date = g.Key.ToString("dd-MM-yyyy"),
                        Revenue = g.Sum(o => o.TotalPrice)
                    })
                    .OrderBy(g => g.Date)
                    .ToList();

                return Ok(revenueData); // Trả về dữ liệu doanh thu
            }
            catch (Exception ex)
            {
                // Xử lý lỗi trong quá trình thực thi
                return BadRequest(new { error = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }

        // API thống kê tổng số sản phẩm
        [HttpGet("total-products")]
        public async Task<IActionResult> GetTotalProducts()
        {
            try
            {
                // Lấy số lượng sản phẩm
                var totalProducts = await _productRepository.GetAllAsync();
                return Ok(new { TotalProducts = totalProducts.Count() });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }

        // API thống kê tổng số sản phẩm tồn kho
        [HttpGet("total-inventory")]
        public async Task<IActionResult> GetTotalInventory()
        {
            try
            {
                // Tính tổng số sản phẩm tồn kho
                var totalInventoryProducts = await _context.Warehouse.SumAsync(w => w.QuantityInStock);
                return Ok(new { TotalInventoryProducts = totalInventoryProducts });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }

        // API thống kê tổng số đơn hàng
        [HttpGet("total-orders")]
        public async Task<IActionResult> GetTotalOrders()
        {
            try
            {
                // Tính tổng số đơn hàng
                var totalOrders = await _context.Orders.CountAsync();
                return Ok(new { TotalOrders = totalOrders });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }
    }
}
                    