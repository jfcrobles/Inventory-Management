using System.ComponentModel.DataAnnotations;

public class Product
{
    public int ID { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 1)]
    public required string Name { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
    public decimal Price { get; set; }

    [Required]
    public required string Category { get; set; }

    public required string Description { get; set; }

    [Required]
    [StringLength(50)]
    public required string SKU { get; set; }
}