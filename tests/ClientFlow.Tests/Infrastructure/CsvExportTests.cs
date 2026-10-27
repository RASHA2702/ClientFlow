using System.Text;
using ClientFlow.Application.DTOs;
using ClientFlow.Domain.Enums;
using ClientFlow.Infrastructure.Export;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClientFlow.Tests.Infrastructure;

[TestClass]
public class CsvExportTests
{
    [TestMethod]
    public void ExportTimeEntries_GeneratesValidCsvHeaderAndRows()
    {
        var exporter = new CsvExporter();
        var entries = new List<TimeEntryDto>
        {
            new TimeEntryDto(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "API Optimization",
                Guid.NewGuid(),
                "Apex FinTech",
                "usr_1",
                "Rashad",
                new DateTime(2026, 10, 27),
                8.0m,
                true,
                "Refactored LINQ queries",
                false,
                null,
                DateTime.UtcNow
            )
        };

        var bytes = exporter.ExportTimeEntries(entries);
        var csv = Encoding.UTF8.GetString(bytes);

        Assert.IsTrue(csv.Contains("Id,Date,Project,Task,User,Hours,Billable,Invoiced,Notes"));
        Assert.IsTrue(csv.Contains("API Optimization"));
        Assert.IsTrue(csv.Contains("Apex FinTech"));
        Assert.IsTrue(csv.Contains("Rashad"));
        Assert.IsTrue(csv.Contains("8.0"));
    }

    [TestMethod]
    public void ExportInvoices_GeneratesValidCsvHeaderAndRows()
    {
        var exporter = new CsvExporter();
        var invoices = new List<InvoiceDto>
        {
            new InvoiceDto(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "Nordic HealthTech",
                Guid.NewGuid(),
                "Telemetry Portal",
                "INV-2026-1002",
                new DateTime(2026, 10, 1),
                new DateTime(2026, 10, 31),
                10000m,
                20m,
                2000m,
                12000m,
                InvoiceStatus.Paid,
                "EUR",
                "October sprint invoice",
                DateTime.UtcNow,
                DateTime.UtcNow,
                new List<InvoiceItemDto>()
            )
        };

        var bytes = exporter.ExportInvoices(invoices);
        var csv = Encoding.UTF8.GetString(bytes);

        Assert.IsTrue(csv.Contains("InvoiceNumber,Client,Project,IssueDate,DueDate,SubTotal,TaxAmount,TotalAmount,Currency,Status"));
        Assert.IsTrue(csv.Contains("INV-2026-1002"));
        Assert.IsTrue(csv.Contains("Nordic HealthTech"));
        Assert.IsTrue(csv.Contains("12000"));
        Assert.IsTrue(csv.Contains("Paid"));
    }

    [TestMethod]
    [DataRow("=SUM(A1:A10)", "'=SUM(A1:A10)")]
    [DataRow("+cmd|' /C calc'!A0", "'+cmd|' /C calc'!A0")]
    [DataRow("-123", "'-123")]
    [DataRow("@SUM(A1)", "'@SUM(A1)")]
    [DataRow("@SUM(1,2)", "\"'@SUM(1,2)\"")]
    [DataRow("\tmalicious", "'\tmalicious")]
    public void EscapeCsv_SanitizesFormulaInjectionPrefixes_PrependQuote(string input, string expected)
    {
        var sanitized = CsvExporter.EscapeCsv(input);
        Assert.AreEqual(expected, sanitized);
    }

    [TestMethod]
    public void ExportTimeEntries_WithFormulaInjectionPayload_NeutralizesPayload()
    {
        var exporter = new CsvExporter();
        var entries = new List<TimeEntryDto>
        {
            new TimeEntryDto(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "=HYPERLINK(\"http://evil.com\")",
                Guid.NewGuid(),
                "Apex FinTech",
                "usr_1",
                "Rashad",
                new DateTime(2026, 10, 27),
                4.0m,
                true,
                "@dangerous_command",
                false,
                null,
                DateTime.UtcNow
            )
        };

        var bytes = exporter.ExportTimeEntries(entries);
        var csv = Encoding.UTF8.GetString(bytes);

        // Verify prefixes are prepended with single-quote to prevent Excel / Sheets formula execution
        Assert.IsTrue(csv.Contains("\"'=HYPERLINK(\"\"http://evil.com\"\")\""));
        Assert.IsTrue(csv.Contains("'@dangerous_command"));
    }
}
