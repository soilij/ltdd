using System.ComponentModel.DataAnnotations;

namespace Lap03WebBanHang.Models
{
    public class Category
    {
        public int Id { get; set; }
        [Required, StringLength(50)]
        public string Name { get; set; }
        public string ImageUrlCate { get; set; }
        public List<Product>? Products { get; set; }
        
    }
}
