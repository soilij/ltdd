using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Lap03WebBanHang.Models;
using Lap03WebBanHang.DataAccess;

public class OrderHistoryController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public OrderHistoryController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Account", new { area = "Identity" });
        }

        var orders = await _context.Orders
            .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
            .Where(o => o.UserId == user.Id)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        return View(orders); // Trả về List<Order> thay vì OrderHistory
    }
}