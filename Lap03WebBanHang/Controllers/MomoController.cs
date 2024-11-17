using Lap03WebBanHang.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using Thanh_toan_qua_Momo;

namespace Lap03WebBanHang.Controllers
{
    public class MomoController : Controller
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public IActionResult HomePage()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Process(Order order, decimal totalPrice)
        {
            // Khai báo các thông số yêu cầu cần gửi tới hệ thống MoMo
            string endpoint = "https://test-payment.momo.vn/v2/gateway/api/create";
            string partnerCode = "MOMO5RGX20191128";
            string accessKey = "M8brj9K6E22vXoDB";
            string serectkey = "nqQiVSgDMy809JoPF6OzP5OdBUB550Y4";
            string orderInfo = "Thanh toán đơn hàng";
            string redirectUrl = "https://webhook.site/b3088a6a-2d17-4f8d-a383-71389a6c600b";
            string ipnUrl = "https://webhook.site/b3088a6a-2d17-4f8d-a383-71389a6c600b";
            string requestType = "captureWallet";

            // Tính tổng số tiền cần thanh toán dựa trên thông tin đơn hàng
            long amount = Convert.ToInt32(totalPrice);
            string orderId = Guid.NewGuid().ToString();
            string requestId = Guid.NewGuid().ToString();
            string extraData = "";

            // Xây dựng rawHash để tạo chữ ký HMAC SHA256
            string rawHash = $"accessKey={accessKey}&amount={amount}&extraData={extraData}&ipnUrl={ipnUrl}&orderId={orderId}&orderInfo={orderInfo}&partnerCode={partnerCode}&redirectUrl={redirectUrl}&requestId={requestId}&requestType={requestType}";
            log.Debug("rawHash = " + rawHash);

            MomoBaomat crypto = new MomoBaomat();
            // Tạo chữ ký HMAC SHA256
            string signature = crypto.signSHA256(rawHash, serectkey);
            log.Debug("Signature = " + signature);

            // Xây dựng JSON request tới MoMo
            JObject message = new JObject
            {
                { "partnerCode", partnerCode },
                { "partnerName", "Test" },
                { "storeId", "MomoTestStore" },
                { "requestId", requestId },
                { "amount", amount },
                { "orderId", orderId },
                { "orderInfo", orderInfo },
                { "redirectUrl", redirectUrl },
                { "ipnUrl", ipnUrl },
                { "lang", "en" },
                { "extraData", extraData },
                { "requestType", requestType },
                { "signature", signature }
            };
            log.Debug("Json request to MoMo: " + message.ToString());

            // Gửi yêu cầu thanh toán tới MoMo và nhận kết quả trả về
            string responseFromMomo = TaoAPI.sendPaymentRequest(endpoint, message.ToString());
            JToken jmessage = JToken.Parse(responseFromMomo);
            log.Debug("Return from MoMo: " + jmessage.ToString());

            // Mở trình duyệt để hiển thị trang thanh toán của MoMo
            // Trong ứng dụng web, bạn có thể chuyển hướng người dùng đến URL trang thanh toán hoặc hiển thị trang thanh toán trong iframe
            return Redirect(jmessage.Value<string>("payUrl"));
        }
    }
}
