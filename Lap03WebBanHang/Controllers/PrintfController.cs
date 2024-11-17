using Lap03WebBanHang.Repositories;
using Microsoft.AspNetCore.Mvc;
using Lap03WebBanHang.Models;

public class PrintfController : Controller
{
    private readonly IOrderRepository _orderRepository;

    public PrintfController(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    [HttpGet]
    public IActionResult Index(int orderId, string userId, DateTime orderDate, decimal totalPrice, string shippingAddress, string notes)
    {
        // Truy vấn chi tiết đơn hàng từ orderId
        var orderDetails = _orderRepository.GetOrderDetails(orderId);

        // Truyền thông tin đơn hàng và chi tiết đơn hàng vào view
        var viewModel = new PrintfViewModel
        {
            OrderId = orderId,
            UserId = userId,
            OrderDate = orderDate,
            TotalPrice = totalPrice,
            ShippingAddress = shippingAddress,
            Notes = notes,
            OrderDetails = orderDetails
        };

        return View(viewModel);
    }
}
