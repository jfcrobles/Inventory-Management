public class Movement
{
    public int ID { get; set; }
    public int ProductID { get; set; }
    public Product? Product { get; set; }
    public int SourceStoreID { get; set; }
    public Store? SourceStore { get; set; }
    public int TargetStoreID { get; set; }
    public Store? TargetStore { get; set; }
    public int Qty { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public MovementType Type { get; set; }
}

public enum MovementType
{
    IN,
    OUT,
    TRANSFER
}
