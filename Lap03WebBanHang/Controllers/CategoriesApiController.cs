using Lap03WebBanHang.DataAccess;
using Lap03WebBanHang.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Lap03WebBanHang.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesApiController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public CategoriesApiController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Lấy tất cả danh mục
        [HttpGet]
        public IActionResult GetCategories()
        {
            var categories = _dbContext.Categories.ToList();
            foreach (var category in categories)
            {
                category.Products = GetProductsForCategory(category.Id); // Tải sản phẩm cho mỗi danh mục
            }

            return Ok(categories);
        }

        // Lấy danh mục theo ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategory(int id)
        {
            var category = await _dbContext.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            category.Products = GetProductsForCategory(category.Id); // Tải sản phẩm cho danh mục
            return Ok(category);
        }

        // Thêm mới danh mục
        [HttpPost]
        [Authorize(Roles = "Admin")] // Chỉ admin mới có quyền thêm danh mục
        public async Task<IActionResult> AddCategory([FromBody] Category category)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _dbContext.Categories.Add(category);
            await _dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category);
        }

        // Cập nhật danh mục
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")] // Chỉ admin mới có quyền sửa danh mục
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] Category category)
        {
            if (id != category.Id)
            {
                return BadRequest();
            }

            _dbContext.Entry(category).State = EntityState.Modified;

            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoryExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // Xóa danh mục
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // Chỉ admin mới có quyền xóa danh mục
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _dbContext.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            _dbContext.Categories.Remove(category);
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }

        // Hàm lấy danh sách sản phẩm cho một danh mục
        private List<Product> GetProductsForCategory(int categoryId)
        {
            return _dbContext.Products.Where(p => p.CategoryId == categoryId).ToList();
        }

        // Kiểm tra xem danh mục có tồn tại trong cơ sở dữ liệu hay không
        private bool CategoryExists(int id)
        {
            return _dbContext.Categories.Any(e => e.Id == id);
        }
    }
}
