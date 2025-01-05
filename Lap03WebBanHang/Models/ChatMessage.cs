namespace Lap03WebBanHang.Models
{
    public class ChatMessage
    {
        public string Sender { get; set; } // "User" hoặc "GPT"
        public string Message { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now; // Thời gian gửi tin nhắn
    }
}
