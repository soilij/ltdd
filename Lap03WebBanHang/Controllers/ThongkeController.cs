using Lap03WebBanHang.DataAccess;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Lap03WebBanHang.Controllers
{
    public class ThongkeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ThongkeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var revenueByDay = await _context.Orders
                .GroupBy(o => o.OrderDate.Date)
                .Select(g => new { Date = g.Key, TotalRevenue = g.Sum(o => o.TotalPrice) })
                .OrderBy(r => r.Date)
                .ToListAsync();
            return View(revenueByDay);
        }

        [HttpPost]
        public IActionResult RevenueByDay()
        {
            var revenueByDay = _context.Orders
                .GroupBy(o => o.OrderDate.Date)
                .Select(g => new { Date = g.Key, TotalRevenue = g.Sum(o => o.TotalPrice) })
                .OrderBy(r => r.Date)
                .ToList();
            return Json(revenueByDay);
        }
    }
}

