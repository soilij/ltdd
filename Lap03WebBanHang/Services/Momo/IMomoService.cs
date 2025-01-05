using Lap03WebBanHang.Models;
using Lap03WebBanHang.Models.Momo;

namespace Lap03WebBanHang.Services.Momo
{
    public interface IMomoService
    {
        Task<MomoCreatePaymentResponseModel> CreatePaymentUrl(OrderInfoModel model); // Chỉ khai báo phương thức, không cần phần thân
        MomoExcuteResponseModel PaymentExecuteAsync(IQueryCollection collection);
    }

}
