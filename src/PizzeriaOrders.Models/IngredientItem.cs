namespace PizzeriaOrders.Models;

public class IngredientItem
{
    public decimal Quantity { get; set; }
    public UnitType Units { get; set; }
    public IngredientType Type { get; set; }
}