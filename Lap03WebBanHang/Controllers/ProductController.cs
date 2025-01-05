using Lap03WebBanHang.Models;
using Lap03WebBanHang.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using System.IO;
using Lap03WebBanHang.Areas.Admin.Controllers;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Lap03WebBanHang.DataAccess;
using Lap03WebBanHang.ViewModel;
namespace Lap03WebBanHang.Controllers
{



    namespace ProjectName.Controllers
    {
        // [Authorize(Roles = SD.Role_Admin)]
        public class ProductController : Controller
        {

            private readonly IOrderRepository _orderRepository;
            private readonly IProductRepository _productRepository;
            private readonly ICategoryRepository _categoryRepository;
            private readonly ApplicationDbContext _context;
            private string bookCondition;

            public ProductController( IProductRepository productRepository, ICategoryRepository categoryRepository, IOrderRepository orderRepository, ApplicationDbContext context)
            {
                _productRepository = productRepository;
                _categoryRepository = categoryRepository;
                _orderRepository = orderRepository;
                _context = context;
            }

            public IActionResult HomePage()
            {
                return View();
            }

            // Hiển thị danh sách sản phẩm
            public async Task<IActionResult> Index(int? categoryId, int page = 1, int pageSize = 6)
            {
                var products = await _productRepository.GetAllAsync();
                var categories = await _categoryRepository.GetAllAsync();

                // Lọc sản phẩm theo categoryId nếu được chọn
                if (categoryId.HasValue)
                {
                    products = products.Where(p => p.CategoryId == categoryId).ToList();
                }

                // Phân trang
                var totalProducts = products.Count();
                var totalPages = (int)Math.Ceiling((double)totalProducts / pageSize);
                var paginatedProducts = products.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                ViewBag.SelectedCategoryId = categoryId;
                ViewBag.Categories = categories;
                ViewBag.Page = page;
                ViewBag.TotalPages = totalPages;

                return View(paginatedProducts);
            }

            //// Hiển thị danh sách sản phẩm
            //public async Task<IActionResult> Index1(int? categoryId)
            //{
            //    var products = await _productRepository.GetAllAsync();
            //    var categories = await _categoryRepository.GetAllAsync();

            //    // Lọc sản phẩm theo categoryId nếu được chọn
            //    if (categoryId.HasValue)
            //    {
            //        products = products.Where(p => p.CategoryId == categoryId).ToList();
            //    }

            //    ViewBag.SelectedCategoryId = categoryId;
            //    ViewBag.Categories = categories;

            //    return View(products);
            //}







            // Hiển thị form thêm sản phẩm mới

           
            [Authorize(Roles = SD.Role_Admin)]
            public async Task<IActionResult> Add()
            {
                var categories = await _categoryRepository.GetAllAsync();
                ViewBag.Categories = new SelectList(categories, "Id", "Name");

                
                return View();
            }
            // Xử lý thêm sản phẩm mới

            [HttpPost]

            public async Task<IActionResult> Add(Product product, IFormFile imageUrl)
            {
                if (ModelState.IsValid)
                {
                    if (imageUrl != null)
                    {
                        // Lưu hình ảnh đại diện tham khảo bài 02 hàm SaveImage
                        product.ImageUrl = await SaveImage(imageUrl);
                    }


                    await _productRepository.AddAsync(product);
                    return RedirectToAction("Products", "Admin", new { area = "Admin" });
                }
                // Nếu ModelState không hợp lệ, hiển thị form với dữ liệu đã nhập
                var categories = await _categoryRepository.GetAllAsync();
                ViewBag.Categories = new SelectList(categories, "Id", "Name");
                return View(product);
            }


            // Viết thêm hàm SaveImage
            private async Task<string> SaveImage(IFormFile imageFile)
            {
                // Kiểm tra xem tệp đã được chọn chưa
                if (imageFile != null && imageFile.Length > 0)
                {
                    // Tạo tên file duy nhất
                    var fileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;

                    // Tạo đường dẫn đầy đủ lưu trữ hình ảnh
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", fileName);

                    // Lưu hình ảnh vào đường dẫn đã chọn
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    // Trả về đường dẫn của hình ảnh đã lưu
                    return "/images/" + fileName;
                }
                // Trả về null nếu không có hình ảnh được tải lên
                return null;
            }

            // Phương thức tìm kiếm
            public async Task<IActionResult> SearchProducts(string query, int page = 1, int pageSize = 10)
            {
                ViewData["Query"] = query;
                var productsQuery = _context.Products.AsQueryable();

                if (!string.IsNullOrWhiteSpace(query))
                {
                    productsQuery = productsQuery.Where(p => p.Name.ToLower().Contains(query.ToLower()));
                }

                var totalProducts = await productsQuery.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalProducts / pageSize);
                var products = await productsQuery
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var model = new ProductViewModel
                {
                    Products = products,
                    CurrentPage = page,
                    TotalPages = totalPages,
                    PageSize = pageSize
                };

                return View("ShowProduct", model);
            }


            //Danh mục sản phẩm
            // Tải danh mục vào ViewBag
            [NonAction]
            private void LoadCategoriesToViewBag()
            {
                var categories = _context.Categories.ToList();
                ViewBag.Categories = categories;
            }

            public IActionResult ProductsByCategory(int categoryId)
            {
                LoadCategoriesToViewBag(); // Gọi phương thức để load danh mục
                var products = _context.Products
                    .Where(p => p.CategoryId == categoryId)
                    .ToList();

                ViewBag.CategoryName = _context.Categories
                    .FirstOrDefault(c => c.Id == categoryId)?.Name;

                return View(products);
            }


            // Handle adding category form submission
            [HttpPost]
            [Authorize(Roles = SD.Role_Admin)]
            public async Task<IActionResult> AddCategory(Category category)
            {
                if (ModelState.IsValid)
                {
                    await _categoryRepository.AddAsync(category);
                    return RedirectToAction(nameof(Index)); // Chuyển hướng đến action Index sau khi thêm danh mục thành công
                }
                return View(category); // Nếu ModelState không hợp lệ, trả về view AddCategory với dữ liệu category đã nhập
            }


            // Hiển thị thông tin chi tiết sản phẩm
            public async Task<IActionResult> Display(int id)
            {
                var product = await _productRepository.GetByIdAsync(id);
                if (product == null)
                {
                    return NotFound();
                }
                return View(product);
            }
            // Hiển thị form cập nhật sản phẩm
            [Authorize(Roles = SD.Role_Admin)]
            public async Task<IActionResult> Update(int id)
            {
                var product = await _productRepository.GetByIdAsync(id);
                if (product == null)
                {
                    return NotFound();
                }


                var categories = await _categoryRepository.GetAllAsync();
                ViewBag.Categories = new SelectList(categories, "Id", "Name", product.CategoryId);
                return View(product);
            }
            // Xử lý cập nhật sản phẩm
            [HttpPost]
            public async Task<IActionResult> Update(int id, Product product, IFormFile imageUrl)
            {
                ModelState.Remove("ImageUrl"); // Loại bỏ xác thực ModelState cho ImageUrl
                if (id != product.Id)
                {
                    return NotFound();
                }


                if (ModelState.IsValid)
                {


                    var existingProduct = await _productRepository.GetByIdAsync(id); // Giả định có phương thức GetByIdAsync


                    // Giữ nguyên thông tin hình ảnh nếu không có hình mới được tải lên
                    if (imageUrl == null)
                    {
                        product.ImageUrl = existingProduct.ImageUrl;
                    }
                    else
                    {
                        // Lưu hình ảnh mới
                        product.ImageUrl = await SaveImage(imageUrl);
                    }
                    // Cập nhật các thông tin khác của sản phẩm
                    existingProduct.Name = product.Name;
                    existingProduct.Price = product.Price;
                    existingProduct.Description = product.Description;
                    existingProduct.CategoryId = product.CategoryId;
                    existingProduct.ImageUrl = product.ImageUrl;


                    await _productRepository.UpdateAsync(existingProduct);

                    return RedirectToAction(nameof(Index));
                }
                var categories = await _categoryRepository.GetAllAsync();
                ViewBag.Categories = new SelectList(categories, "Id", "Name");
                return View(product);
            }


            // Hiển thị form xác nhận xóa sản phẩm
            [Authorize(Roles = SD.Role_Admin)]
            public async Task<IActionResult> Delete(int id)
            {
                var product = await _productRepository.GetByIdAsync(id);
                if (product == null)
                {
                    return NotFound();
                }
                return View(product);
            }


            // Xử lý xóa sản phẩm
            [HttpPost, ActionName("DeleteConfirmed")]
            public async Task<IActionResult> DeleteConfirmed(int id)
            {
                await _productRepository.DeleteAsync(id);
                return RedirectToAction(nameof(Index));
            }

            [HttpGet]
            public async Task<IActionResult> GetSimilarProducts(int productId)
            {
                // Lấy danh sách sản phẩm cùng loại từ repository
                var similarProducts = await _productRepository.GetSimilarProductsAsync(productId);

                // Lấy chỉ tên của các sản phẩm cùng loại
                var similarProductNames = similarProducts.Select(p => p.Name);

                // Trả về danh sách tên sản phẩm cùng loại dưới dạng JSON
                return Json(similarProductNames);
            }

            public IActionResult ShowProduct(int page = 1, int pageSize = 8)
            {
                // Tổng số sản phẩm
                var totalProducts = _context.Products.Count();

                // Lấy danh sách sản phẩm cho trang hiện tại
                var products = _context.Products
                    .Skip((page - 1) * pageSize)  // Bỏ qua các sản phẩm đã hiển thị
                    .Take(pageSize)               // Lấy số sản phẩm theo trang
                    .ToList();

                // Tính tổng số trang
                var totalPages = (int)Math.Ceiling((double)totalProducts / pageSize);

                // Tạo đối tượng phân trang để truyền vào View
                var model = new ProductViewModel
                {
                    Products = products,         // Sửa lại 'Product' thành 'Products' cho đúng với ViewModel
                    CurrentPage = page,
                    TotalPages = totalPages,
                    PageSize = pageSize
                };

                return View(model);
            }



            public async Task<IActionResult> Details(long id)
            {
                var product = await _context.Products
                    .Include(p => p.Category)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (product == null)
                {
                    return NotFound();
                }

                // Lọc các sản phẩm cùng Category (ngoại trừ sản phẩm hiện tại)
                var relatedProducts = _context.Products
                    .Where(p => p.CategoryId == product.Category.Id && p.Id != product.Id)
                    .Take(4) // Giới hạn số lượng hiển thị (ví dụ: 4)
                    .ToList();

                ViewData["RelatedProducts"] = relatedProducts;

                return View(product);
            }


        }
    }
}
