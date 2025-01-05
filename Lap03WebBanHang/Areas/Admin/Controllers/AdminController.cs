using Lap03WebBanHang.Models;
using Lap03WebBanHang.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using Lap03WebBanHang.Areas.Admin.Controllers;
using Lap03WebBanHang.ViewModel;
using Microsoft.EntityFrameworkCore;
using Lap03WebBanHang.DataAccess;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;
using System.Drawing;

namespace Lap03WebBanHang.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]

    public class AdminController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ApplicationDbContext _context;
        private readonly IOrderRepository _orderRepository;  // Thêm vào
        
        public AdminController(IProductRepository productRepository, ICategoryRepository categoryRepository, ApplicationDbContext context, IOrderRepository orderRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _context = context;
            _orderRepository = orderRepository;
        }

        public async Task<IActionResult> Index()
        {
            // Lấy tổng số sản phẩm
            var totalProducts = await _productRepository.GetAllAsync();
            ViewBag.TotalProducts = totalProducts.Count();

            // Lấy tổng số danh mục
            var totalCategories = await _categoryRepository.GetAllAsync();
            ViewBag.TotalCategories = totalCategories.Count();

            // Lấy tổng số sản phẩm tồn kho
            var totalInventoryProducts = await _context.Warehouse.SumAsync(w => w.QuantityInStock);
            ViewBag.TotalInventoryProducts = totalInventoryProducts;

            // Lấy danh sách sản phẩm mới thêm gần đây (giả sử lấy 5 sản phẩm gần nhất)
            ViewBag.RecentProducts = totalProducts.OrderByDescending(p => p.Id).Take(5).ToList();       

            return View();
        }
        [HttpGet]
        public async Task<IActionResult> GetRevenueData(DateTime startDate, DateTime endDate)
        {
            try
            {
                // Kiểm tra logic ngày
                if (startDate > endDate)
                {
                    return BadRequest(new { error = "Ngày bắt đầu không được lớn hơn ngày kết thúc." });
                }

                // Lọc các đơn hàng theo trạng thái và ngày
                var orders = await _context.Orders
                    .Where(o => o.DeliveryStatus == "Completed"
                             && o.OrderDate.Date >= startDate.Date
                             && o.OrderDate.Date <= endDate.Date)
                    .ToListAsync();

                if (!orders.Any())
                {
                    return Json(new { message = "Không có hóa đơn nào trong khoảng thời gian này." });
                }

                // Group by ngày và tính tổng doanh thu
                var revenueData = orders
                    .GroupBy(o => o.OrderDate.Date)
                    .Select(g => new
                    {
                        Date = g.Key.ToString("yyyy-MM-dd"), // Trả về ngày định dạng "yyyy-MM-dd"
                        Revenue = g.Sum(o => o.TotalPrice)
                    })
                    .OrderBy(g => g.Date)
                    .ToList();

                return Json(revenueData);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }





        // --- QUẢN LÝ SẢN PHẨM ---

        // Hiển thị danh sách sản phẩm
        public async Task<IActionResult> Products(int? categoryId, int page = 1, int pageSize = 10)
        {
            // Retrieve products based on category or all products
            var products = categoryId.HasValue
                ? await _productRepository.GetProductsByCategoryAsync(categoryId.Value)
                : await _productRepository.GetAllAsync();

            // Calculate total products and total pages
            var totalProducts = products.Count();
            var totalPages = (int)Math.Ceiling((double)totalProducts / pageSize);

            // Apply pagination
            var paginatedProducts = products
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Pass data to the view
            ViewBag.Categories = new SelectList(await _categoryRepository.GetAllAsync(), "Id", "Name");
            ViewBag.SelectedCategoryId = categoryId;
            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;

            return View(paginatedProducts);
        }


        // --- QUẢN LÝ DANH MỤC ---

        // Hiển thị danh sách danh mục
        public async Task<IActionResult> Categories()
        {
            var categories = await _categoryRepository.GetAllAsync();
            return View(categories);
        }

        // Hiển thị form thêm danh mục
      
        public async Task<IActionResult> Orders(int page = 1, int pageSize = 10)
        {
            // Lấy tất cả đơn hàng từ cơ sở dữ liệu
            var orders = await _orderRepository.GetAllAsync();

            // Tính toán số lượng đơn hàng và số trang
            var totalOrders = orders.Count();
            var totalPages = (int)Math.Ceiling((double)totalOrders / pageSize);

            // Phân trang
            var paginatedOrders = orders
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Truyền dữ liệu vào view
            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;

            return View(paginatedOrders);
        }

        public IActionResult Details(int id)
        {
            var order = _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefault(o => o.Id == id);

            if (order == null)
            {
                return NotFound("Hóa đơn không tồn tại.");
            }
            // Lấy tên người dùng từ bảng AspNetUsers
            var user = _context.Users.FirstOrDefault(u => u.Id == order.UserId);
            var userName = user != null ? user.FullName : "Người dùng không xác định";

            var viewModel = new OrderViewModel
            {
                Id = order.Id,
                UserName = userName,
                UserId = order.UserId,
                OrderDate = order.OrderDate,
                TotalPrice = order.TotalPrice,
                ShippingAddress = order.ShippingAddress,
                Notes = order.Notes,
                PaymentMethod = order.PaymentMethod,
                OrderDetails = order.OrderDetails.ToList(),
                Phone = order.Phone,    
            };

            return View(viewModel);
        }

        public IActionResult GenerateQRCode(string qrText)
        {
            var qrWriter = new BarcodeWriterPixelData
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new EncodingOptions
                {
                    Height = 125, // Chiều cao mã QR
                    Width = 125,  // Chiều rộng mã QR
                    Margin = 1    // Biên xung quanh mã QR
                }
            };

            var pixelData = qrWriter.Write(qrText);

            // Chuyển đổi dữ liệu QR thành hình ảnh PNG
            using (var bitmap = new Bitmap(pixelData.Width, pixelData.Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb))
            {
                var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    System.Drawing.Imaging.ImageLockMode.WriteOnly, bitmap.PixelFormat);
                try
                {
                    System.Runtime.InteropServices.Marshal.Copy(pixelData.Pixels, 0, bitmapData.Scan0, pixelData.Pixels.Length);
                }
                finally
                {
                    bitmap.UnlockBits(bitmapData);
                }

                using (var stream = new MemoryStream())
                {
                    bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                    return File(stream.ToArray(), "image/png");
                }
            }
        }

        public async Task<IActionResult> Warehouse(int page = 1, int pageSize = 10)
        {
            // Lấy danh sách kho hàng với thông tin sản phẩm và nhà cung cấp
            var warehouseQuery = _context.Warehouse
                .Include(w => w.Product)
                .Include(w => w.Supplier) // Include Supplier
                .AsQueryable();

            // Áp dụng phân trang
            var paginatedWarehouse = await PaginatedList<Warehouse>.CreateAsync(
                warehouseQuery, page, pageSize);

            ViewBag.Page = page;
            ViewBag.TotalPages = paginatedWarehouse.TotalPages;

            return View(paginatedWarehouse);
        }

        public IActionResult ImportStock()
        {
            ViewBag.Products = new SelectList(_context.Products.ToList(), "Id", "Name");
            ViewBag.Suppliers = new SelectList(_context.Suppliers.ToList(), "Id", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ImportStock(int productId, int quantity, decimal price, int supplierId)
        {
            if (quantity <= 0 || price <= 0)
            {
                return BadRequest("Số lượng và giá phải lớn hơn 0.");
            }

            var warehouseItem = await _context.Warehouse
                .FirstOrDefaultAsync(w => w.ProductId == productId && w.Supplier.Id == supplierId);

            if (warehouseItem != null)
            {
                warehouseItem.QuantityInStock += quantity;
                warehouseItem.LastUpdated = DateTime.Now;
            }
            else
            {
                var newWarehouseItem = new Warehouse
                {
                    ProductId = productId,
                    QuantityInStock = quantity,
                    ReorderLevel = 10,
                    LastUpdated = DateTime.Now,
                    Supplier = await _context.Suppliers.FindAsync(supplierId)
                };

                await _context.Warehouse.AddAsync(newWarehouseItem);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Warehouse");
        }

        public IActionResult ManageStock(int id)
        {
            var warehouseItem = _context.Warehouse
                .Include(w => w.Product)
                .Include(w => w.Supplier)
                .FirstOrDefault(w => w.Id == id);

            if (warehouseItem == null)
            {
                return NotFound();
            }

            ViewBag.Suppliers = new SelectList(_context.Suppliers.ToList(), "Id", "Name");
            return View(warehouseItem);
        }

        [HttpPost]
        public async Task<IActionResult> ManageStock(int id, int quantity, string transactionType, int supplierId)
        {
            var warehouseItem = await _context.Warehouse
                .Include(w => w.Product)
                .FirstOrDefaultAsync(w => w.Id == id);

            if (warehouseItem == null)
            {
                return NotFound();
            }

            if (transactionType == "Import")
            {
                warehouseItem.QuantityInStock += quantity;
                warehouseItem.Supplier = await _context.Suppliers.FindAsync(supplierId);
            }
            else if (transactionType == "Export")
            {
                if (warehouseItem.QuantityInStock < quantity)
                {
                    return BadRequest("Số lượng xuất vượt quá tồn kho.");
                }
                warehouseItem.QuantityInStock -= quantity;
            }

            warehouseItem.LastUpdated = DateTime.Now;

            await _context.SaveChangesAsync();
            return RedirectToAction("Warehouse");
        }

        public IActionResult WarehouseAlerts()
        {
            var lowStockItems = _context.Warehouse
                .Include(w => w.Product)
                .Where(w => w.QuantityInStock <= 10)
                .ToList();

            return View(lowStockItems);
        }



    }

    
}


