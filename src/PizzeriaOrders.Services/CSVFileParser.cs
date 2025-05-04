using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace PizzeriaOrders.Services;

public class CSVFileParser<T> : IFileParser<T>
{
    public List<T> Load(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            throw new ArgumentException("Path cannot be null or empty", nameof(path));
        }

        if (!File.Exists(path))
        {
            throw new FileNotFoundException("The specified file does not exist", path);
        }

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            HeaderValidated = null,
            MissingFieldFound = null,
        };

        using var reader = new StreamReader(path);
        using var csv = new CsvReader(reader, config);

        try
        {
            return csv.GetRecords<T>().ToList();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to parse CSV file", ex);
        }
    }
}