using ClientFlow.Application.DTOs;
using ClientFlow.Domain.Enums;
using ClientFlow.Domain.Interfaces;

namespace ClientFlow.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IClientRepository _clientRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly ITimeEntryRepository _timeEntryRepository;
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IProjectService _projectService;
    private readonly ITaskService _taskService;
    private readonly IInvoiceService _invoiceService;

    public DashboardService(
        IClientRepository clientRepository,
        IProjectRepository projectRepository,
        ITaskRepository taskRepository,
        ITimeEntryRepository timeEntryRepository,
        IInvoiceRepository invoiceRepository,
        IProjectService projectService,
        ITaskService taskService,
        IInvoiceService invoiceService)
    {
        _clientRepository = clientRepository;
        _projectRepository = projectRepository;
        _taskRepository = taskRepository;
        _timeEntryRepository = timeEntryRepository;
        _invoiceRepository = invoiceRepository;
        _projectService = projectService;
        _taskService = taskService;
        _invoiceService = invoiceService;
    }

    public async Task<DashboardKpiDto> GetDashboardKpisAsync(CancellationToken ct = default)
    {
        var clients = await _clientRepository.GetAllAsync(status: ClientStatus.Active, ct: ct);
        var projects = await _projectRepository.GetAllAsync(status: ProjectStatus.Active, ct: ct);

        int openTasksCount = 0;
        var urgentTasks = new List<TaskDto>();

        foreach (var p in projects)
        {
            var tasks = await _taskService.GetTasksByProjectAsync(p.Id, ct: ct);
            openTasksCount += tasks.Count(t => t.Status != TaskItemStatus.Done);
            urgentTasks.AddRange(tasks.Where(t => t.Priority == TaskPriority.Urgent && t.Status != TaskItemStatus.Done));
        }

        var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        decimal totalHoursThisMonth = 0;

        foreach (var p in projects)
        {
            var report = await _timeEntryRepository.GetByProjectIdAsync(p.Id, startOfMonth, DateTime.UtcNow, ct);
            totalHoursThisMonth += report.Sum(e => e.HoursWorked);
        }

        var allInvoices = await _invoiceRepository.GetAllAsync(ct: ct);
        var totalRevenue = allInvoices.Where(i => i.Status == InvoiceStatus.Paid).Sum(i => i.TotalAmount);
        var outstanding = allInvoices.Where(i => i.Status == InvoiceStatus.Sent || i.Status == InvoiceStatus.Overdue).Sum(i => i.TotalAmount);

        var recentProjectsResult = await _projectService.GetProjectsAsync(page: 1, pageSize: 5, ct: ct);
        var recentInvoicesResult = await _invoiceService.GetInvoicesAsync(page: 1, pageSize: 5, ct: ct);

        return new DashboardKpiDto(
            clients.Count,
            projects.Count,
            openTasksCount,
            totalHoursThisMonth,
            totalRevenue,
            outstanding,
            recentProjectsResult.Items,
            urgentTasks.Take(5).ToList(),
            recentInvoicesResult.Items
        );
    }
}
