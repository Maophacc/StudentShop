using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConnectDB.Models
{
    public class Bill
    {
        [Key]
        public int BillId { get; set; }

        [Required]
        public int OrderId { get; set; }
        [ForeignKey("OrderId")]
        public virtual SalesOrder? SalesOrder { get; set; }

        [Required]
        public decimal Subtotal { get; set; } // Dành cho tổng tiền hàng

        public decimal Discount { get; set; } // Tiền giảm giá từ voucher/promotion

        [Required]
        public decimal VAT { get; set; } // Tiền thuế

        [Required]
        public decimal Total { get; set; } // Tổng thanh toán = Sub - Disc + VAT

        [Required]
        public int PaymentMethodId { get; set; }
        [ForeignKey("PaymentMethodId")]
        public virtual PaymentMethod? PaymentMethod { get; set; }
        
        public DateTime CreatedAt { get; set; }
    }
}
