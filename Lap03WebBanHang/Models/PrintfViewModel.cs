namespace Lap03WebBanHang.Models
{
    public class PrintfViewModel
    {
        public int OrderId { get; set; }
        public string UserId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalPrice { get; set; }
        public string ShippingAddress { get; set; }
        public string Notes { get; set; }
        public List<OrderDetail> OrderDetails { get; set; }
    }
}
