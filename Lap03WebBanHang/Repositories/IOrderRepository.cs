using Lap03WebBanHang.DataAccess;
using Lap03WebBanHang.Models;
using Lap03WebBanHang.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace Lap03WebBanHang.Repositories
{
    public interface IOrderRepository
    {
        Task<List<Order>> GetAllAsync(); // Lấy tất cả đơn hàng
        Task<OrderViewModel> GetOrderByIdAsync(int orderId); // Lấy thông tin hóa đơn theo ID
        Task<List<OrderDetail>> GetOrderDetailsAsync(int orderId);
    }

    public class OrderRepository : IOrderRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public OrderRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<Order>> GetAllAsync()
        {
            return await _dbContext.Orders.ToListAsync();
        }

        public async Task<OrderViewModel> GetOrderByIdAsync(int orderId)
        {
            var order = await _dbContext.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) return null;

            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == order.UserId);

            return new OrderViewModel
            {
                Id = order.Id,
                UserId = order.UserId,
                UserName = user?.UserName, // Lấy tên người dùng
                OrderDate = order.OrderDate,
                TotalPrice = order.TotalPrice,
                ShippingAddress = order.ShippingAddress,
                Notes = order.Notes,
                OrderDetails = order.OrderDetails.ToList()
            };
        }

        public async Task<List<OrderDetail>> GetOrderDetailsAsync(int orderId)
        {
            return await _dbContext.OrderDetails
                .Include(od => od.Product) // Bao gồm thông tin sản phẩm
                .Where(od => od.OrderId == orderId)
                .ToListAsync();
        }
    }
}
