using Lap03WebBanHang.DataAccess;
using Lap03WebBanHang.Models;
using Lap03WebBanHang.Models.Momo;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace Lap03WebBanHang.Services.Momo
{
    public class MomoService : IMomoService
    {
        private readonly IOptions<MomoOptionModel> _options;
        private readonly ApplicationDbContext _context;
        public MomoService(IOptions<MomoOptionModel> options,ApplicationDbContext context )
        {
            _options = options;
            _context = context;
        }
        public async Task<MomoCreatePaymentResponseModel> CreatePaymentUrl(OrderInfoModel model)
        {
            var orderIdString = model.OrderId;

            // Sử dụng Regex để chỉ lấy phần số trong orderId (nếu có chuỗi bổ sung)
            Regex regex = new Regex(@"\d+"); // Biểu thức chính quy để tìm phần số
            Match match = regex.Match(orderIdString);

            if (match.Success)
            {
                // Lấy phần số từ orderId
                var orderId = match.Value;

                // Chuyển orderId thành số và đảm bảo rằng nó hợp lệ
                if (int.TryParse(orderId, out int orderIdInt))
                {
                    // Lấy đơn hàng từ cơ sở dữ liệu
                    var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderIdInt);

                    if (order != null)
                    {
                        // Cập nhật phương thức thanh toán vào cơ sở dữ liệu
                        order.PaymentMethod = "MOMO";
                        await _context.SaveChangesAsync(); // Lưu thay đổi vào cơ sở dữ liệu
                    }
                    else
                    {
                        throw new InvalidOperationException($"Đơn hàng với ID {orderIdInt} không tồn tại.");
                    }
                }
                else
                {
                    throw new FormatException("OrderId không hợp lệ. Không thể chuyển đổi phần số sang kiểu số.");
                }
            }
            else
            {
                // Nếu không tìm thấy số trong orderId, xử lý lỗi
                throw new FormatException("OrderId không hợp lệ. Chỉ được phép chứa số.");
            }

            // 2. Ghép chuỗi thông tin đơn hàng
            model.OderInfomation = "Nội dung: Thanh toán mua hàng tại Tiệm điện Trần Duy";

            // 3. Chuẩn bị dữ liệu rawData cho việc tạo chữ ký
            var rawData =
                $"partnerCode={_options.Value.PartnerCode}" +
                $"&accessKey={_options.Value.AccessKey}" +
                $"&requestId={orderIdString}" + // Dùng OrderId thực tế (chuỗi)
                $"&amount={model.Amount}" +
                $"&orderId={orderIdString}" + // Dùng OrderId thực tế (chuỗi)
                $"&orderInfo={model.OderInfomation}" +
                $"&returnUrl={_options.Value.ReturnUrl}" +
                $"&notifyUrl={_options.Value.NotifyUrl}" +
                $"&extraData=";

            // 4. Tính chữ ký HMAC SHA256
            var signature = ComputeHmacSha256(rawData, _options.Value.SecretKey);

            // 5. Tạo RestClient và RestRequest
            var client = new RestClient(_options.Value.MomoApiUrl);
            var request = new RestRequest() { Method = Method.Post };
            request.AddHeader("Content-Type", "application/json; charset=UTF-8");

            // 6. Chuẩn bị body cho request
            var requestData = new
            {
                accessKey = _options.Value.AccessKey,
                partnerCode = _options.Value.PartnerCode,
                requestType = _options.Value.RequestType,
                notifyUrl = _options.Value.NotifyUrl,
                returnUrl = _options.Value.ReturnUrl,
                orderId = orderIdString, // Dùng OrderId thực tế
                amount = model.Amount.ToString(),
                orderInfo = model.OderInfomation,
                requestId =orderIdString, // Dùng OrderId thực tế
                extraData = "",
                signature = signature
            };

            request.AddParameter("application/json", JsonConvert.SerializeObject(requestData), ParameterType.RequestBody);

            // 7. Gửi request và nhận phản hồi
            var response = await client.ExecuteAsync(request);

            // 8. Trả về dữ liệu phản hồi
            if (!response.IsSuccessful)
            {
                throw new InvalidOperationException($"Request to Momo API failed. Status: {response.StatusCode}");
            }

            return JsonConvert.DeserializeObject<MomoCreatePaymentResponseModel>(response.Content);
        }


        public MomoExcuteResponseModel PaymentExecuteAsync(IQueryCollection collection)
        {
            var amountString = collection.FirstOrDefault(s => s.Key == "amount").Value.ToString();
            var orderInfo = collection.FirstOrDefault(s => s.Key == "orderInfo").Value.ToString();
            var orderId = collection.FirstOrDefault(s => s.Key == "orderId").Value.ToString();
            var message = collection.FirstOrDefault(s => s.Key == "message").Value.ToString();
            var errorCode = collection.FirstOrDefault(s => s.Key == "errorCode").Value.ToString();

            // Kiểm tra nếu thanh toán thành công
            bool isSuccess = errorCode == "0"; // Mã lỗi "0" đại diện cho thành công

            // Chuyển amount thành decimal? (nullable decimal) nếu có thể
            decimal? amount = null;
            if (decimal.TryParse(amountString, out decimal tempAmount))
            {
                amount = tempAmount;
            }

            // Trả về MomoExcuteResponseModel với giá trị đã xử lý
            return new MomoExcuteResponseModel()
            {
                Amount = amount,
                OrderId = orderId,
                OrderInfo = orderInfo,
                Message = message,
                ErrorCode = errorCode,
                IsSuccess = isSuccess
            };
        }

        private string ComputeHmacSha256(string message, string secretKey)
        {
            // Chuyển đổi secretKey và message từ chuỗi sang mảng byte (UTF-8 encoding)
            var keyBytes = Encoding.UTF8.GetBytes(secretKey);
            var messageBytes = Encoding.UTF8.GetBytes(message);

            // Mảng để lưu trữ kết quả hash
            byte[] hashBytes;

            // Sử dụng HMACSHA256 để tính toán chữ ký
            using (var hmac = new HMACSHA256(keyBytes))
            {
                hashBytes = hmac.ComputeHash(messageBytes);
            }

            // Chuyển hash từ mảng byte sang chuỗi ký tự hexadecimal
            var hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

            // Trả về chuỗi chữ ký
            return hashString;
        }


       
       
    }
}
