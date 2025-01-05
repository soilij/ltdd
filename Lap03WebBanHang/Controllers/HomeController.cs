using Lap03WebBanHang.Models;
using Lap03WebBanHang.Repositories;
using Lap03WebBanHang.ViewModel;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Lap03WebBanHang.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository; // Thêm repository cho danh mục

        public HomeController(IProductRepository productRepository, ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task<IActionResult> Index(int? pageNumber)
        {
            int pageSize = 8; // Số lượng sản phẩm trên mỗi trang
            pageNumber ??= 1;

            // Lấy tất cả sản phẩm
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

            // Lấy danh sách danh mục
            var categories = await _categoryRepository.GetAllAsync();

            // Tạo HomeViewModel và truyền dữ liệu
            var homeViewModel = new HomeViewModels
            {
                Products = paginatedProducts,
                Categories = categories.ToList()
            };

            // Truyền thêm thông tin phân trang qua ViewBag
            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = pageNumber;
            ViewBag.BestSellingProducts = bestSellingProducts;
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.ErrorMessage = TempData["ErrorMessage"];

            return View(homeViewModel);
        }
        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }
    }
}
