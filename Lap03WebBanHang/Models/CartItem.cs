namespace Lap03WebBanHang.Models
{
    public class CartItem
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        // Thêm thuộc tính Product vào CartItem
        public Product Product { get; set; }
    }
}
