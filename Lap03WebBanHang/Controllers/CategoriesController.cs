using Lap03WebBanHang.Areas.Admin.Controllers;
using Lap03WebBanHang.DataAccess;
using Lap03WebBanHang.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Lap03WebBanHang.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        public CategoriesController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public IActionResult HomePage()
        {
            return View();
        }
        public IActionResult Index()
        {
            var categories = _dbContext.Categories.ToList();

            foreach (var category in categories)
            {
                category.Products = GetProductsForCategory(category.Id);
            }

            return View(categories);
        }
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Add(Category category)
        {
            if (ModelState.IsValid)
            {
                _dbContext.Categories.Add(category);
                _dbContext.SaveChanges();

                return RedirectToAction(nameof(Index));
            }

            return View(category);
        }
        // Hiển thị form xác nhận xóa sản phẩm
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _dbContext.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // Xử lý xóa danh mục
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = SD.Role_Admin)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _dbContext.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            // Xóa danh mục và lưu thay đổi vào cơ sở dữ liệu
            _dbContext.Categories.Remove(category);
            await _dbContext.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


    private List<Product> GetProductsForCategory(int categoryId)
        {
            return _dbContext.Products.Where(p => p.CategoryId == categoryId).ToList();
        }
    }
}
