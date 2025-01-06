using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/stores/{storeId}/inventory")]
public class StoreInventoryController : ControllerBase
{
    private readonly InventoryDbContext _context;

    public StoreInventoryController(InventoryDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetInventoryByStore(int storeId)
    {
        var inventory = await _context.Inventory
            .Where(i => i.StoreID == storeId)
            .Include(i => i.Product)
            .ToListAsync();

        if (!inventory.Any()) return NotFound("No inventory found for the specified store.");
        return Ok(inventory);
    }

    [HttpPut("{productId}/minstock")]
    public async Task<IActionResult> UpdateMinStock(int storeId, int productId, [FromBody] int newMinStock)
    {
        if (newMinStock < 0)
        {
            return BadRequest("Minimum stock must be a non-negative value.");
        }

        var storeExists = await _context.Stores.AnyAsync(s => s.ID == storeId);
        if (!storeExists)
        {
            return NotFound("Store not found.");
        }

        var productExists = await _context.Products.AnyAsync(p => p.ID == productId);
        if (!productExists)
        {
            return NotFound("Product not found.");
        }

        var inventory = await _context.Inventory.FirstOrDefaultAsync(i => i.StoreID == storeId && i.ProductID == productId);
        if (inventory == null)
        {
            return NotFound("This store does not handle the specified product.");
        }

        inventory.MinStock = newMinStock;
        await _context.SaveChangesAsync();

        return Ok("Minimum stock updated successfully.");
    }
}