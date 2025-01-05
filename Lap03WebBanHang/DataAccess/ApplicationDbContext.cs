using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Lap03WebBanHang.Models;
using Microsoft.EntityFrameworkCore;

namespace Lap03WebBanHang.DataAccess
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)

        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }      
        public DbSet<MomoInfoModel> MomoInfos { get; set; }  // Thêm dòng này
        public DbSet<Warehouse> Warehouse { get; set; }
        public DbSet<WarehouseTransaction> WarehouseTransactions { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }

    }

}
