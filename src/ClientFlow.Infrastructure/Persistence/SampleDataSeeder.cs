using ClientFlow.Domain.Entities;
using ClientFlow.Domain.Enums;

namespace ClientFlow.Infrastructure.Persistence;

public static class SampleDataSeeder
{
    public static void SeedSampleData(InMemoryDataStore store)
    {
        if (store.Clients.Any()) return;

        // 1. Clients
        var client1 = Client.Create("Apex FinTech UK", "Apex Financial Technologies Ltd", "contracts@apexfintech.co.uk", "+44 20 7946 0912", "1 Canada Square, Canary Wharf, London, E14 5AA", "GBP");
        var client2 = Client.Create("Nordic HealthTech", "Nordic Health Systems AB", "engineering@nordichealth.se", "+46 8 123 4567", "Sveavagen 44, Stockholm, Sweden", "EUR");
        var client3 = Client.Create("Pacific Cloud Sydney", "Pacific Cloud Solutions Pty", "devops@pacificcloud.com.au", "+61 2 9876 5432", "George St, Sydney NSW 2000, Australia", "AUD");

        store.Clients[client1.Id] = client1;
        store.Clients[client2.Id] = client2;
        store.Clients[client3.Id] = client3;

        // 2. Projects
        var proj1 = Project.Create(client1.Id, "Payment Gateway Migration & Reconciliation", "Modernize legacy transaction processor to high-throughput ASP.NET Core API with idempotent webhook reconciliations.", 24000m, 95m, DateTime.UtcNow.AddMonths(-2), DateTime.UtcNow.AddMonths(1));
        proj1.Update(proj1.Name, proj1.Description, proj1.Budget, proj1.HourlyRate, ProjectStatus.Active, proj1.StartDateUtc, proj1.TargetCompletionDateUtc);

        var proj2 = Project.Create(client2.Id, "HIPAA Compliant Patient Telemetry Portal", "Build an Angular 16 single-page application with real-time biometric charts and FHIR API synchronization.", 35000m, 110m, DateTime.UtcNow.AddMonths(-1), DateTime.UtcNow.AddMonths(2));
        proj2.Update(proj2.Name, proj2.Description, proj2.Budget, proj2.HourlyRate, ProjectStatus.Active, proj2.StartDateUtc, proj2.TargetCompletionDateUtc);

        var proj3 = Project.Create(client3.Id, "Multi-Tenant Cloud Tenant Provisioner", "Automated customer workspace creation with isolated schema migrations and Azure KeyVault integration.", 18000m, 85m, DateTime.UtcNow.AddMonths(-3), DateTime.UtcNow.AddMonths(-1));
        proj3.Update(proj3.Name, proj3.Description, proj3.Budget, proj3.HourlyRate, ProjectStatus.Completed, proj3.StartDateUtc, proj3.TargetCompletionDateUtc);

        store.Projects[proj1.Id] = proj1;
        store.Projects[proj2.Id] = proj2;
        store.Projects[proj3.Id] = proj3;

        // 3. Milestones
        var ms1 = Milestone.Create(proj1.Id, "Phase 1: Architecture Spike & Webhook Sandbox", "Complete Idempotency filter and event stream mock.", DateTime.UtcNow.AddMonths(-1), 8000m);
        ms1.MarkCompleted();
        proj1.AddMilestone(ms1);

        var ms2 = Milestone.Create(proj1.Id, "Phase 2: Payment Provider Direct Integration", "Integrate Stripe and BACS direct debits.", DateTime.UtcNow.AddDays(14), 16000m);
        proj1.AddMilestone(ms2);

        // 4. Tasks
        var task1 = ProjectTask.Create(proj1.Id, "Implement Distributed Idempotency Key Middleware", "Ensure webhook payloads cannot be double-processed using Redis/SQL locks.", TaskPriority.Urgent, 16m, "Rashad", DateTime.UtcNow.AddDays(2));
        task1.UpdateStatus(TaskItemStatus.InProgress);

        var task2 = ProjectTask.Create(proj1.Id, "Reconciliation Discrepancy Reporting Service", "Nightly scheduled comparison between bank ledger and local transactions.", TaskPriority.High, 24m, "Rashad", DateTime.UtcNow.AddDays(7));
        task2.UpdateStatus(TaskItemStatus.Todo);

        var task3 = ProjectTask.Create(proj2.Id, "Angular Reactive Forms for Patient Intake", "Implement multi-step reactive validation with accessible error state handling.", TaskPriority.Medium, 18m, "Rashad", DateTime.UtcNow.AddDays(5));
        task3.UpdateStatus(TaskItemStatus.Review);

        var task4 = ProjectTask.Create(proj3.Id, "Zero-Downtime Database Migration Runner", "Execute EF Core idempotent migration scripts during rolling container updates.", TaskPriority.High, 20m, "Rashad", DateTime.UtcNow.AddDays(-10));
        task4.UpdateStatus(TaskItemStatus.Done);

        store.Tasks[task1.Id] = task1;
        store.Tasks[task2.Id] = task2;
        store.Tasks[task3.Id] = task3;
        store.Tasks[task4.Id] = task4;

        // 5. Time Entries
        var time1 = TimeEntry.Create(task1.Id, proj1.Id, "usr_rashad", "Rashad", DateTime.UtcNow.AddDays(-2), 6.5m, true, "Designed and unit-tested IdempotencyKeyAttribute with SHA256 payload hashing.");
        var time2 = TimeEntry.Create(task1.Id, proj1.Id, "usr_rashad", "Rashad", DateTime.UtcNow.AddDays(-1), 7.0m, true, "Integrated SQL Server row-level locking for concurrency conflict prevention.");
        var time3 = TimeEntry.Create(task3.Id, proj2.Id, "usr_rashad", "Rashad", DateTime.UtcNow.AddDays(-3), 8.0m, true, "Built Angular custom validators for medical identifier checksums.");

        store.TimeEntries[time1.Id] = time1;
        store.TimeEntries[time2.Id] = time2;
        store.TimeEntries[time3.Id] = time3;

        // 6. Invoices
        var inv1 = Invoice.Create(client1.Id, proj1.Id, "INV-2026-1001", DateTime.UtcNow.AddDays(-20), DateTime.UtcNow.AddDays(10), 20m, "GBP", "Payment Milestone 1: Architectural Spike & Core Engine Setup.");
        inv1.AddItem("Phase 1 Engineering Delivery", 1, 8000m);
        inv1.MarkSent();

        var inv2 = Invoice.Create(client3.Id, proj3.Id, "INV-2026-0985", DateTime.UtcNow.AddMonths(-1), DateTime.UtcNow.AddDays(-5), 10m, "AUD", "Tenant Migration & Deployment Completion.");
        inv2.AddItem("Cloud Infrastructure Migration Sprint", 1, 18000m);
        inv2.MarkPaid();

        store.Invoices[inv1.Id] = inv1;
        store.Invoices[inv2.Id] = inv2;

        // 7. Audit Log
        store.AuditLogs.Add(AuditLog.Create(nameof(Client), client1.Id.ToString(), "Create", "System", "Apex FinTech UK client account initiated."));
        store.AuditLogs.Add(AuditLog.Create(nameof(Invoice), inv2.Id.ToString(), "PaymentReceived", "Admin", "Received full settlement of AUD 19,800.00 for INV-2026-0985."));
    }
}
