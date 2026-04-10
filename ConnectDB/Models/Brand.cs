using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation; // Thêm thư viện này

namespace ConnectDB.Models
{
    public class Brand
    {
        [Key]
        public int BrandId { get; set; }

        [Required]
        [MaxLength(100)]
        public string BrandName { get; set; }

        [MaxLength(100)]
        public string? Origin { get; set; } // Thêm dấu ? để Origin cũng không bắt buộc

        // SỬA TẠI ĐÂY: Thêm dấu ? và [ValidateNever]
        [ValidateNever]
        public virtual ICollection<Product>? Products { get; set; }
    }
}