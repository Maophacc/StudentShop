using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ConnectDB.Models
{
    public class SalesOrder
    {
        [Key]
        public int OrderId { get; set; }

        public DateTime OrderDate { get; set; } // Thời gian đặt hàng

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } // Pending, Paid, Shipped, Cancelled

        // Người mua online
        public int? CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public virtual Customer? Customer { get; set; }

        // Nhân viên xử lý (có thể null nếu order online tự động tự vào hệ thống)
        public int? UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        [ValidateNever]
        public virtual ICollection<OrderDetail>? OrderDetails { get; set; }
        
        [ValidateNever]
        public virtual Bill? Bill { get; set; }
    }
}
