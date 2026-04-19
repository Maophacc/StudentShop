using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ConnectDB.Data;
using ConnectDB.Models;

namespace ConnectDB.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Product
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts(
            string? search, 
            int? categoryId, 
            int? brandId,
            string? sortBy = "productId",
            string? sortOrder = "desc",
            int pageNumber = 0,
            int pageSize = 100)
        {
            var query = _context.Products.Include(p => p.Category).Include(p => p.Brand).AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(p => p.Name.Contains(search) || p.ProductCode.Contains(search));

            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId);

            if (brandId.HasValue)
                query = query.Where(p => p.BrandId == brandId);

            // Sorting logic
            if (sortBy?.ToLower() == "random")
            {
                query = query.OrderBy(p => Guid.NewGuid());
            }
            else
            {
                bool isDesc = sortOrder?.ToLower() == "desc";
                switch (sortBy?.ToLower())
                {
                    case "name":
                        query = isDesc ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name);
                        break;
                    case "price":
                        query = isDesc ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price);
                        break;
                    case "discount":
                        // Giả sử có trường Discount hoặc tính toán gì đó, ở đây ví dụ sắp xếp theo Price nếu không có Discount
                        query = isDesc ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price);
                        break;
                    default:
                        query = isDesc ? query.OrderByDescending(p => p.ProductId) : query.OrderBy(p => p.ProductId);
                        break;
                }
            }

            // Pagination logic
            return await query
                .Skip(pageNumber * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        // GET: api/Product/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _context.Products.Include(p => p.Category).Include(p => p.Brand).FirstOrDefaultAsync(p => p.ProductId == id);
            if (product == null) return NotFound();
            return product;
        }

        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetProduct), new { id = product.ProductId }, product);
        }

        [HttpPut("{id}/stock")]
        public async Task<IActionResult> UpdateStock(int id, [FromBody] int newStock)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            product.Stock = newStock;
            product.Status = newStock > 0 ? "InStock" : "OutOfStock";
            
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, Product product)
        {
            if (id != product.ProductId) return BadRequest("ProductId mismatch");

            var existingProduct = await _context.Products.FindAsync(id);
            if (existingProduct == null) return NotFound();

            existingProduct.Name = product.Name;
            existingProduct.ProductCode = product.ProductCode;
            existingProduct.Description = product.Description;
            existingProduct.Price = product.Price;
            existingProduct.Stock = product.Stock;
            existingProduct.Status = product.Stock > 0 ? "InStock" : "OutOfStock";
            existingProduct.CategoryId = product.CategoryId;
            existingProduct.BrandId = product.BrandId;
            existingProduct.ImageUrl = product.ImageUrl;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}