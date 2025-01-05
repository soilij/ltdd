using Lap03WebBanHang.DataAccess;
using Lap03WebBanHang.Models;
using Microsoft.EntityFrameworkCore;

namespace Lap03WebBanHang.Repositories
{
    public class EFProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _context;
        public EFProductRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            // Chờ cho dữ liệu được load xong trước khi trả về
            return await _context.Products
                .Include(p => p.Category) // Include thông tin về category
                .ToListAsync();
        }


        public async Task<Product> GetByIdAsync(int id)
        {
            // return await _context.Products.FindAsync(id);
            // lấy thông tin kèm theo category
            return await _context.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task AddAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }


        public async Task<IEnumerable<Product>> SearchProductsAsync(string query)
        {
            return await _context.Products
                .Where(p => p.Name.Contains(query) || (p.Description != null && p.Description.Contains(query)))
                .ToListAsync();
        }
        public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId)
        {
            return await _context.Products
                .Where(p => p.CategoryId == categoryId)
                .ToListAsync();
        }
        public async Task<IEnumerable<Product>> GetSimilarProductsAsync(int productId)
        {
            // Tìm sản phẩm dựa trên productId
            var product = await _context.Products.FindAsync(productId);

            // Nếu không tìm thấy sản phẩm, trả về danh sách rỗng
            if (product == null)
                return Enumerable.Empty<Product>();

            // Tìm các sản phẩm cùng loại và không phải là sản phẩm hiện tại (productId)
            var similarProducts = await _context.Products
                .Where(p => p.CategoryId == product.CategoryId && p.Id != productId)
                .ToListAsync();

            return similarProducts;
        }
        public async Task<List<ProductSales>> GetBestSellingProductsAsync()
        {
            // Lấy dữ liệu từ OrderDetails và tính tổng số lượng bán cho từng sản phẩm
            return await _context.OrderDetails
                .GroupBy(od => od.ProductId) // Nhóm theo ProductId để tính tổng số lượng bán
                .OrderByDescending(g => g.Sum(od => od.Quantity)) // Sắp xếp theo tổng số lượng bán giảm dần
                .Take(6) // Lấy 6 sản phẩm bán chạy nhất
                .Select(g => new ProductSales
                {
                    Product = g.FirstOrDefault().Product, // Lấy thông tin sản phẩm (có thể dùng FirstOrDefault() hoặc bất kỳ cách nào để lấy sản phẩm)
                    QuantitySold = g.Sum(od => od.Quantity) // Tổng số lượng bán
                })
                .ToListAsync(); // Chuyển kết quả thành danh sách
        }





    }
}

