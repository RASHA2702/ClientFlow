using ClientFlow.Domain.Entities;
using ClientFlow.Domain.Enums;
using ClientFlow.Domain.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClientFlow.Tests.Domain;

[TestClass]
public class ClientDomainTests
{
    [TestMethod]
    public void Create_WithValidParameters_InitializesClientCorrectly()
    {
        // Arrange & Act
        var client = Client.Create("Apex FinTech", "Apex Ltd", "dev@apex.com", "+44123456", "London", "GBP");

        // Assert
        Assert.AreNotEqual(Guid.Empty, client.Id);
        Assert.AreEqual("Apex FinTech", client.Name);
        Assert.AreEqual("Apex Ltd", client.CompanyName);
        Assert.AreEqual("dev@apex.com", client.Email);
        Assert.AreEqual("GBP", client.Currency);
        Assert.AreEqual(ClientStatus.Active, client.Status);
        Assert.IsTrue(client.CreatedAtUtc <= DateTime.UtcNow);
    }

    [TestMethod]
    [ExpectedException(typeof(BusinessRuleValidationException))]
    public void Create_WithEmptyName_ThrowsBusinessRuleValidationException()
    {
        Client.Create("", "Company", "test@test.com");
    }

    [TestMethod]
    [ExpectedException(typeof(BusinessRuleValidationException))]
    public void Create_WithInvalidEmail_ThrowsBusinessRuleValidationException()
    {
        Client.Create("Valid Name", "Company", "invalid-email-address");
    }

    [TestMethod]
    public void UpdateDetails_UpdatesPropertiesAndSetsUpdatedAtUtc()
    {
        // Arrange
        var client = Client.Create("Initial", "Initial Ltd", "init@test.com");

        // Act
        client.UpdateDetails("Updated Name", "Updated Ltd", "updated@test.com", "+123", "New Address", "EUR");

        // Assert
        Assert.AreEqual("Updated Name", client.Name);
        Assert.AreEqual("updated@test.com", client.Email);
        Assert.AreEqual("EUR", client.Currency);
        Assert.IsNotNull(client.UpdatedAtUtc);
    }

    [TestMethod]
    public void SetStatus_ChangesStatus()
    {
        // Arrange
        var client = Client.Create("Client", "Company", "c@test.com");

        // Act
        client.SetStatus(ClientStatus.Inactive);

        // Assert
        Assert.AreEqual(ClientStatus.Inactive, client.Status);
    }
}
