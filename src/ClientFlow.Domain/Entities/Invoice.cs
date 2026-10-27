using ClientFlow.Domain.Enums;
using ClientFlow.Domain.Exceptions;

namespace ClientFlow.Domain.Entities;

public class InvoiceItem
{
    public Guid Id { get; private set; }
    public Guid InvoiceId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal LineTotal => Math.Round(Quantity * UnitPrice, 2);

    private InvoiceItem() { }

    public static InvoiceItem Create(Guid invoiceId, string description, decimal quantity, decimal unitPrice)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new BusinessRuleValidationException(nameof(Description), "Item description is required.");
        if (quantity <= 0)
            throw new BusinessRuleValidationException(nameof(Quantity), "Quantity must be greater than 0.");
        if (unitPrice < 0)
            throw new BusinessRuleValidationException(nameof(UnitPrice), "Unit price cannot be negative.");

        return new InvoiceItem
        {
            Id = Guid.NewGuid(),
            InvoiceId = invoiceId,
            Description = description.Trim(),
            Quantity = quantity,
            UnitPrice = unitPrice
        };
    }
}

public class Invoice
{
    public Guid Id { get; private set; }
    public Guid ClientId { get; private set; }
    public Guid? ProjectId { get; private set; }
    public string InvoiceNumber { get; private set; } = string.Empty;
    public DateTime IssueDateUtc { get; private set; }
    public DateTime DueDateUtc { get; private set; }
    public decimal SubTotal { get; private set; }
    public decimal TaxRatePercentage { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public InvoiceStatus Status { get; private set; } = InvoiceStatus.Draft;
    public string Currency { get; private set; } = "USD";
    public string? Notes { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? PaidAtUtc { get; private set; }

    private readonly List<InvoiceItem> _items = new();
    public IReadOnlyCollection<InvoiceItem> Items => _items.AsReadOnly();

    private Invoice() { }

    public static Invoice Create(Guid clientId, Guid? projectId, string invoiceNumber, DateTime issueDateUtc, DateTime dueDateUtc, decimal taxRatePercentage = 0, string currency = "USD", string? notes = null)
    {
        if (clientId == Guid.Empty)
            throw new BusinessRuleValidationException(nameof(ClientId), "Client identifier is required.");
        if (string.IsNullOrWhiteSpace(invoiceNumber))
            throw new BusinessRuleValidationException(nameof(InvoiceNumber), "Invoice number is required.");
        if (dueDateUtc < issueDateUtc)
            throw new BusinessRuleValidationException(nameof(DueDateUtc), "Due date cannot be earlier than issue date.");
        if (taxRatePercentage < 0 || taxRatePercentage > 100)
            throw new BusinessRuleValidationException(nameof(TaxRatePercentage), "Tax rate must be between 0 and 100 percent.");

        return new Invoice
        {
            Id = Guid.NewGuid(),
            ClientId = clientId,
            ProjectId = projectId,
            InvoiceNumber = invoiceNumber.Trim().ToUpperInvariant(),
            IssueDateUtc = issueDateUtc.Date,
            DueDateUtc = dueDateUtc.Date,
            TaxRatePercentage = taxRatePercentage,
            Currency = string.IsNullOrWhiteSpace(currency) ? "USD" : currency.Trim().ToUpperInvariant(),
            Notes = notes?.Trim(),
            Status = InvoiceStatus.Draft,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void AddItem(string description, decimal quantity, decimal unitPrice)
    {
        if (Status == InvoiceStatus.Paid)
            throw new BusinessRuleValidationException(nameof(Status), "Cannot add items to an already paid invoice.");

        var item = InvoiceItem.Create(Id, description, quantity, unitPrice);
        _items.Add(item);
        RecalculateTotals();
    }

    public void ClearItems()
    {
        _items.Clear();
        RecalculateTotals();
    }

    public void RecalculateTotals()
    {
        SubTotal = Math.Round(_items.Sum(i => i.LineTotal), 2);
        TaxAmount = Math.Round(SubTotal * (TaxRatePercentage / 100m), 2);
        TotalAmount = SubTotal + TaxAmount;
    }

    public void MarkSent()
    {
        if (Status == InvoiceStatus.Paid) return;
        Status = InvoiceStatus.Sent;
    }

    public void MarkPaid()
    {
        Status = InvoiceStatus.Paid;
        PaidAtUtc = DateTime.UtcNow;
    }

    public void MarkCancelled()
    {
        if (Status == InvoiceStatus.Paid)
            throw new BusinessRuleValidationException(nameof(Status), "Cannot cancel an already paid invoice.");
        Status = InvoiceStatus.Cancelled;
    }
}
