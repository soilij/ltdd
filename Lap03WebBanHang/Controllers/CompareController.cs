using Lap03WebBanHang.Models;
using Lap03WebBanHang.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lap03WebBanHang.Controllers
{
    public class CompareController : Controller
    {
        private readonly IProductRepository _productRepository;

        public CompareController(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        // Hiển thị form so sánh sản phẩm
        public async Task<IActionResult> Index()
        {
            var products = await _productRepository.GetAllAsync();
            var viewModel = new ProductComparisonViewModel
            {
                Products = products
            };
            return View(viewModel);
        }

        // Xử lý so sánh sản phẩm
        [HttpPost]
        public async Task<IActionResult> Compare(int productId1, int productId2)
        {
            var product1 = await _productRepository.GetByIdAsync(productId1);
            var product2 = await _productRepository.GetByIdAsync(productId2);

            if (product1 == null || product2 == null)
            {
                return NotFound();
            }

            var viewModel = new ProductComparisonViewModel
            {
                Product1 = product1,
                Product2 = product2
            };

            return View("Index", viewModel);
        }

        // Lấy chi tiết sản phẩm
        public async Task<IActionResult> GetProductDetails(int productId)
        {
            var product = await _productRepository.GetByIdAsync(productId);

            if (product == null)
            {
                return NotFound();
            }

            return PartialView("_ProductDetails", product);
        }
    }

    public class ProductComparisonViewModel
    {
        public IEnumerable<Product> Products { get; set; }
        public Product Product1 { get; set; }
        public Product Product2 { get; set; }
    }

}
