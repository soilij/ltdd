namespace Lap03WebBanHang.ViewModel
{
    public class InventoryViewModel
    {
        public int ProductId { get; set; }
        public string ?ProductName { get; set; }
        public string? ImageUrl { get; set; }
        public decimal Price { get; set; }
        public string? CategoryName { get; set; }
        public int Quantity { get; set; }
        public int MinimumStockLevel { get; set; }
        public string ?StockStatus { get; set; } // Đủ hàng, hết hàng, cảnh báo
        public string? Description { get; set; }
    }
}
