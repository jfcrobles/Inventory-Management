using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Xunit;
public class ProductsControllerTests
{
    private InventoryDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new InventoryDbContext(options);
    }

    [Fact]
    public async Task GetProducts_ShouldReturnAllProducts_WhenNoFiltersApplied()
    {
        var dbContext = GetInMemoryDbContext();
        dbContext.Products.AddRange(
            new Product { ID = 1, Name = "Product1", Price = 10, Category = "TestCategory", Description = "Test Description", SKU = "SKUTest" },
            new Product { ID = 2, Name = "Product2", Price = 20, Category = "TestCategory2", Description = "Test Description2", SKU = "SKUTest2" }
        );
        await dbContext.SaveChangesAsync();

        var controller = new ProductsController(dbContext);

        var result = await controller.GetProducts(null, null, null, null);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsAssignableFrom<ProductResponse>(okResult.Value);

        Assert.Equal(2, response.TotalItems);
    }

    [Fact]
    public async Task GetProducts_ShouldReturnFilteredProducts()
    {
        var dbContext = GetInMemoryDbContext();
        dbContext.Products.AddRange(
            new Product { Name = "Product A", Description = "Test Description", Category = "Category1", Price = 50, SKU = "SKUTest" },
            new Product { Name = "Product B", Description = "Test Description2", Category = "Category2", Price = 100, SKU = "SKUTest2" }
        );
        await dbContext.SaveChangesAsync();

        var controller = new ProductsController(dbContext);

        var result = await controller.GetProducts(category: "Category1");

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsAssignableFrom<dynamic>(okResult.Value);
        Assert.Single(response.Products); // Solo un producto debe coincidir
        Assert.Equal("Product A", response.Products[0].Name);
    }

    [Fact]
    public async Task GetProduct_ShouldReturnNotFound_WhenProductDoesNotExist()
    {
        var dbContext = GetInMemoryDbContext();
        var controller = new ProductsController(dbContext);

        var result = await controller.GetProduct(999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task CreateProduct_ShouldAddProductToDatabase()
    {
        var dbContext = GetInMemoryDbContext();
        var controller = new ProductsController(dbContext);

        var newProduct = new Product
        {
            Name = "NewProduct",
            Price = 30,
            Category = "TestCategory",
            Description = "Test Description",
            SKU = "SKU123"
        };

        var result = await controller.CreateProduct(newProduct);

        var createdAtResult = Assert.IsType<CreatedAtActionResult>(result);
        var product = Assert.IsType<Product>(createdAtResult.Value);

        Assert.Equal("NewProduct", product.Name);
        Assert.Single(dbContext.Products);
    }

    [Fact]
    public async Task GetProduct_ShouldReturnProduct_WhenValidIdIsProvided()
    {
        var dbContext = GetInMemoryDbContext();
        var product = new Product
        {
            ID = 1,
            Name = "Product 1",
            Price = 10,
            Category = "TestCategory",
            Description = "Test Product",
            SKU = "SKU123"
        };
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        var controller = new ProductsController(dbContext);

        var result = await controller.GetProduct(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedProduct = Assert.IsType<Product>(okResult.Value);
        Assert.Equal("Product 1", returnedProduct.Name);
    }

    [Fact]
    public async Task UpdateProduct_ShouldFail_WhenIdDoesNotMatch()
    {
        var dbContext = GetInMemoryDbContext();
        var product = new Product
        {
            ID = 1,
            Name = "Old Product",
            Price = 50,
            Category = "TestCategory",
            Description = "Test Product",
            SKU = "SKU123"
        };
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        var controller = new ProductsController(dbContext);

        var updatedProduct = new Product
        {
            ID = 2,
            Name = "Updated Product",
            Price = 100,
            Category = "TestCategory",
            Description = "Test Product",
            SKU = "SKU123"
        };

        var result = await controller.UpdateProduct(1, updatedProduct);

        var badRequestResult = Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task UpdateProduct_ShouldUpdateProductInDatabase()
    {
        // Arrange
        var dbContext = GetInMemoryDbContext();
        var product = new Product
        {
            ID = 1,
            Name = "Old Product",
            Price = 15,
            Category = "TestCategory",
            Description = "Old Product Description",
            SKU = "SKU126"
        };
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        var controller = new ProductsController(dbContext);
        var updatedProduct = new Product
        {
            ID = 1,
            Name = "Updated Product",
            Price = 20,
            Category = "Updated Category",
            Description = "Updated Description",
            SKU = "SKU127"
        };

        // Act
        var result = await controller.UpdateProduct(1, updatedProduct);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedProduct = Assert.IsType<Product>(okResult.Value);
        Assert.Equal("Updated Product", returnedProduct.Name);
        Assert.Equal(20, returnedProduct.Price);  // Verifica que el precio haya sido actualizado
    }

    [Fact]
    public async Task DeleteProduct_ShouldRemoveProductFromDatabase()
    {
        var dbContext = GetInMemoryDbContext();
        var product = new Product
        {
            ID = 1,
            Name = "Product to be deleted",
            Price = 10,
            Category = "TestCategory",
            Description = "Test Product to be deleted",
            SKU = "SKU128"
        };
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        var controller = new ProductsController(dbContext);

        var result = await controller.DeleteProduct(1);

        Assert.IsType<OkResult>(result);
        Assert.Empty(dbContext.Products);
    }

    [Fact]
    public async Task DeleteProduct_ShouldFail_WhenProductDoesNotExist()
    {
        var dbContext = GetInMemoryDbContext();
        var controller = new ProductsController(dbContext);

        var result = await controller.DeleteProduct(999);

        var notFoundResult = Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetProducts_ShouldReturnPaginatedResults()
    {
        var dbContext = GetInMemoryDbContext();
        for (int i = 1; i <= 20; i++)
        {
            dbContext.Products.Add(new Product
            {
                Name = $"Product {i}",
                Price = i * 10,
                Category = $"TestCategory {i}",
                Description = $"Test Product to be deleted {i}",
                SKU = $"SKU128 {i}"
            });
        }
        await dbContext.SaveChangesAsync();

        var controller = new ProductsController(dbContext);

        var result = await controller.GetProducts(page: 2, pageSize: 5);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsAssignableFrom<dynamic>(okResult.Value);

        Assert.Equal(5, response.Products.Count);
        Assert.Equal("Product 6", response.Products[0].Name);
    }

    [Fact]
    public async Task CreateProduct_ShouldFail_WhenDataIsInvalid()
    {
        var dbContext = GetInMemoryDbContext();
        var controller = new ProductsController(dbContext);

        var invalidProduct = new Product
        {
            Name = "",
            Price = -10,
            Category = "TestCategory",
            Description = "Test Description",
            SKU = "SKU123"
        };

        controller.ModelState.AddModelError("Name", "The Name field is required.");
        controller.ModelState.AddModelError("Name", "The field Name must be a string with a minimum length of 1 and a maximum length of 100.");
        controller.ModelState.AddModelError("Price", "Price must be greater than 0.");

        var result = await controller.CreateProduct(invalidProduct);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var response = badRequestResult.Value as dynamic;
        Assert.Contains("The Name field is required.", response);
        Assert.Contains("The field Name must be a string with a minimum length of 1 and a maximum length of 100.", response);
        Assert.Contains("Price must be greater than 0.", response);
    }

}