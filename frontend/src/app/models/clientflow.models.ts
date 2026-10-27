export type ClientStatus = 'Active' | 'Inactive' | 'Lead';
export type ProjectStatus = 'Planning' | 'Active' | 'OnHold' | 'Completed' | 'Cancelled';
export type MilestoneStatus = 'Pending' | 'InProgress' | 'Completed' | 'Invoiced';
export type TaskPriority = 'Low' | 'Medium' | 'High' | 'Urgent';
export type TaskItemStatus = 'Backlog' | 'Todo' | 'InProgress' | 'Review' | 'Done';
export type InvoiceStatus = 'Draft' | 'Sent' | 'Paid' | 'Overdue' | 'Cancelled';

export interface PagedResult<T> {
  items: T[];
  pageIndex: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface Client {
  id: string;
  name: string;
  companyName: string;
  email: string;
  phone?: string;
  billingAddress?: string;
  currency: string;
  status: ClientStatus;
  createdAtUtc: string;
  activeProjectsCount: number;
  totalBilled: number;
}

export interface Project {
  id: string;
  clientId: string;
  clientName: string;
  name: string;
  description?: string;
  budget: number;
  hourlyRate: number;
  status: ProjectStatus;
  startDateUtc?: string;
  targetCompletionDateUtc?: string;
  createdAtUtc: string;
  totalHoursLogged: number;
  totalCostEstimated: number;
  taskCount: number;
  completedTaskCount: number;
}

export interface Milestone {
  id: string;
  projectId: string;
  title: string;
  description?: string;
  targetDateUtc?: string;
  amount: number;
  status: MilestoneStatus;
  completedAtUtc?: string;
}

export interface ProjectTask {
  id: string;
  projectId: string;
  title: string;
  description?: string;
  priority: TaskPriority;
  status: TaskItemStatus;
  estimatedHours: number;
  loggedHours: number;
  assignedTo?: string;
  dueDateUtc?: string;
  createdAtUtc: string;
}

export interface TimeEntry {
  id: string;
  projectTaskId: string;
  taskTitle: string;
  projectId: string;
  projectName: string;
  userId: string;
  userName: string;
  dateUtc: string;
  hoursWorked: number;
  isBillable: boolean;
  notes?: string;
  isInvoiced: boolean;
  invoiceId?: string;
  createdAtUtc: string;
}

export interface TimeReportSummary {
  totalHours: number;
  billableHours: number;
  nonBillableHours: number;
  totalBillableAmount: number;
  entries: TimeEntry[];
}

export interface InvoiceItem {
  id: string;
  description: string;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
}

export interface Invoice {
  id: string;
  clientId: string;
  clientName: string;
  projectId?: string;
  projectName?: string;
  invoiceNumber: string;
  issueDateUtc: string;
  dueDateUtc: string;
  subTotal: number;
  taxRatePercentage: number;
  taxAmount: number;
  totalAmount: number;
  status: InvoiceStatus;
  currency: string;
  notes?: string;
  createdAtUtc: string;
  paidAtUtc?: string;
  items: InvoiceItem[];
}

export interface DashboardKpi {
  totalActiveClients: number;
  totalActiveProjects: number;
  openTasksCount: number;
  totalHoursLoggedThisMonth: number;
  totalRevenueCollected: number;
  totalOutstandingInvoicesAmount: number;
  recentProjects: Project[];
  urgentTasks: ProjectTask[];
  recentInvoices: Invoice[];
}
