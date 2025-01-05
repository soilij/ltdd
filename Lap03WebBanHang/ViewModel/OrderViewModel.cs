using Lap03WebBanHang.Models;

namespace Lap03WebBanHang.ViewModel
{
    public class OrderViewModel
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; } // Thêm thuộc tính UserName
        public DateTime OrderDate { get; set; }
        public decimal TotalPrice { get; set; }
        public string ShippingAddress { get; set; }
        public string Notes { get; set; }
        public string PaymentMethod { get; set; } // Thêm thuộc tính PaymentMethod
        public string DeliveryStatus { get; set; }
        public string Phone {  get; set; }  
        public List<OrderDetail> OrderDetails { get; set; }
    }
}
