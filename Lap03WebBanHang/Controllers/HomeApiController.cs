using Lap03WebBanHang.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Lap03WebBanHang.Controllers.Api
{
    [ApiController]
    [Route("api/home")]
    public class HomeApiController : ControllerBase
    {
        private readonly IProductRepository _productRepository;

        public HomeApiController(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        // API: Lấy danh sách sản phẩm bán chạy nhất kèm số lượng bán
        [HttpGet("best-selling-products")]
        public async Task<IActionResult> GetBestSellingProducts()
        {
            try
            {
                var bestSellingProducts = await _productRepository.GetBestSellingProductsAsync();

                if (!bestSellingProducts.Any())
                {
                    return NotFound(new { message = "Không có sản phẩm bán chạy." });
                }

                return Ok(bestSellingProducts);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }
    }

}
