using ClientFlow.Domain.Entities;
using ClientFlow.Domain.Enums;
using ClientFlow.Domain.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClientFlow.Tests.Domain;

[TestClass]
public class ProjectAndMilestoneTests
{
    [TestMethod]
    public void CreateProject_ValidInput_InitializesCorrectly()
    {
        var clientId = Guid.NewGuid();
        var project = Project.Create(clientId, "Migration", "Core modernization", 25000m, 100m);

        Assert.AreNotEqual(Guid.Empty, project.Id);
        Assert.AreEqual(clientId, project.ClientId);
        Assert.AreEqual("Migration", project.Name);
        Assert.AreEqual(25000m, project.Budget);
        Assert.AreEqual(100m, project.HourlyRate);
        Assert.AreEqual(ProjectStatus.Planning, project.Status);
    }

    [TestMethod]
    [ExpectedException(typeof(BusinessRuleValidationException))]
    public void CreateProject_NegativeBudget_ThrowsException()
    {
        Project.Create(Guid.NewGuid(), "Migration", "Desc", -100m, 100m);
    }

    [TestMethod]
    [ExpectedException(typeof(BusinessRuleValidationException))]
    public void CreateProject_NegativeHourlyRate_ThrowsException()
    {
        Project.Create(Guid.NewGuid(), "Migration", "Desc", 1000m, -50m);
    }

    [TestMethod]
    public void AddMilestone_AddsToCollectionAndCanComplete()
    {
        var project = Project.Create(Guid.NewGuid(), "Portal", null, 10000m, 80m);
        var milestone = Milestone.Create(project.Id, "MVP Release", "First deployment", DateTime.UtcNow.AddDays(30), 5000m);

        project.AddMilestone(milestone);
        Assert.AreEqual(1, project.Milestones.Count);
        Assert.AreEqual(MilestoneStatus.Pending, milestone.Status);

        milestone.MarkCompleted();
        Assert.AreEqual(MilestoneStatus.Completed, milestone.Status);
        Assert.IsNotNull(milestone.CompletedAtUtc);
    }
}
