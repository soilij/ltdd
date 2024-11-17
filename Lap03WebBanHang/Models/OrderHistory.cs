namespace Lap03WebBanHang.Models
{
    public class OrderHistory
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public List<OrderDetail> OrderDetails { get; set; }
        public ApplicationUser User { get; set; }
    }
}
