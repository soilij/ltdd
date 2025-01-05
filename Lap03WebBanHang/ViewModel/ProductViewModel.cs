using Lap03WebBanHang.Models;

namespace Lap03WebBanHang.ViewModel
{
    public class ProductViewModel
    {
        public List<Product> ?Products { get; set; }   // Danh sách sản phẩm
        public int CurrentPage { get; set; }           // Trang hiện tại
        public int TotalPages { get; set; }            // Tổng số trang
        public int PageSize { get; set; }              // Kích thước trang (số sản phẩm mỗi trang)
    }
}
