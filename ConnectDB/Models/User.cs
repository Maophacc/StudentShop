using System.ComponentModel.DataAnnotations;

namespace ConnectDB.Models 
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; }

        [Required]
        [MaxLength(255)]
        public string PasswordHash { get; set; } // Thực tế phải mã hóa, không lưu password trơn nhé

        [MaxLength(255)]
        public string? ResetToken { get; set; }

        public DateTime? ResetTokenExpiry { get; set; }

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; }

        [Required]
        public int RoleId { get; set; }
        public virtual Role? Role { get; set; }

        // Navigation Property: 1 User có thể tạo nhiều Phiếu giao dịch
        public virtual ICollection<InventoryTransaction>? InventoryTransactions { get; set; }
    }
}