namespace ClientFlow.Domain.Enums;

public enum ClientStatus
{
    Active = 1,
    Inactive = 2,
    Lead = 3
}

public enum ProjectStatus
{
    Planning = 1,
    Active = 2,
    OnHold = 3,
    Completed = 4,
    Cancelled = 5
}

public enum MilestoneStatus
{
    Pending = 1,
    InProgress = 2,
    Completed = 3,
    Invoiced = 4
}

public enum TaskPriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Urgent = 4
}

public enum TaskItemStatus
{
    Backlog = 1,
    Todo = 2,
    InProgress = 3,
    Review = 4,
    Done = 5
}

public enum InvoiceStatus
{
    Draft = 1,
    Sent = 2,
    Paid = 3,
    Overdue = 4,
    Cancelled = 5
}

public enum UserRole
{
    Admin = 1,
    ProjectManager = 2,
    TeamMember = 3,
    ClientGuest = 4
}
