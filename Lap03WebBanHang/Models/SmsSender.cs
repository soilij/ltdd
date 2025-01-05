using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using Microsoft.Extensions.Configuration;
namespace Lap03WebBanHang.Models
{
    public class SmsSender : ISmsSender
    {
        private readonly IConfiguration _configuration;

        public SmsSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void SendSms(string toPhoneNumber, string message)
        {
            var accountSid = _configuration["TwilioSettings:AccountSid"];
            var authToken = _configuration["TwilioSettings:AuthToken"];
            var fromPhoneNumber = _configuration["TwilioSettings:PhoneNumber"];

            TwilioClient.Init(accountSid, authToken);

            // Chuẩn hóa số điện thoại (thêm mã quốc gia nếu cần)
            var normalizedPhoneNumber = NormalizePhoneNumber(toPhoneNumber);

            var messageOptions = new CreateMessageOptions(new PhoneNumber(normalizedPhoneNumber))
            {
                From = new PhoneNumber(fromPhoneNumber),
                Body = message
            };

            var msg = MessageResource.Create(messageOptions);
        }

        // Hàm chuẩn hóa số điện thoại, thêm mã quốc gia (+84 cho Việt Nam)
        private string NormalizePhoneNumber(string phoneNumber)
        {
            // Nếu số bắt đầu bằng '0', thay thế bằng '+84' cho Việt Nam
            if (phoneNumber.StartsWith("0"))
            {
                return "+84" + phoneNumber.Substring(1);
            }
            return phoneNumber; // Nếu số đã có mã quốc gia
        }
    }
    public interface ISmsSender
    {
        void SendSms(string toPhoneNumber, string message);
    }

}
