namespace PizzeriaOrders.Models;

public class InvalidOrder
{
    public Order Order { get; set; }
    public string Reason { get; set; }
}
