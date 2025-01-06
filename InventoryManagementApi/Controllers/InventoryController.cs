using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

 [ApiController]
    [Route("api/inventory")]
    public class InventoryController : ControllerBase
    {
        private readonly InventoryDbContext _context;

        public InventoryController(InventoryDbContext context)
        {
            _context = context;
        }

        [HttpPost("transfer")]
        public async Task<IActionResult> TransferStock([FromBody] StockTransferDTO transferRequest)
        {
            if (transferRequest == null) return BadRequest("Invalid transfer request.");

            var sourceInventory = await _context.Inventory
                .FirstOrDefaultAsync(i => i.StoreID == transferRequest.SourceStoreId && i.ProductID == transferRequest.ProductId);

            if (sourceInventory == null || sourceInventory.Qty < transferRequest.Quantity)
            {
                return BadRequest("Insufficient stock in source store.");
            }

            sourceInventory.Qty -= transferRequest.Quantity;

            var destinationInventory = await _context.Inventory
                .FirstOrDefaultAsync(i => i.StoreID == transferRequest.DestinationStoreId && i.ProductID == transferRequest.ProductId);

            if (destinationInventory == null)
            {
                destinationInventory = new Inventory
                {
                    StoreID = transferRequest.DestinationStoreId,
                    ProductID = transferRequest.ProductId,
                    Qty = transferRequest.Quantity
                };
                _context.Inventory.Add(destinationInventory);
            }
            else
            {
                destinationInventory.Qty += transferRequest.Quantity;
            }

             var movement = new Movement
            {
                ProductID = transferRequest.ProductId,
                SourceStoreID = transferRequest.SourceStoreId,
                TargetStoreID = transferRequest.DestinationStoreId,
                Qty = transferRequest.Quantity,
                Timestamp = DateTime.UtcNow,
                Type = MovementType.TRANSFER
            };
            _context.Movements.Add(movement);

            await _context.SaveChangesAsync();
            return Ok("Stock transfer successful.");
        }

        [HttpGet("alerts")]
        public async Task<IActionResult> GetLowStockAlerts()
        {
            var lowStockItems = await _context.Inventory
                .Where(i => i.Qty <= i.MinStock)
                .Include(i => i.Product)
                .ToListAsync();

            //if (!lowStockItems.Any()) return Ok("No products with low stock.");
            return Ok(lowStockItems);
        }
    }

    public class StockTransferDTO
    {
        public int SourceStoreId { get; set; }
        public int DestinationStoreId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

