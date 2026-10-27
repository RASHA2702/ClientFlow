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

    [TestMethod]
    public void Invoice_CommercialRoundingAwayFromZero_RoundsHalfUp()
    {
        // Arrange: 1.01 quantity * $2.50 unit price = 2.525.
        // Banker's rounding (ToEven) produces 2.52.
        // Commercial rounding (AwayFromZero) produces 2.53.
        var invoice = Invoice.Create(Guid.NewGuid(), null, "INV-ROUND-1", DateTime.UtcNow, DateTime.UtcNow.AddDays(7));
        invoice.AddItem("Precision Item", 1.01m, 2.50m);

        Assert.AreEqual(2.53m, invoice.Items.First().LineTotal);
        Assert.AreEqual(2.53m, invoice.SubTotal);
    }

    [TestMethod]
    public void Invoice_TaxCalculation_UsesCommercialRoundingAwayFromZero()
    {
        // Subtotal = $10.05, TaxRate = 5% => 10.05 * 0.05 = 0.5025 => 0.50.
        // Subtotal = $10.50, TaxRate = 5% => 10.50 * 0.05 = 0.525 => With AwayFromZero: 0.53
        var invoice = Invoice.Create(Guid.NewGuid(), null, "INV-ROUND-2", DateTime.UtcNow, DateTime.UtcNow.AddDays(7), 5.0m);
        invoice.AddItem("Consulting", 1m, 10.50m);

        Assert.AreEqual(10.50m, invoice.SubTotal);
        Assert.AreEqual(0.53m, invoice.TaxAmount);
        Assert.AreEqual(11.03m, invoice.TotalAmount);
    }
}
