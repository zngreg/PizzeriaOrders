using Newtonsoft.Json;

namespace PizzeriaOrders.Services;

public class JSONFileParser<T> : IFileParser<T>
{
    public List<T> Load(string path)
    {
        try
        {
            var json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<List<T>>(json) ?? new List<T>();
        }
        catch (JsonException)
        {
            return new List<T>();
        }
    }
}
