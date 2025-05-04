using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using PizzeriaOrders.Models;
using PizzeriaOrders.Services;

Console.WriteLine("Enter file type (json/csv), default is json:");
var fileType = Console.ReadLine()?.ToLower() ?? "json";

List<string> supportedFileTypes = new List<string> { "json", "csv" };
if (!supportedFileTypes.Contains(fileType) && !string.IsNullOrWhiteSpace(fileType))
{
    Console.WriteLine($"Unsupported file type '{fileType}'. Supported types are: {string.Join(", ", supportedFileTypes)}");
    return;
}
Console.WriteLine($"Using file type: {fileType}");

IList<Order> orders = new List<Order>();

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

var filePaths = configuration.GetSection("FilePaths").Get<FilePaths>();

var serviceProvider = new ServiceCollection()
    .AddLogging(configure => configure.AddConsole())
    .AddSingleton<IFileParser<Product>>(provider =>
    {
        return new JSONFileParser<Product>();
    })
    .AddSingleton<IFileParser<Ingredient>>(provider =>
    {
        return new JSONFileParser<Ingredient>();
    })
    .AddSingleton<IFileParser<Order>>(provider =>
    {
        IFileParser<Order> fileParser = fileType == "csv" ? new CSVFileParser<Order>() : new JSONFileParser<Order>();
        orders = fileParser.Load(fileType == "csv" ? filePaths.OrdersCsv : filePaths.OrdersJson);
        return fileParser;
    })
    .AddSingleton<IOrderValidator>(provider =>
    {
        var fileParser = provider.GetService<IFileParser<Product>>() ?? throw new InvalidOperationException("FileParser for Product is not configured.");
        var products = fileParser.Load(filePaths.Products);
        var logger = provider.GetService<ILogger<OrderValidator>>() ?? throw new InvalidOperationException("Logger for OrderValidator is not configured.");
        return new OrderValidator(products, logger);
    })
    .AddSingleton<IPriceCalculator>(provider =>
    {
        var fileParser = provider.GetService<IFileParser<Product>>() ?? throw new InvalidOperationException("FileParser for Product is not configured.");
        var products = fileParser.Load(filePaths.Products);
        var logger = provider.GetService<ILogger<PriceCalculator>>() ?? throw new InvalidOperationException("Logger for PriceCalculator is not configured.");
        return new PriceCalculator(products, logger);
    })
    .AddSingleton<IIngredientAggregator>(provider =>
    {
        var fileParser = provider.GetService<IFileParser<Ingredient>>() ?? throw new InvalidOperationException("FileParser for Product is not configured.");
        var ingredients = fileParser.Load(filePaths.Ingredients);
        var logger = provider.GetService<ILogger<IngredientAggregator>>() ?? throw new InvalidOperationException("Logger for IngredientAggregator is not configured.");
        return new IngredientAggregator(ingredients, logger);
    })
    .AddSingleton<IQueueService, MockQueueService>()
    .AddSingleton<IOrderProcessor, OrderProcessor>()
    .BuildServiceProvider();

var orderProcessor = serviceProvider.GetService<IOrderProcessor>();

orderProcessor?.ProcessOrders(orders);