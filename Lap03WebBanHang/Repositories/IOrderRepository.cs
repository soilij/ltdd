using Lap03WebBanHang.DataAccess;
using Lap03WebBanHang.Models;
using Microsoft.EntityFrameworkCore;

namespace Lap03WebBanHang.Repositories
{
    public interface IOrderRepository
    {
        List<OrderDetail> GetOrderDetails(int orderId);
    }

    public class OrderRepository : IOrderRepository
    {
        private readonly ApplicationDbContext dbContext;

        public OrderRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public List<OrderDetail> GetOrderDetails(int orderId)
        {
            // Triển khai logic để lấy chi tiết đơn hàng từ cơ sở dữ liệu hoặc nguồn dữ liệu khác
            return dbContext.OrderDetails
                            .Include(od => od.Product) // Include Product vào truy vấn
                            .Where(od => od.OrderId == orderId)
                            .ToList();
        }

    }
}
