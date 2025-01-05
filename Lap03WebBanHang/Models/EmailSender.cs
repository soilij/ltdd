using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Lap03WebBanHang.Models
{
    public class EmailSender // Đảm bảo khai báo lớp EmailSender
    {
        private const string FromEmail = "tranquocbao.9a10.20172018@gmail.com"; // Địa chỉ email của bạn
        private const string AppPassword = "lovt sngh rons elim"; // Mật khẩu ứng dụng của bạn

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var mailMessage = new MailMessage
            {
                From = new MailAddress(FromEmail),
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };
            mailMessage.To.Add(toEmail);

            using (var smtpClient = new SmtpClient("smtp.gmail.com", 587))
            {
                smtpClient.Credentials = new NetworkCredential(FromEmail, AppPassword);
                smtpClient.EnableSsl = true;

                try
                {
                    await smtpClient.SendMailAsync(mailMessage);
                }
                catch (Exception ex)
                {
                    // Log error nếu gửi email thất bại
                    Console.WriteLine($"Error sending email: {ex.Message}");
                }
            }
        }
    }
}
