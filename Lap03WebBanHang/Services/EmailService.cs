using Lap03WebBanHang.Models;
using System.Threading.Tasks;

public class EmailService
{
    private readonly EmailSender _emailSender;

    public EmailService(EmailSender emailSender)
    {
        _emailSender = emailSender;
    }

    // Phương thức gửi email thông báo thanh toán thành công
    public async Task SendPaymentConfirmationEmail(string email, string fullName, string paymentMethod, decimal totalAmount)
    {
        // Sử dụng văn hóa "vi-VN" để định dạng số tiền
        var cultureInfo = new System.Globalization.CultureInfo("vi-VN");
        string formattedAmount = totalAmount.ToString("C", cultureInfo);

        string message = $@"
        Chào {fullName},

        Cảm ơn bạn đã thanh toán đơn hàng của mình tại cửa hàng của chúng tôi.

        Phương thức thanh toán: {paymentMethod}
        Tổng số tiền thanh toán: {formattedAmount}

        Cảm ơn bạn đã tin tưởng và sử dụng dịch vụ của chúng tôi!

        Trân trọng,
        Đội ngũ bán hàng";

        await _emailSender.SendEmailAsync(email, "Xác nhận thanh toán", message);
    }

    // Thêm phương thức SendEmailAsync để có thể gửi bất kỳ email nào từ EmailService
    public async Task SendEmailAsync(string email, string subject, string message)
    {
        await _emailSender.SendEmailAsync(email, subject, message);
    }
}
