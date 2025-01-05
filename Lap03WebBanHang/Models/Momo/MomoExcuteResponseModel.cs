namespace Lap03WebBanHang.Models.Momo
{
    public class MomoExcuteResponseModel
    {
        public string FullName { get; set; }
        public string OrderId { get; set; }
        public string OrderInfo { get; set; }
        public decimal? Amount { get; set; } // Dành cho Amount, nullable decimal
        public string Message { get; set; }
        public string ErrorCode { get; set; }
        public bool IsSuccess { get; set; }
        public DateTime? DatePaid { get; set; }
    }
}
