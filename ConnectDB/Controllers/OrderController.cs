using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ConnectDB.Data;
using ConnectDB.Models;

namespace ConnectDB.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrderController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Create SalesOrder
                var order = new SalesOrder
                {
                    OrderDate = DateTime.UtcNow,
                    Status = "Pending",
                    CustomerId = request.CustomerId,
                    OrderDetails = new List<OrderDetail>()
                };

                decimal subtotal = 0;

                // Process Cart Items
                foreach (var item in request.Items)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product == null || product.Stock < item.Quantity)
                        return BadRequest($"Product {item.ProductId} is out of stock or does not exist.");

                    // Deduct stock
                    product.Stock -= item.Quantity;
                    if (product.Stock == 0) product.Status = "OutOfStock";

                    var detail = new OrderDetail
                    {
                        ProductId = product.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = product.Price
                    };

                    subtotal += detail.UnitPrice * item.Quantity;
                    order.OrderDetails.Add(detail);
                }

                _context.SalesOrders.Add(order);
                await _context.SaveChangesAsync();

                // Generate Bill
                decimal discount = 0;
                // Simplified apply voucher logic
                if (!string.IsNullOrEmpty(request.VoucherCode))
                {
                    var voucher = await _context.Vouchers.Include(v => v.Promotion).FirstOrDefaultAsync(v => v.Code == request.VoucherCode && v.IsActive);
                    if (voucher != null && voucher.CurrentUsages < voucher.MaxUsages)
                    {
                        if (voucher.Promotion.Type == "Percentage")
                            discount = subtotal * (voucher.Promotion.Value / 100);
                        else if (voucher.Promotion.Type == "FixedAmount")
                            discount = voucher.Promotion.Value;

                        voucher.CurrentUsages++;
                    }
                }

                decimal vat = (subtotal - discount) * 0.1m; // 10% VAT
                decimal total = subtotal - discount + vat;

                var bill = new Bill
                {
                    OrderId = order.OrderId,
                    Subtotal = subtotal,
                    Discount = discount,
                    VAT = vat,
                    Total = total,
                    PaymentMethodId = request.PaymentMethodId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Bills.Add(bill);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { OrderId = order.OrderId, TotalAmount = total, BillDetails = bill });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, ex.Message);
            }
        }

        // GET: api/Order
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SalesOrder>>> GetOrders()
        {
            return await _context.SalesOrders
                .Include(o => o.Customer)
                .Include(o => o.Bill)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        // GET: api/Order/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SalesOrder>> GetOrder(int id)
        {
            var order = await _context.SalesOrders
                .Include(o => o.Customer)
                .Include(o => o.Bill)
                .Include(o => o.OrderDetails!)
                    .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null) return NotFound();

            return order;
        }
    }

    public class CheckoutRequest
    {
        public int? CustomerId { get; set; }
        public int PaymentMethodId { get; set; }
        public string? VoucherCode { get; set; }
        public List<CartItem> Items { get; set; }
    }

    public class CartItem
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
