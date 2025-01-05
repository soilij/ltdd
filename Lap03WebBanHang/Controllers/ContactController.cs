using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mail;

namespace Lap03WebBanHang.Controllers
{
    public class ContactController : Controller
    {
        [HttpPost]
        public IActionResult SendEmail(string firstname, string lastname, string country, string subject)
        {
            try
            {
                var fromAddress = new MailAddress("tranquocbao.9a10.20172018@gmail.com", "Tran Quoc Bao");
                var toAddress = new MailAddress("tranquocbao.9a10.20172018@gmail.com");
                const string fromPassword = "chmt kyvp ymeg eqvd"; // Mật khẩu ứng dụng

                string body = $"Tên: {firstname}\nSố điện thoại: {lastname}\nLý do: {country}\nNội dung:\n{subject}";

                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
                };

                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = "New Contact Form Submission",
                    Body = body
                })
                {
                    smtp.Send(message);
                }

                ViewBag.Message = "Email đã được gửi thành công!";
                return View("~/Views/Home/Success.cshtml"); 

            }
            catch (Exception ex)
            {
                ViewBag.Message = "Gửi email thất bại. Lỗi: " + ex.Message;
                return View("Error");
            }
        }
    }
}
