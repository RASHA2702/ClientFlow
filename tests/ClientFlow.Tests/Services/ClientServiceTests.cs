using ClientFlow.Application.DTOs;
using ClientFlow.Application.Services;
using ClientFlow.Domain.Entities;
using ClientFlow.Domain.Enums;
using ClientFlow.Domain.Exceptions;
using ClientFlow.Domain.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace ClientFlow.Tests.Services;

[TestClass]
public class ClientServiceTests
{
    private Mock<IClientRepository> _clientRepoMock = null!;
    private Mock<IProjectRepository> _projectRepoMock = null!;
    private Mock<IInvoiceRepository> _invoiceRepoMock = null!;
    private Mock<IAuditLogRepository> _auditRepoMock = null!;
    private ClientService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _clientRepoMock = new Mock<IClientRepository>();
        _projectRepoMock = new Mock<IProjectRepository>();
        _invoiceRepoMock = new Mock<IInvoiceRepository>();
        _auditRepoMock = new Mock<IAuditLogRepository>();

        _service = new ClientService(
            _clientRepoMock.Object,
            _projectRepoMock.Object,
            _invoiceRepoMock.Object,
            _auditRepoMock.Object
        );
    }

    [TestMethod]
    public async Task CreateClientAsync_WhenEmailDoesNotExist_CreatesAndAudits()
    {
        // Arrange
        var request = new CreateClientRequest("Global Logistics", "GL Inc", "billing@globallogistics.com", "+123456", "NYC", "USD");
        _clientRepoMock.Setup(r => r.ExistsByEmailAsync(request.Email, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _service.CreateClientAsync(request, "AdminUser");

        // Assert
        Assert.AreEqual("Global Logistics", result.Name);
        Assert.AreEqual("billing@globallogistics.com", result.Email);

        _clientRepoMock.Verify(r => r.AddAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()), Times.Once);
        _auditRepoMock.Verify(r => r.AddAsync(It.Is<AuditLog>(a => a.Action == "Create" && a.PerformedBy == "AdminUser"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    [ExpectedException(typeof(BusinessRuleValidationException))]
    public async Task CreateClientAsync_WhenEmailAlreadyExists_ThrowsValidationException()
    {
        // Arrange
        var request = new CreateClientRequest("Duplicate", "Dup Ltd", "exists@test.com", null, null, "USD");
        _clientRepoMock.Setup(r => r.ExistsByEmailAsync(request.Email, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _service.CreateClientAsync(request, "AdminUser");
    }

    [TestMethod]
    [ExpectedException(typeof(BusinessRuleValidationException))]
    public async Task DeleteClientAsync_WhenClientHasActiveProjects_ThrowsException()
    {
        // Arrange
        var client = Client.Create("Active Corp", "AC", "active@corp.com");
        var activeProject = Project.Create(client.Id, "Migration", null, 10000m, 100m);
        activeProject.Update(activeProject.Name, null, 10000m, 100m, ProjectStatus.Active, null, null);

        _clientRepoMock.Setup(r => r.GetByIdAsync(client.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);
        _projectRepoMock.Setup(r => r.GetAllAsync(client.Id, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Project> { activeProject });

        // Act
        await _service.DeleteClientAsync(client.Id, "AdminUser");
    }
}
