namespace Lap03WebBanHang.Models
{
    public class WarehouseTransaction
    {
        public int Id { get; set; }
        public int WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; }
        public int Quantity { get; set; } // Số lượng nhập hoặc xuất
        public string TransactionType { get; set; } // 'Import' hoặc 'Export'
        public DateTime TransactionDate { get; set; } = DateTime.Now;
    }
}
