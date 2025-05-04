using Newtonsoft.Json;

namespace PizzeriaOrders.Services.Unit.Tests;

[TestFixture]
public class JSONFileParserTests
{
    private string _testFilePath;

    [SetUp]
    public void SetUp()
    {
        _testFilePath = Path.GetTempFileName();
    }

    [TearDown]
    public void TearDown()
    {
        if (File.Exists(_testFilePath))
        {
            File.Delete(_testFilePath);
        }
    }

    [Test]
    public void Load_ValidJsonFile_ReturnsDeserializedList()
    {
        // Arrange
        var testData = new List<TestItem> { new TestItem { Id = 1, Name = "Test" } };
        File.WriteAllText(_testFilePath, JsonConvert.SerializeObject(testData));
        var parser = new JSONFileParser<TestItem>();

        // Act
        var result = parser.Load(_testFilePath);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].Id, Is.EqualTo(1));
        Assert.That(result[0].Name, Is.EqualTo("Test"));
    }

    [Test]
    public void Load_InvalidJsonFile_ReturnsEmptyList()
    {
        // Arrange
        File.WriteAllText(_testFilePath, "Invalid JSON");
        var parser = new JSONFileParser<TestItem>();

        // Act
        var result = parser.Load(_testFilePath);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(0));
    }

    [Test]
    public void Load_EmptyJsonFile_ReturnsEmptyList()
    {
        // Arrange
        File.WriteAllText(_testFilePath, "[]");
        var parser = new JSONFileParser<TestItem>();

        // Act
        var result = parser.Load(_testFilePath);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(0));
    }

    private class TestItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}