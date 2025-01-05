using Microsoft.AspNetCore.Mvc;
using Lap03WebBanHang.DataAccess;
using Lap03WebBanHang.Models;
using System;
using System.Linq;  // Added for LINQ queries
using System.Threading.Tasks;
using Lap03WebBanHang.Services.Momo;
using System.Security.Claims;
using Lap03WebBanHang.Models.Momo;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Lap03WebBanHang.Controllers
{
    public class PaymentCallbackController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMomoService _momoService;
        private readonly EmailService _emailService;
        private readonly UserManager<ApplicationUser> _userManager; // Inject UserManager

        public PaymentCallbackController(ApplicationDbContext context, IMomoService momoService, EmailService emailService, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _momoService = momoService;
            _emailService = emailService;
            _userManager = userManager; // Store the UserManager
        }

        [HttpGet]
        public async Task<IActionResult> PaymentCallBack()
        {
            var requestQuery = HttpContext.Request.Query;

            string errorCode = requestQuery["errorCode"];
            string orderId = requestQuery["orderId"]; // Get order ID from the request

            if (errorCode == "0") // Thành công
            {
                // Lấy thông tin người dùng
                string userEmail = User.FindFirstValue(ClaimTypes.Email);
                ApplicationUser currentUser = await _userManager.FindByEmailAsync(userEmail);
                string fullName = currentUser?.FullName ?? "Người dùng không xác định";

                var newMomoInsert = new MomoInfoModel
                {
                    OrderId = orderId,
                    FullName = fullName,
                    Amount = decimal.Parse(requestQuery["amount"]),
                    OrderInfo = requestQuery["orderInfo"],
                    DatePaid = DateTime.Now
                };

                // Lưu thông tin vào cơ sở dữ liệu
                _context.Add(newMomoInsert);
                await _context.SaveChangesAsync();

                // Gửi email xác nhận
                await _emailService.SendPaymentConfirmationEmail(userEmail, fullName, "Momo", newMomoInsert.Amount ?? 0);

                TempData["success"] = "Thanh toán thành công!";
                return View("PaymentCallBack", newMomoInsert);
            }
            else // Thất bại
            {
                string errorMessage = requestQuery["message"];
                string failMessage = $"Giao dịch thanh toán không thành công: {errorMessage}";

                // Giải mã URL trước khi áp dụng biểu thức chính quy
                string decodedOrderId = System.Net.WebUtility.UrlDecode(orderId);

                // Trích xuất phần số từ orderId
                Regex regex = new Regex(@"\d+"); // Biểu thức chính quy để lấy số
                Match match = regex.Match(decodedOrderId);

                if (match.Success) // Nếu trích xuất thành công phần số
                {
                    int orderIdInt = int.Parse(match.Value); // Chuyển đổi thành số nguyên

                    var failedOrder = await _context.Orders.FirstOrDefaultAsync(m => m.Id == orderIdInt);

                    if (failedOrder != null)
                    {
                        // Xóa đơn hàng do thanh toán không thành công
                        _context.Orders.Remove(failedOrder); // Xóa đơn hàng
                        await _context.SaveChangesAsync(); // Lưu thay đổi
                    }
                    else
                    {
                        TempData["error"] = "Không tìm thấy đơn hàng tương ứng trong cơ sở dữ liệu.";
                        return View("PaymentCallBack");
                    }
                }
                else
                {
                    TempData["error"] = "Không tìm thấy OrderId hợp lệ để xử lý.";
                    return View("PaymentCallBack");
                }

                // Gửi email thông báo thanh toán thất bại
                await _emailService.SendEmailAsync(
                    User.FindFirstValue(ClaimTypes.Email),
                    "Thanh toán thất bại",
                    failMessage
                );

                TempData["error"] = "Giao dịch thanh toán thất bại. Đơn hàng của bạn đã bị xóa vui lòng mua hàng lại.";
                return View("PaymentCallBack");
            }

        }
    }
}
