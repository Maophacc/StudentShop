using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ConnectDB.Data;
using ConnectDB.Models;

namespace ConnectDB.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReportController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("generate-daily-snapshot")]
        public async Task<IActionResult> GenerateDailySnapshot(DateTime date)
        {
            var startOfDay = date.Date;
            var endOfDay = startOfDay.AddDays(1);

            var dailyBills = await _context.Bills
                .Include(b => b.SalesOrder)
                .Where(b => b.CreatedAt >= startOfDay && b.CreatedAt < endOfDay)
                .ToListAsync();

            var snapshot = new RevenueSnapshot
            {
                Date = startOfDay,
                TotalRevenue = dailyBills.Sum(b => b.Total),
                TotalOrders = dailyBills.Select(b => b.OrderId).Distinct().Count(),
                TotalCustomers = dailyBills.Where(b => b.SalesOrder.CustomerId != null).Select(b => b.SalesOrder.CustomerId).Distinct().Count()
            };

            _context.RevenueSnapshots.Add(snapshot);
            await _context.SaveChangesAsync();

            return Ok(snapshot);
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var totalRevenue = await _context.Bills.SumAsync(b => b.Total);
            var activeProducts = await _context.Products.CountAsync(p => p.Status == "InStock");
            var pendingOrders = await _context.SalesOrders.CountAsync(o => o.Status == "Pending");

            return Ok(new {
                TotalRevenue = totalRevenue,
                ActiveProducts = activeProducts,
                PendingOrders = pendingOrders
            });
        }
    }
}
