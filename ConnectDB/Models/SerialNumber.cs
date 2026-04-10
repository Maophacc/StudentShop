using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation; // Thêm cái này nếu muốn dùng [ValidateNever]

namespace ConnectDB.Models
{
    public class SerialNumber
    {
        [Key]
        public int SerialId { get; set; }

        [Required]
        [MaxLength(100)]
        public string ImeiCode { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } // IN_STOCK, SOLD, DAMAGED

        [MaxLength(500)]
        // THAY ĐỔI 1: Thêm dấu ? để không bắt buộc nhập ghi chú
        public string? ConditionNote { get; set; }

        [Required]
        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        // THAY ĐỔI 2: Thêm dấu ? để API không bắt gửi cả Object Product lên
        [ValidateNever] // Ngăn API check validation cho đối tượng này
        public virtual Product? Product { get; set; }

        public int? InboundTransactionId { get; set; }

        [ForeignKey("InboundTransactionId")]
        // THAY ĐỔI 3: Thêm dấu ?
        [ValidateNever]
        public virtual InventoryTransaction? InboundTransaction { get; set; }

        public int? OutboundTransactionId { get; set; }

        [ForeignKey("OutboundTransactionId")]
        // THAY ĐỔI 4: Thêm dấu ?
        [ValidateNever]
        public virtual InventoryTransaction? OutboundTransaction { get; set; }
    }
}