namespace PizzeriaOrders.Models;

public class OrderProduct
{
    public string ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
}