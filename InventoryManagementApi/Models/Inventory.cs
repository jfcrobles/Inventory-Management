public class Inventory
{
    public int ID { get; set; }
    public int ProductID { get; set; }
    public Product? Product { get; set; }
    public int StoreID { get; set; }
    public Store? Store { get; set; }
    public int Qty { get; set; }
    public int MinStock { get; set; }
}
