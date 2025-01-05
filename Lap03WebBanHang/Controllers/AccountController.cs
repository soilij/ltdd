using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Lap03WebBanHang.Controllers
{
    public class AccountController : Controller  // Đảm bảo bạn kế thừa Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AccountController> _logger;


        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }
        // Action này sẽ nhận mã quản trị từ AJAX và kiểm tra tính hợp lệ
        [Route("register/check-admin-key")] // Đặt route rõ ràng cho action này
        [HttpPost]
        public IActionResult CheckAdminKey(string adminKey)
        {
            // Kiểm tra mã quản trị (có thể lấy từ cơ sở dữ liệu hoặc cấu hình)
            if (adminKey == "123456")  // Giả sử mã quản trị hợp lệ là "123456"
            {
                return Json(new { isValid = true });  // Trả về true nếu mã hợp lệ
            }
            else
            {
                return Json(new { isValid = false });  // Trả về false nếu mã không hợp lệ
            }
        }
        [HttpGet]
        public async Task<IActionResult> Manage()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            // Cập nhật ViewBag với các tùy chọn tài khoản
            ViewBag.AccountOptions = new
            {
                ManageAccount = Url.Action("Manage", "Account"),
                OrderHistory = Url.Action("OrderHistory", "Account"),
                Logout = Url.Action("Logout", "Account")
            };

            return View(user);
        }

        public IActionResult OrderHistory()
        {
            // Logic để lấy lịch sử mua hàng của người dùng
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }


    }
}
