using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PizzeriaOrders.Models;

[JsonConverter(typeof(StringEnumConverter))]
public enum IngredientType
{
    Dairy,
    Meat,
    Vegetable,
    Grain,
    Other
}