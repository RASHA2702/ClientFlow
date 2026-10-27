using ClientFlow.Domain.Entities;
using ClientFlow.Domain.Enums;
using ClientFlow.Domain.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClientFlow.Tests.Domain;

[TestClass]
public class TimeAndInvoiceTests
{
    [TestMethod]
    public void CreateTimeEntry_ValidHours_CalculatesProperly()
    {
        var entry = TimeEntry.Create(Guid.NewGuid(), Guid.NewGuid(), "u1", "Rashad", DateTime.UtcNow, 4.5m, true, "Unit testing");

        Assert.AreEqual(4.5m, entry.HoursWorked);
        Assert.IsTrue(entry.IsBillable);
        Assert.IsFalse(entry.IsInvoiced);
    }

    [TestMethod]
    [ExpectedException(typeof(BusinessRuleValidationException))]
    public void CreateTimeEntry_ZeroOrNegativeHours_ThrowsException()
    {
        TimeEntry.Create(Guid.NewGuid(), Guid.NewGuid(), "u1", "Rashad", DateTime.UtcNow, 0m, true, "Zero hours");
    }

    [TestMethod]
    [ExpectedException(typeof(BusinessRuleValidationException))]
    public void CreateTimeEntry_Over24Hours_ThrowsException()
    {
        TimeEntry.Create(Guid.NewGuid(), Guid.NewGuid(), "u1", "Rashad", DateTime.UtcNow, 25m, true, "Too many hours");
    }

    [TestMethod]
    public void Invoice_CalculationOfItemsAndTax_IsAccurate()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var invoice = Invoice.Create(clientId, null, "INV-2026-0001", DateTime.UtcNow, DateTime.UtcNow.AddDays(30), 20m, "USD");

        // Act: 10 hours @ $100 = $1,000; 5 hours @ $120 = $600. Total subtotal = $1,600.
        // Tax 20% on $1600 = $320. Total = $1,920.
        invoice.AddItem("Backend Development", 10m, 100m);
        invoice.AddItem("Architecture Review", 5m, 120m);

        // Assert
        Assert.AreEqual(1600m, invoice.SubTotal);
        Assert.AreEqual(320m, invoice.TaxAmount);
        Assert.AreEqual(1920m, invoice.TotalAmount);
        Assert.AreEqual(InvoiceStatus.Draft, invoice.Status);
    }

    [TestMethod]
    public void Invoice_StatusTransitions_EnforcedCorrectly()
    {
        var invoice = Invoice.Create(Guid.NewGuid(), null, "INV-2026-0002", DateTime.UtcNow, DateTime.UtcNow.AddDays(15));
        invoice.AddItem("Sprint 1", 1m, 5000m);

        invoice.MarkSent();
        Assert.AreEqual(InvoiceStatus.Sent, invoice.Status);

        invoice.MarkPaid();
        Assert.AreEqual(InvoiceStatus.Paid, invoice.Status);
        Assert.IsNotNull(invoice.PaidAtUtc);
    }

    [TestMethod]
    [ExpectedException(typeof(BusinessRuleValidationException))]
    public void Invoice_CannotCancelPaidInvoice()
    {
        var invoice = Invoice.Create(Guid.NewGuid(), null, "INV-2026-0003", DateTime.UtcNow, DateTime.UtcNow.AddDays(15));
        invoice.AddItem("Consulting", 1m, 1000m);
        invoice.MarkPaid();

        // Should throw
        invoice.MarkCancelled();
    }
}
