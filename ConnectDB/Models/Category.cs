using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation; // Thêm thư viện này

namespace ConnectDB.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required]
        [MaxLength(100)]
        public string CategoryName { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; } // Thêm dấu ?

        // SỬA TẠI ĐÂY: Thêm dấu ? và [ValidateNever]
        [ValidateNever]
        public virtual ICollection<Product>? Products { get; set; }
    }
}