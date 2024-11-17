using Lap03WebBanHang.Models;
using Lap03WebBanHang.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using System.IO;
using Lap03WebBanHang.Areas.Admin.Controllers;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
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
            private string bookCondition;

            public ProductController(IProductRepository productRepository, ICategoryRepository categoryRepository, IOrderRepository orderRepository)
            {
                _productRepository = productRepository;
                _categoryRepository = categoryRepository;
                _orderRepository = orderRepository;
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

            // Hiển thị danh sách sản phẩm
            public async Task<IActionResult> Index1(int? categoryId)
            {
                var products = await _productRepository.GetAllAsync();
                var categories = await _categoryRepository.GetAllAsync();

                // Lọc sản phẩm theo categoryId nếu được chọn
                if (categoryId.HasValue)
                {
                    products = products.Where(p => p.CategoryId == categoryId).ToList();
                }

                ViewBag.SelectedCategoryId = categoryId;
                ViewBag.Categories = categories;

                return View(products);
            }







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
                    return RedirectToAction(nameof(Index));
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

            //Tìm kiếm
            public async Task<IActionResult> SearchProducts(string query)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(query))
                        return BadRequest("Search query is required.");

                    var result = await _productRepository.SearchProductsAsync(query);
                    // Include thông tin về category khi tìm kiếm
                    foreach (var product in result)
                    {
                        product.Category = await _categoryRepository.GetByIdAsync(product.CategoryId);
                    }
                    ViewBag.Categories = await _categoryRepository.GetAllAsync(); // Gán giá trị cho ViewBag.Categories
                    return View("Index", result);
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }


            //Danh mục sản phẩm
            public async Task<IActionResult> ProductsByCategory(int categoryId)
            {
                var productsInCategory = await _productRepository.GetProductsByCategoryAsync(categoryId);
                return View("Index", productsInCategory);
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



        }
    }
}
