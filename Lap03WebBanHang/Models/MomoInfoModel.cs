using System.ComponentModel.DataAnnotations;
namespace Lap03WebBanHang.Models
{
    public class MomoInfoModel
    {
        public int Id { get; set; }
        public string OrderId { get; set; }
        public string OrderInfo { get; set; }
        public decimal ? Amount { get; set; }  
         public  string FullName    { get; set; }
        public DateTime ? DatePaid { get; set; }  
    }
}
