using Lap03WebBanHang.DataAccess;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;

namespace Lap03WebBanHang.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }
        [Range(0.01, 1000000000)]
        public decimal Price { get; set; }
        public string Description { get; set; }
        public string? ImageUrl { get; set; }
        public List<ProductImage>? Images { get; set; }

        public int CategoryId { get; set; }
        [JsonIgnore] // Để tránh serialize tham chiếu vòng
        public Category? Category { get; set; }


    }
}