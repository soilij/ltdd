using Lap03WebBanHang.DataAccess;
using Lap03WebBanHang.Models;
using Lap03WebBanHang.Services.Momo;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Lap03WebBanHang.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IMomoService _momoService;
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService; // Thêm EmailService

        public PaymentController(IMomoService momoService, ApplicationDbContext context, EmailService emailService)
        {
            _momoService = momoService;
            _context = context;
            _emailService = emailService; // Khởi tạo EmailService
        }

        [HttpPost]
        public async Task<IActionResult> CreatePaymentUrl(OrderInfoModel model)
        {
            var response = await _momoService.CreatePaymentUrl(model);
            return Redirect(response.PayUrl);
        }

        [HttpGet]
        public IActionResult PaymentCallBack()
        {
            var response = _momoService.PaymentExecuteAsync(HttpContext.Request.Query);
            return View(response);
        }

        [HttpPost]
        public async Task<IActionResult> PayWithCash(int orderId)
        {
            // Tìm đơn hàng theo Id
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
            if (order != null)
            {
                // Cập nhật phương thức thanh toán thành "Cash"
                order.PaymentMethod = "Cash";
                await _context.SaveChangesAsync(); // Lưu thay đổi vào CSDL

                // Gửi email thông báo thanh toán thành công
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == order.UserId); // Lấy người dùng tương ứng
                if (user != null)
                {
                    await _emailService.SendPaymentConfirmationEmail(
                        user.Email,
                        user.FullName,
                        order.PaymentMethod,
                        order.TotalPrice
                    );
                }

                // Điều hướng về trang Home/Index với thông báo thành công
                TempData["SuccessMessage"] = "Thanh toán bằng tiền mặt đã được ghi nhận và email xác nhận đã được gửi.";
                return RedirectToAction("Index", "Home");
            }

            // Nếu không tìm thấy đơn hàng
            TempData["ErrorMessage"] = "Không tìm thấy đơn hàng.";
            return RedirectToAction("Index", "Home");
        }
    }

}
