using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ConnectDB.Models
{
    public class Combo
    {
        [Key]
        public int ComboId { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; }

        public string? Description { get; set; }

        [Required]
        public decimal Price { get; set; } // Giá ưu đãi
        
        public bool IsActive { get; set; }

        // Ví dụ một Combo có thể gồm nhiều ComboDetail để link tới Product (ở đây giản lược để thành thông tin text nếu muốn, hoặc thêm ComboDetail)
    }
}
