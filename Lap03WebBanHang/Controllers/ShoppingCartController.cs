using Lap03WebBanHang.Extensions;
using Lap03WebBanHang.Models;
using Lap03WebBanHang.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Lap03WebBanHang.DataAccess;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Lap03WebBanHang.Services.Momo;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace Lap03WebBanHang.Controllers
{
    [Authorize]
    public class ShoppingCartController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly MomoService _momoService;
        private readonly ISmsSender _smsSender;


       public ShoppingCartController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IProductRepository productRepository, MomoService momoService, ISmsSender smsSender)
    {
        _smsSender = smsSender;
            _productRepository = productRepository;
            _context = context;
            _userManager = userManager;
           _momoService = momoService;  
        }

        public IActionResult HomePage()
        {
            return View();
        }

        // Trang hiển thị giỏ hàng
        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
            if (cart.Items.Count == 0)
            {
                ViewBag.Message = "Your shopping cart is empty.";
            }
            return View(cart);
        }

        // Trang checkout
        public IActionResult Checkout()
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            if (cart == null || !cart.Items.Any())
            {
                return RedirectToAction("Index");
            }
            var order = new Order();
            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(Order order)
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            if (cart == null || !cart.Items.Any())
            {
                return RedirectToAction("Index"); // Giỏ hàng trống
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account"); // Nếu không có người dùng, chuyển hướng đến trang đăng nhập
            }

            // Gắn thông tin cho đơn hàng
            order.UserId = user.Id;
            order.OrderDate = DateTime.UtcNow;
            order.TotalPrice = cart.Items.Sum(i => i.Price * i.Quantity);
            order.PaymentMethod = "Pending";
            order.DeliveryStatus = "Chờ giao hàng";

            // Kiểm tra số điện thoại
            if (string.IsNullOrWhiteSpace(order.Phone))
            {
                ModelState.AddModelError("Phone", "Số điện thoại là bắt buộc.");
                return View(order); // Trả lại form với thông báo lỗi nếu số điện thoại bị trống
            }

            // Khởi tạo danh sách OrderDetails
            order.OrderDetails = new List<OrderDetail>();

            // Duyệt qua các sản phẩm trong giỏ hàng để tạo danh sách chi tiết đơn hàng
            foreach (var item in cart.Items)
            {
                var warehouseItem = await _context.Warehouse.FirstOrDefaultAsync(w => w.ProductId == item.ProductId);
                if (warehouseItem == null)
                {
                    // Xử lý khi không tìm thấy sản phẩm trong kho
                    ModelState.AddModelError("", $"Product with ID {item.ProductId} not found in warehouse.");
                    return View(order);
                }

                if (warehouseItem.QuantityInStock < item.Quantity)
                {
                    // Xử lý khi số lượng sản phẩm trong kho không đủ
                    ModelState.AddModelError("", $"Product with ID {item.ProductId} does not have enough stock.");
                    return View(order);
                }

                // Trừ số lượng trong kho
                warehouseItem.QuantityInStock -= item.Quantity;

                // Thêm vào chi tiết đơn hàng
                order.OrderDetails.Add(new OrderDetail
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Price
                });
            }

            // Thêm đơn hàng vào cơ sở dữ liệu
            _context.Orders.Add(order);

            // Lưu thay đổi
            await _context.SaveChangesAsync();

            // Xóa giỏ hàng sau khi đặt hàng
            HttpContext.Session.Remove("Cart");

            return View("OrderCompleted", order); // Trang xác nhận hoàn thành đơn hàng
        }

        // Thêm sản phẩm vào giỏ hàng
        public async Task<IActionResult> AddToCart(int productId, int quantity)
        {
            var product = await GetProductFromDatabase(productId);
            var cartItem = new CartItem
            {
                ProductId = productId,
                Name = product.Name,
                Price = product.Price,
                Quantity = quantity,
                Product = product // Lưu thông tin sản phẩm vào CartItem
            };

            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
            cart.AddItem(cartItem);
            HttpContext.Session.SetObjectAsJson("Cart", cart);

            return RedirectToAction("Index");
        }


        // Cập nhật số lượng sản phẩm trong giỏ hàng
        [HttpPost]
        public IActionResult UpdateQuantity(int productId, int quantity)
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            if (cart != null)
            {
                var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
                if (item != null)
                {
                    item.Quantity = quantity;  // Cập nhật số lượng sản phẩm
                    HttpContext.Session.SetObjectAsJson("Cart", cart);  // Lưu lại giỏ hàng vào session
                }
            }
            return RedirectToAction("Index");
        }

        // Xóa sản phẩm khỏi giỏ hàng
        public IActionResult RemoveFromCart(int productId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            if (cart != null)
            {
                cart.RemoveItem(productId);
                HttpContext.Session.SetObjectAsJson("Cart", cart);
            }
            return RedirectToAction("Index");
        }

        // Xóa toàn bộ giỏ hàng
        public IActionResult ClearCart()
        {
            HttpContext.Session.Remove("Cart");
            return RedirectToAction("Index");
        }

        // Phương thức lấy thông tin sản phẩm từ database
        private async Task<Product> GetProductFromDatabase(int productId)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            return product;
        }
      
    }
}

