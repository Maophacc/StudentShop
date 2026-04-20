using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ConnectDB.Data;
using ConnectDB.Models;

namespace ConnectDB.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CartController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CartController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Cart/5
        [HttpGet("{userId}")]
        public async Task<ActionResult<object>> GetCart(int userId)
        {
            var cartItems = await _context.CartItems
                .Include(c => c.Product)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            return Ok(new { UserId = userId, CartItems = cartItems });
        }

        // POST: api/Cart
        [HttpPost]
        public async Task<IActionResult> AddToCart([FromBody] CartRequest request)
        {
            var product = await _context.Products.FindAsync(request.ProductId);
            if (product == null) return NotFound("Sản phẩm không tồn tại.");

            if (product.Stock < request.Quantity)
                return BadRequest($"Số lượng yêu cầu vượt quá kho (Còn {product.Stock}).");

            var existingItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.UserId == request.UserId && c.ProductId == request.ProductId);

            if (existingItem != null)
            {
                existingItem.Quantity += request.Quantity;
                if (product.Stock < existingItem.Quantity)
                    return BadRequest($"Tổng số lượng trong giỏ ({existingItem.Quantity}) vượt quá kho (Còn {product.Stock}).");
            }
            else
            {
                var cartItem = new CartItem
                {
                    UserId = request.UserId,
                    ProductId = request.ProductId,
                    Quantity = request.Quantity
                };
                _context.CartItems.Add(cartItem);
            }

            await _context.SaveChangesAsync();
            return Ok(new { Message = "Đã thêm vào giỏ hàng" });
        }

        // PUT: api/Cart/5
        [HttpPut("{cartItemId}")]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, [FromBody] int quantity)
        {
            var cartItem = await _context.CartItems.Include(c => c.Product).FirstOrDefaultAsync(c => c.CartItemId == cartItemId);
            if (cartItem == null) return NotFound();

            if (cartItem.Product.Stock < quantity)
                return BadRequest($"Kho chỉ còn {cartItem.Product.Stock} sản phẩm.");

            cartItem.Quantity = quantity;
            await _context.SaveChangesAsync();
            return Ok();
        }

        // DELETE: api/Cart/5
        [HttpDelete("{cartItemId}")]
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            var cartItem = await _context.CartItems.FindAsync(cartItemId);
            if (cartItem == null) return NotFound();

            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();
            return Ok();
        }

        // DELETE: api/Cart/user/5
        [HttpDelete("user/{userId}")]
        public async Task<IActionResult> ClearCart(int userId)
        {
            var items = await _context.CartItems.Where(c => c.UserId == userId).ToListAsync();
            _context.CartItems.RemoveRange(items);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }

    public class CartRequest
    {
        public int UserId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
