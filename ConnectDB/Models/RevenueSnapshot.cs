using System.ComponentModel.DataAnnotations;

namespace ConnectDB.Models
{
    public class RevenueSnapshot
    {
        [Key]
        public int SnapshotId { get; set; }

        public DateTime Date { get; set; } // Ngày thống kê

        public decimal TotalRevenue { get; set; } // Tổng doanh thu trong ngày
        
        public int TotalOrders { get; set; } // Số lượng đơn hàng
        
        public int TotalCustomers { get; set; } // Khách hàng
    }
}
