using NUnit.Framework;
using PizzeriaOrders.Services;
using System;
using System.Collections.Generic;
using System.IO;

namespace PizzeriaOrders.Services.Unit.Tests;

[TestFixture]
public class CSVFileParserTests
{
    private string _validCsvPath;
    private string _invalidCsvPath;
    private string _nonExistentPath;

    [SetUp]
    public void SetUp()
    {
        _validCsvPath = Path.GetTempFileName();
        File.WriteAllText(_validCsvPath, "Id,Name\n1,Test\n2,Sample");

        _invalidCsvPath = Path.GetTempFileName();
        File.WriteAllText(_invalidCsvPath, "Invalid CSV Content");

        _nonExistentPath = Path.Combine(Path.GetTempPath(), "nonexistent.csv");
    }

    [TearDown]
    public void TearDown()
    {
        if (File.Exists(_validCsvPath)) File.Delete(_validCsvPath);
        if (File.Exists(_invalidCsvPath)) File.Delete(_invalidCsvPath);
    }

    [Test]
    public void Load_ValidCsv_ReturnsRecords()
    {
        var parser = new CSVFileParser<TestRecord>();
        var records = parser.Load(_validCsvPath);

        Assert.That(2, Is.EqualTo(records.Count));
        Assert.That("Test", Is.EqualTo(records[0].Name));
        Assert.That("Sample", Is.EqualTo(records[1].Name));
    }

    [Test]
    public void Load_NullOrEmptyPath_ThrowsArgumentException()
    {
        var parser = new CSVFileParser<TestRecord>();

        Assert.Throws<ArgumentException>(() => parser.Load(null));
        Assert.Throws<ArgumentException>(() => parser.Load(string.Empty));
    }

    [Test]
    public void Load_NonExistentPath_ThrowsFileNotFoundException()
    {
        var parser = new CSVFileParser<TestRecord>();

        Assert.Throws<FileNotFoundException>(() => parser.Load(_nonExistentPath));
    }

    [Test]
    public void Load_InvalidCsv_ReturnsNoRecords()
    {
        var parser = new CSVFileParser<TestRecord>();

        var records = parser.Load(_invalidCsvPath);
        Assert.That(records, Is.Empty);
    }

    private class TestRecord
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}