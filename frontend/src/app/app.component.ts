import { Component, OnInit } from '@angular/core';
import { ClientFlowApiService } from './services/clientflow-api.service';
import {
  Client,
  Project,
  ProjectTask,
  Invoice,
  DashboardKpi,
  TimeReportSummary,
  TaskItemStatus,
  TaskPriority,
  ClientStatus,
  ProjectStatus,
  InvoiceStatus
} from './models/clientflow.models';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  title = 'ClientFlow Enterprise Platform';
  activeTab: 'dashboard' | 'clients' | 'projects' | 'tasks' | 'time' | 'invoices' = 'dashboard';

  // Loading & Alerts
  loading = false;
  toastMessage: string | null = null;
  toastType: 'success' | 'error' | 'info' = 'info';

  // Dashboard Data
  kpis: DashboardKpi | null = null;

  // Clients State
  clients: Client[] = [];
  clientSearch = '';
  clientStatusFilter: ClientStatus | '' = '';
  showClientModal = false;
  newClient = {
    name: '',
    companyName: '',
    email: '',
    phone: '',
    billingAddress: '',
    currency: 'USD'
  };

  // Projects State
  projects: Project[] = [];
  projectStatusFilter: ProjectStatus | '' = '';
  showProjectModal = false;
  newProject = {
    clientId: '',
    name: '',
    description: '',
    budget: 15000,
    hourlyRate: 95,
    startDateUtc: new Date().toISOString().split('T')[0],
    targetCompletionDateUtc: ''
  };

  // Tasks State
  selectedProjectId: string = '';
  projectTasks: ProjectTask[] = [];
  showTaskModal = false;
  newTask = {
    title: '',
    description: '',
    priority: 'Medium' as TaskPriority,
    estimatedHours: 8,
    assignedTo: 'Rashad'
  };

  // Time Tracking State
  timeReport: TimeReportSummary | null = null;
  timeTaskId = '';
  newTimeEntry = {
    hoursWorked: 4,
    isBillable: true,
    notes: ''
  };

  // Invoices State
  invoices: Invoice[] = [];
  showInvoiceModal = false;
  newInvoice = {
    clientId: '',
    projectId: '',
    taxRatePercentage: 20,
    currency: 'USD',
    notes: 'Net 30 terms. Thank you for your business.',
    items: [
      { description: 'Sprint 1 Development & Code Review', quantity: 40, unitPrice: 95 }
    ]
  };

  constructor(public api: ClientFlowApiService) {}

  ngOnInit(): void {
    this.loadDashboard();
    this.loadClients();
    this.loadProjects();
    this.loadInvoices();
  }

  showToast(message: string, type: 'success' | 'error' | 'info' = 'success'): void {
    this.toastMessage = message;
    this.toastType = type;
    setTimeout(() => {
      this.toastMessage = null;
    }, 4000);
  }

  // Dashboard
  loadDashboard(): void {
    this.loading = true;
    this.api.getDashboardKpis().subscribe({
      next: (data) => {
        this.kpis = data;
        this.loading = false;
      },
      error: (err) => {
        this.showToast('Failed to load dashboard metrics. Ensure backend API is running.', 'error');
        this.loading = false;
      }
    });
  }

  // Clients
  loadClients(): void {
    const status = this.clientStatusFilter ? (this.clientStatusFilter as ClientStatus) : undefined;
    this.api.getClients(this.clientSearch, status).subscribe({
      next: (res) => {
        this.clients = res.items;
      },
      error: () => this.showToast('Error loading clients', 'error')
    });
  }

  saveClient(): void {
    if (!this.newClient.name || !this.newClient.email) {
      this.showToast('Please provide both Client Name and valid Email', 'error');
      return;
    }

    this.api.createClient(this.newClient).subscribe({
      next: (created) => {
        this.showToast(`Client '${created.name}' created successfully.`);
        this.showClientModal = false;
        this.newClient = { name: '', companyName: '', email: '', phone: '', billingAddress: '', currency: 'USD' };
        this.loadClients();
        this.loadDashboard();
      },
      error: (err) => this.showToast(err.error?.detail || 'Failed to save client', 'error')
    });
  }

  deleteClient(client: Client): void {
    if (confirm(`Are you sure you want to delete client '${client.name}'?`)) {
      this.api.deleteClient(client.id).subscribe({
        next: () => {
          this.showToast(`Client '${client.name}' removed.`);
          this.loadClients();
          this.loadDashboard();
        },
        error: (err) => this.showToast(err.error?.detail || 'Failed to delete client', 'error')
      });
    }
  }

  // Projects
  loadProjects(): void {
    const status = this.projectStatusFilter ? (this.projectStatusFilter as ProjectStatus) : undefined;
    this.api.getProjects(undefined, status).subscribe({
      next: (res) => {
        this.projects = res.items;
        if (!this.selectedProjectId && this.projects.length > 0) {
          this.selectProjectForTasks(this.projects[0].id);
        }
      },
      error: () => this.showToast('Error loading projects', 'error')
    });
  }

  saveProject(): void {
    if (!this.newProject.clientId || !this.newProject.name) {
      this.showToast('Please select a Client and enter a Project Name', 'error');
      return;
    }

    this.api.createProject(this.newProject).subscribe({
      next: (created) => {
        this.showToast(`Project '${created.name}' created successfully.`);
        this.showProjectModal = false;
        this.loadProjects();
        this.loadDashboard();
      },
      error: (err) => this.showToast(err.error?.detail || 'Failed to create project', 'error')
    });
  }

  // Tasks & Kanban
  selectProjectForTasks(projectId: string): void {
    this.selectedProjectId = projectId;
    this.loadProjectTasks();
    this.loadTimeReport();
  }

  loadProjectTasks(): void {
    if (!this.selectedProjectId) return;
    this.api.getTasksByProject(this.selectedProjectId).subscribe({
      next: (tasks) => {
        this.projectTasks = tasks;
        if (tasks.length > 0 && !this.timeTaskId) {
          this.timeTaskId = tasks[0].id;
        }
      },
      error: () => this.showToast('Error loading tasks', 'error')
    });
  }

  getTasksByColumn(status: TaskItemStatus): ProjectTask[] {
    return this.projectTasks.filter((t) => t.status === status);
  }

  updateTaskStatus(task: ProjectTask, newStatus: TaskItemStatus): void {
    this.api.updateTaskStatus(task.id, newStatus).subscribe({
      next: () => {
        task.status = newStatus;
        this.showToast(`Task '${task.title}' moved to ${newStatus}.`);
        this.loadDashboard();
      },
      error: (err) => this.showToast(err.error?.detail || 'Failed to update task status', 'error')
    });
  }

  saveTask(): void {
    if (!this.newTask.title) {
      this.showToast('Task title is required', 'error');
      return;
    }

    const payload = {
      projectId: this.selectedProjectId,
      ...this.newTask
    };

    this.api.createTask(payload).subscribe({
      next: (created) => {
        this.showToast(`Task '${created.title}' added.`);
        this.showTaskModal = false;
        this.newTask = { title: '', description: '', priority: 'Medium', estimatedHours: 8, assignedTo: 'Rashad' };
        this.loadProjectTasks();
        this.loadDashboard();
      },
      error: (err) => this.showToast(err.error?.detail || 'Failed to add task', 'error')
    });
  }

  // Time Tracker
  loadTimeReport(): void {
    if (!this.selectedProjectId) return;
    this.api.getProjectTimeReport(this.selectedProjectId).subscribe({
      next: (report) => {
        this.timeReport = report;
      },
      error: () => this.showToast('Error loading timesheet report', 'error')
    });
  }

  logTime(): void {
    if (!this.timeTaskId || !this.selectedProjectId) {
      this.showToast('Please select a project and a task', 'error');
      return;
    }

    const payload = {
      projectTaskId: this.timeTaskId,
      projectId: this.selectedProjectId,
      userId: 'usr_rashad',
      userName: 'Rashad',
      dateUtc: new Date().toISOString(),
      hoursWorked: this.newTimeEntry.hoursWorked,
      isBillable: this.newTimeEntry.isBillable,
      notes: this.newTimeEntry.notes
    };

    this.api.logTime(payload).subscribe({
      next: () => {
        this.showToast(`Logged ${payload.hoursWorked} hours successfully.`);
        this.newTimeEntry.notes = '';
        this.loadTimeReport();
        this.loadProjectTasks();
        this.loadDashboard();
      },
      error: (err) => this.showToast(err.error?.detail || 'Failed to log time', 'error')
    });
  }

  // Invoices
  loadInvoices(): void {
    this.api.getInvoices().subscribe({
      next: (res) => {
        this.invoices = res.items;
      },
      error: () => this.showToast('Error loading invoices', 'error')
    });
  }

  addInvoiceItem(): void {
    this.newInvoice.items.push({ description: 'Additional Engineering Deliverable', quantity: 10, unitPrice: 95 });
  }

  removeInvoiceItem(index: number): void {
    if (this.newInvoice.items.length > 1) {
      this.newInvoice.items.splice(index, 1);
    }
  }

  saveInvoice(): void {
    if (!this.newInvoice.clientId) {
      this.showToast('Please select a Client for the invoice', 'error');
      return;
    }

    const payload = {
      clientId: this.newInvoice.clientId,
      projectId: this.newInvoice.projectId || null,
      issueDateUtc: new Date().toISOString(),
      dueDateUtc: new Date(Date.now() + 30 * 24 * 60 * 60 * 1000).toISOString(),
      taxRatePercentage: this.newInvoice.taxRatePercentage,
      currency: this.newInvoice.currency,
      notes: this.newInvoice.notes,
      items: this.newInvoice.items
    };

    this.api.createInvoice(payload).subscribe({
      next: (created) => {
        this.showToast(`Invoice '${created.invoiceNumber}' created (${created.totalAmount} ${created.currency}).`);
        this.showInvoiceModal = false;
        this.loadInvoices();
        this.loadDashboard();
      },
      error: (err) => this.showToast(err.error?.detail || 'Failed to create invoice', 'error')
    });
  }

  markInvoiceSent(inv: Invoice): void {
    this.api.markInvoiceSent(inv.id).subscribe({
      next: (updated) => {
        inv.status = updated.status;
        this.showToast(`Invoice '${inv.invoiceNumber}' marked as Sent.`);
        this.loadDashboard();
      },
      error: (err) => this.showToast(err.error?.detail || 'Failed to update invoice status', 'error')
    });
  }

  markInvoicePaid(inv: Invoice): void {
    this.api.markInvoicePaid(inv.id).subscribe({
      next: (updated) => {
        inv.status = updated.status;
        inv.paidAtUtc = updated.paidAtUtc;
        this.showToast(`Invoice '${inv.invoiceNumber}' marked as Paid! Revenue updated.`);
        this.loadDashboard();
      },
      error: (err) => this.showToast(err.error?.detail || 'Failed to update invoice status', 'error')
    });
  }
}
