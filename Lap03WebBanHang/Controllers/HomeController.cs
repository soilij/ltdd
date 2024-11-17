using Lap03WebBanHang.Models;
using Lap03WebBanHang.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Lap03WebBanHang.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductRepository _productRepository;

        public HomeController(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<IActionResult> Index(int? pageNumber)
        {
            int pageSize = 8; // Số lượng sản phẩm trên mỗi trang
            pageNumber ??= 1;

            var products = await _productRepository.GetAllAsync();
            int totalProducts = products.Count();
            int totalPages = (int)Math.Ceiling((double)totalProducts / pageSize);

            if (pageNumber < 1 || pageNumber > totalPages)
            {
                return NotFound(); // Trang không tồn tại
            }

            var paginatedProducts = products.Skip(((int)pageNumber - 1) * pageSize).Take(pageSize).ToList();

            // Lấy danh sách sản phẩm bán chạy
            var bestSellingProducts = await _productRepository.GetBestSellingProductsAsync();

            // Truyền dữ liệu cần thiết cho view
            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = pageNumber;
            ViewBag.BestSellingProducts = bestSellingProducts;

            return View(paginatedProducts);
        }


      

        public IActionResult DisplayProducts()
        {
            var products = _productRepository.GetAllAsync();
            return View(products);
        }
    }
}
