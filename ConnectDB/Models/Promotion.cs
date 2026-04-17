using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ConnectDB.Models
{
    public class Promotion
    {
        [Key]
        public int PromotionId { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        public string Type { get; set; } // Ví dụ: "Percentage", "FixedAmount"

        [Required]
        public decimal Value { get; set; } // % giảm hoặc số tiền giảm

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        
        [ValidateNever]
        public virtual ICollection<Voucher>? Vouchers { get; set; }
    }
}
