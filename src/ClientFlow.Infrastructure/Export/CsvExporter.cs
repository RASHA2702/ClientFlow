using System.Text;
using ClientFlow.Application.DTOs;

namespace ClientFlow.Infrastructure.Export;

public interface ICsvExporter
{
    byte[] ExportTimeEntries(IReadOnlyList<TimeEntryDto> entries);
    byte[] ExportInvoices(IReadOnlyList<InvoiceDto> invoices);
}

public class CsvExporter : ICsvExporter
{
    public byte[] ExportTimeEntries(IReadOnlyList<TimeEntryDto> entries)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Id,Date,Project,Task,User,Hours,Billable,Invoiced,Notes");

        foreach (var e in entries)
        {
            var notes = EscapeCsv(e.Notes ?? "");
            var project = EscapeCsv(e.ProjectName);
            var task = EscapeCsv(e.TaskTitle);
            var user = EscapeCsv(e.UserName);

            sb.AppendLine($"{e.Id},{e.DateUtc:yyyy-MM-dd},{project},{task},{user},{e.HoursWorked},{e.IsBillable},{e.IsInvoiced},{notes}");
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    public byte[] ExportInvoices(IReadOnlyList<InvoiceDto> invoices)
    {
        var sb = new StringBuilder();
        sb.AppendLine("InvoiceNumber,Client,Project,IssueDate,DueDate,SubTotal,TaxAmount,TotalAmount,Currency,Status");

        foreach (var inv in invoices)
        {
            var client = EscapeCsv(inv.ClientName);
            var project = EscapeCsv(inv.ProjectName ?? "N/A");

            sb.AppendLine($"{inv.InvoiceNumber},{client},{project},{inv.IssueDateUtc:yyyy-MM-dd},{inv.DueDateUtc:yyyy-MM-dd},{inv.SubTotal},{inv.TaxAmount},{inv.TotalAmount},{inv.Currency},{inv.Status}");
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private static string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value)) return "\"\"";
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }
        return value;
    }
}
