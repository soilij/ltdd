using System.Collections.Generic;
using System.Threading.Tasks;
using Lap03WebBanHang.Models;

namespace Lap03WebBanHang.Repositories
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllAsync();
        Task<Product> GetByIdAsync(int id);
        Task AddAsync(Product product);
        Task UpdateAsync(Product product);
        Task DeleteAsync(int id);

        // Thêm phương thức để lấy danh sách sản phẩm theo query
        Task<IEnumerable<Product>> SearchProductsAsync(string query);
        Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId);

        // Phương thức để lấy số lượng đã bán của một sản phẩm
        //Task<int> GetQuantitySoldAsync(int productId);
        Task<IEnumerable<Product>> GetSimilarProductsAsync(int productId); // Thêm phương thức này
        Task<List<ProductSales>> GetBestSellingProductsAsync(); //best selling
    }
}
