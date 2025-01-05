using Lap03WebBanHang.DataAccess;
using Lap03WebBanHang.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Lap03WebBanHang.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WarehouseApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public WarehouseApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. POST: Thêm sản phẩm vào kho
        [HttpPost("import")]
        public async Task<IActionResult> ImportStock([FromBody] ImportStockRequest request)
        {
            if (request == null || request.Quantity <= 0)
            {
                return BadRequest(new { message = "Thông tin nhập kho không hợp lệ." });
            }

            var warehouseItem = await _context.Warehouse
                .FirstOrDefaultAsync(w => w.ProductId == request.ProductId && w.SupplierId == request.SupplierId);

            if (warehouseItem != null)
            {
                warehouseItem.QuantityInStock += request.Quantity;
                warehouseItem.LastUpdated = DateTime.Now;

                await _context.SaveChangesAsync();

                return Ok(new { message = "Cập nhật số lượng sản phẩm vào kho thành công." });
            }
            else
            {
                return NotFound(new { message = "Sản phẩm không có trong kho." });
            }
        }

        // 2. GET: Lấy tất cả sản phẩm trong kho
        [HttpGet("all")]
        public async Task<IActionResult> GetAllProducts()
        {
            var products = await _context.Warehouse
                .Include(w => w.Product)
                .Select(w => new
                {
                    w.ProductId,
                    w.SupplierId,
                    w.QuantityInStock,
                    ProductName = w.Product.Name,
                    ImageUrl = w.Product.ImageUrl
                })
                .ToListAsync();

            if (products == null || products.Count == 0)
            {
                return NotFound(new { message = "Không có sản phẩm trong kho." });
            }

            return Ok(products);
        }

        // 3. GET: Lấy thông tin sản phẩm theo ProductId và SupplierId
        [HttpGet("product/{productId}/supplier/{supplierId}")]
        public async Task<IActionResult> GetProductById(int productId, int supplierId)
        {
            var warehouseItem = await _context.Warehouse
                .Include(w => w.Product)
                .FirstOrDefaultAsync(w => w.ProductId == productId && w.SupplierId == supplierId);

            if (warehouseItem == null)
            {
                return NotFound(new { message = "Sản phẩm không có trong kho." });
            }

            var result = new
            {
                warehouseItem.ProductId,
                warehouseItem.SupplierId,
                warehouseItem.QuantityInStock,
                ProductName = warehouseItem.Product.Name,
                ImageUrl = warehouseItem.Product.ImageUrl
            };

            return Ok(result);
        }

        // 4. PUT: Cập nhật số lượng sản phẩm
        // 4. PUT: Cập nhật số lượng sản phẩm
        [HttpPut("update-quantity")]
        public async Task<IActionResult> UpdateProductQuantity([FromBody] UpdateQuantityRequest request)
        {
            if (request == null || request.NewQuantity <= 0)
            {
                return BadRequest(new { message = "Số lượng cần thêm không hợp lệ." });
            }

            var product = await _context.Warehouse
                .FirstOrDefaultAsync(w => w.ProductId == request.ProductId && w.SupplierId == request.SupplierId);

            if (product == null)
            {
                return NotFound(new { message = "Sản phẩm không tồn tại trong kho." });
            }

            // Thay đổi logic: Cộng thêm số lượng mới vào số lượng hiện tại
            product.QuantityInStock += request.NewQuantity;
            product.LastUpdated = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Cộng thêm số lượng sản phẩm thành công." });
        }

    }

    // Request Model cho việc nhập sản phẩm vào kho
    public class ImportStockRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public int SupplierId { get; set; }
    }

    // Request Model cho việc cập nhật số lượng sản phẩm
    public class UpdateQuantityRequest
    {
        public int ProductId { get; set; }
        public int SupplierId { get; set; }
        public int NewQuantity { get; set; }
    }
}
