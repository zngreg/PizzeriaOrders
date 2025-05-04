namespace PizzeriaOrders.Services;

public interface IFileParser<T>
{
    List<T> Load(string path);
}