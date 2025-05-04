namespace PizzeriaOrders.Models;

public class Order
{
    public string OrderId { get; set; }
    public IList<OrderProduct>? Products { get; set; }
    public DateTime DeliverAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CustomerAddress { get; set; }
    public decimal GrossPrice { get; set; }
    public decimal VATAmount { get; set; }
    public decimal TotalPrice { get; set; }
}
