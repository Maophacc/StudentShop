using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConnectDB.Models
{
    public class Voucher
    {
        [Key]
        public int VoucherId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Code { get; set; } // Mã voucher, ví dụ KHAITRUONG

        public int MaxUsages { get; set; } // Số lần dùng tối đa
        public int CurrentUsages { get; set; } // Số lần đã dùng

        [Required]
        public int PromotionId { get; set; }
        
        [ForeignKey("PromotionId")]
        public virtual Promotion? Promotion { get; set; }
        
        public bool IsActive { get; set; }
    }
}
