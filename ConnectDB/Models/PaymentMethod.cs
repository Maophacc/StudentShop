using System.ComponentModel.DataAnnotations;

namespace ConnectDB.Models
{
    public class PaymentMethod
    {
        [Key]
        public int PaymentMethodId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } // Cash, MoMo, VNPay, CreditCard, COD

        public string? Description { get; set; }
        
        public bool IsActive { get; set; }
    }
}
