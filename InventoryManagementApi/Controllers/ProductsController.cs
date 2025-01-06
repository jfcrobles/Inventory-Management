using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class ProductsController : ControllerBase
{
    private readonly InventoryDbContext _context;

    public ProductsController(InventoryDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts([FromQuery] string? category = null, [FromQuery] decimal? minPrice = null, [FromQuery] decimal? maxPrice = null, [FromQuery] int? stock = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (page <= 0 || pageSize <= 0)
        {
            return BadRequest("Page and pageSize must be greater than zero.");
        }

        var query = _context.Products.AsQueryable();

        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(p => p.Category == category);
        }

        if (minPrice.HasValue)
        {
            query = query.Where(p => p.Price >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= maxPrice.Value);
        }

        if (stock.HasValue)
        {
            query = query.Where(p => _context.Inventory.Any(i => i.ProductID == p.ID && i.Qty == stock.Value));
        }

        var totalItems = await query.CountAsync();
        var products = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var result = new ProductResponse
        {
            TotalItems = totalItems,
            Page = page,
            PageSize = pageSize,
            Products = products
        };

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return NotFound();
        return Ok(product);
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] Product product)
    {
        if (!ModelState.IsValid)
        {
            var errorMessages = string.Join("; ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));
            return BadRequest(errorMessages); 
        }
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetProduct), new { id = product.ID }, product);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] Product product)
    {
        if (id != product.ID) return BadRequest();

        var existingProduct = await _context.Products.FindAsync(id);

        if (existingProduct == null) return NotFound();

        _context.Entry(existingProduct).CurrentValues.SetValues(product);

        await _context.SaveChangesAsync();

        return Ok(existingProduct);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return NotFound();
        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        return Ok();
    }

}

public class ProductResponse
{
    public int TotalItems { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public List<Product>? Products { get; set; }
}
