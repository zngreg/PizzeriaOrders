using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;

namespace PizzeriaOrders.Models;

[JsonConverter(typeof(StringEnumConverter))]
public enum UnitType
{
    Kilograms,
    Liters,
    Pieces
}