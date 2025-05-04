namespace PizzeriaOrders.Models;

public class Product
{
    public string ProductId { get; set; }
    public string ProductName { get; set; }
    public decimal Price { get; set; }
    public decimal VAT { get; set; }
}