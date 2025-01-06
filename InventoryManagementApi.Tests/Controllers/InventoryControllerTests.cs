using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Xunit;
public class InventoryControllerTests
{
    private InventoryDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new InventoryDbContext(options);
    }

    [Fact]
    public async Task TransferStock_ShouldReturnBadRequest_WhenStockIsInsufficient()
    {
        var dbContext = GetInMemoryDbContext();
        dbContext.Inventory.Add(new Inventory { StoreID = 1, ProductID = 1, Qty = 5 });
        await dbContext.SaveChangesAsync();

        var controller = new InventoryController(dbContext);
        var transferRequest = new StockTransferDTO
        {
            SourceStoreId = 1,
            DestinationStoreId = 2,
            ProductId = 1,
            Quantity = 10
        };

        var result = await controller.TransferStock(transferRequest);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task TransferStock_ShouldTransferStockSuccessfully()
    {
        var dbContext = GetInMemoryDbContext();
        dbContext.Inventory.Add(new Inventory { StoreID = 1, ProductID = 1, Qty = 10 });
        await dbContext.SaveChangesAsync();

        var controller = new InventoryController(dbContext);
        var transferRequest = new StockTransferDTO
        {
            SourceStoreId = 1,
            DestinationStoreId = 2,
            ProductId = 1,
            Quantity = 5
        };

        var result = await controller.TransferStock(transferRequest);

        Assert.IsType<OkObjectResult>(result);

        var sourceInventory = await dbContext.Inventory.FirstOrDefaultAsync(i => i.StoreID == 1 && i.ProductID == 1);
        var destinationInventory = await dbContext.Inventory.FirstOrDefaultAsync(i => i.StoreID == 2 && i.ProductID == 1);

        Assert.Equal(5, sourceInventory.Qty);
        Assert.Equal(5, destinationInventory.Qty);
    }

    [Fact]
    public async Task TransferStock_ShouldFail_WhenInsufficientStock()
    {
        var dbContext = GetInMemoryDbContext();

        var sourceStore = new Store { ID = 1, Name = "Source Store", Location = "LocationTest", ManagerName = "ManagerNameTest" };
        var destinationStore = new Store { ID = 2, Name = "Destination Store", Location = "LocationTest", ManagerName = "ManagerNameTest"  };
        var product = new Product
        {
            Name = "Product 1",
            Price = 100,
            Category = "Test",
            Description = "Test Product",
            SKU = "SKU123"
        };

        dbContext.Stores.Add(sourceStore);
        dbContext.Stores.Add(destinationStore);
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        dbContext.Inventory.Add(new Inventory
        {
            StoreID = sourceStore.ID,
            ProductID = product.ID,
            Qty = 5
        });
        await dbContext.SaveChangesAsync();

        var controller = new InventoryController(dbContext);

        var transferRequest = new StockTransferDTO
        {
            SourceStoreId = sourceStore.ID,
            DestinationStoreId = destinationStore.ID,
            ProductId = product.ID,
            Quantity = 100
        };

        var result = await controller.TransferStock(transferRequest);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var errorMessage = badRequestResult.Value as string;

        Assert.Equal("Insufficient stock in source store.", errorMessage);
    }

    [Fact]
    public async Task GetLowStockAlerts_ShouldReturnProductsBelowThreshold()
    {
        var dbContext = GetInMemoryDbContext();
        dbContext.Inventory.AddRange(
            new Inventory { ProductID = 1, StoreID = 1, Qty = 5, MinStock = 10 },
            new Inventory { ProductID = 2, StoreID = 1, Qty = 15, MinStock = 10 }
        );
        dbContext.Products.Add(new Product { ID = 1, Name = "Product1", Price = 30, Category = "TestCategory", Description = "Test Description", SKU = "SKUTest" });
        dbContext.Products.Add(new Product { ID = 2, Name = "Product2", Price = 30, Category = "TestCategory2", Description = "Test Description2", SKU = "SKUTest2" });
        await dbContext.SaveChangesAsync();

        var controller = new InventoryController(dbContext);

        var result = await controller.GetLowStockAlerts();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var alerts = Assert.IsType<List<Inventory>>(okResult.Value);

        Assert.Single(alerts);
        Assert.Equal(1, alerts.First().ProductID);
    }
}