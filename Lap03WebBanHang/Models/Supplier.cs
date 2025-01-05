using System.ComponentModel.DataAnnotations;

namespace Lap03WebBanHang.Models
{
    public class Supplier
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Address { get; set; }
        public string Phone { get; set; }

        // Navigation Property
        public ICollection<Warehouse> Warehouses { get; set; }
    }
}
