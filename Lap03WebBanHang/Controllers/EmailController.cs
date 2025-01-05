using Lap03WebBanHang.Models;
using Microsoft.AspNetCore.Mvc;

namespace Lap03WebBanHang.Controllers
{
    public class EmailController : Controller
    {
        private readonly EmailSender _emailSender;

        public EmailController(EmailSender emailSender)
        {
            _emailSender = emailSender;
        }

        [HttpPost]
        public async Task<IActionResult> SendOrderConfirmationEmail(ApplicationUser user, ShoppingCart cart, Order order)
        {
            // Tạo thông tin chi tiết đơn hàng dưới dạng chuỗi
            var orderDetails = string.Join("\n", cart.Items.Select(i => $"{i.Name} - Số lượng: {i.Quantity} x {i.Price:C}"));

            // Tạo nội dung email
            string message = $@"
            Chào {user.FullName},
            
            Cảm ơn bạn đã mua sắm tại cửa hàng của chúng tôi. Dưới đây là thông tin đơn hàng của bạn:

            {orderDetails}

            Tổng cộng: {order.TotalPrice:C}

            Đơn hàng của bạn hiện đang trong trạng thái chờ giao hàng.
            
            Cảm ơn bạn đã tin tưởng và sử dụng dịch vụ của chúng tôi!
            
            Trân trọng,
            Đội ngũ bán hàng";

            // Gửi email xác nhận đơn hàng
            await _emailSender.SendEmailAsync(user.Email, "Xác nhận đơn hàng thành công", message);

            return Ok(); // Có thể trả về thông báo OK hoặc chuyển hướng tùy nhu cầu
        }
    }

}
