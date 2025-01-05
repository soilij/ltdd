using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lap03WebBanHang.Models
{
    public class Warehouse
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Product")]
        public int ProductId { get; set; } // Khóa ngoại liên kết với bảng Products

        [Range(0, int.MaxValue, ErrorMessage = "QuantityInStock must be a positive number.")]
        public int QuantityInStock { get; set; } // Số lượng sản phẩm trong kho

        [Range(0, int.MaxValue, ErrorMessage = "ReorderLevel must be a positive number.")]
        public int ReorderLevel { get; set; } // Mức tồn kho tối thiểu

        public DateTime LastUpdated { get; set; } = DateTime.Now; // Thời gian cập nhật lần cuối

        // Navigation Property
        public Product? Product { get; set; } // Liên kết với bảng Products

        // Thêm nhà cung cấp
        [ForeignKey("Supplier")]
        public int? SupplierId { get; set; } // Khóa ngoại liên kết với bảng Supplier
        public Supplier? Supplier { get; set; } // Navigation Property đến bảng Supplier
    }
}
