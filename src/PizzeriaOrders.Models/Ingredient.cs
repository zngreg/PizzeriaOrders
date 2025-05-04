namespace PizzeriaOrders.Models;

public class Ingredient
{
    public string ProductId { get; set; }
    public Dictionary<string, IngredientItem> Ingredients { get; set; }
}