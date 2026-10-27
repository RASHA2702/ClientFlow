import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  Client,
  Project,
  ProjectTask,
  TimeEntry,
  TimeReportSummary,
  Invoice,
  DashboardKpi,
  PagedResult,
  ClientStatus,
  ProjectStatus,
  TaskItemStatus,
  InvoiceStatus
} from '../models/clientflow.models';

@Injectable({
  providedIn: 'root'
})
export class ClientFlowApiService {
  private readonly baseUrl = 'http://localhost:5000/api';

  constructor(private http: HttpClient) {}

  // Dashboard
  getDashboardKpis(): Observable<DashboardKpi> {
    return this.http.get<DashboardKpi>(`${this.baseUrl}/dashboard/kpis`);
  }

  // Clients
  getClients(search?: string, status?: ClientStatus, page: number = 1, pageSize: number = 10): Observable<PagedResult<Client>> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    if (search) params = params.set('search', search);
    if (status) params = params.set('status', status);
    return this.http.get<PagedResult<Client>>(`${this.baseUrl}/clients`, { params });
  }

  getClientById(id: string): Observable<Client> {
    return this.http.get<Client>(`${this.baseUrl}/clients/${id}`);
  }

  createClient(client: Partial<Client>): Observable<Client> {
    return this.http.post<Client>(`${this.baseUrl}/clients`, client);
  }

  updateClient(id: string, client: Partial<Client>): Observable<Client> {
    return this.http.put<Client>(`${this.baseUrl}/clients/${id}`, client);
  }

  deleteClient(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/clients/${id}`);
  }

  // Projects
  getProjects(clientId?: string, status?: ProjectStatus, page: number = 1, pageSize: number = 10): Observable<PagedResult<Project>> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    if (clientId) params = params.set('clientId', clientId);
    if (status) params = params.set('status', status);
    return this.http.get<PagedResult<Project>>(`${this.baseUrl}/projects`, { params });
  }

  createProject(project: Partial<Project>): Observable<Project> {
    return this.http.post<Project>(`${this.baseUrl}/projects`, project);
  }

  deleteProject(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/projects/${id}`);
  }

  // Tasks
  getTasksByProject(projectId: string, status?: TaskItemStatus): Observable<ProjectTask[]> {
    let params = new HttpParams();
    if (status) params = params.set('status', status);
    return this.http.get<ProjectTask[]>(`${this.baseUrl}/tasks/project/${projectId}`, { params });
  }

  createTask(task: Partial<ProjectTask>): Observable<ProjectTask> {
    return this.http.post<ProjectTask>(`${this.baseUrl}/tasks`, task);
  }

  updateTaskStatus(id: string, status: TaskItemStatus): Observable<ProjectTask> {
    return this.http.patch<ProjectTask>(`${this.baseUrl}/tasks/${id}/status`, { status });
  }

  deleteTask(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/tasks/${id}`);
  }

  // Time Entries
  logTime(entry: Partial<TimeEntry>): Observable<TimeEntry> {
    return this.http.post<TimeEntry>(`${this.baseUrl}/timeentries`, entry);
  }

  getProjectTimeReport(projectId: string): Observable<TimeReportSummary> {
    return this.http.get<TimeReportSummary>(`${this.baseUrl}/timeentries/project/${projectId}`);
  }

  exportTimeCsvUrl(projectId: string): string {
    return `${this.baseUrl}/timeentries/project/${projectId}/export`;
  }

  // Invoices
  getInvoices(clientId?: string, status?: InvoiceStatus, page: number = 1, pageSize: number = 10): Observable<PagedResult<Invoice>> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    if (clientId) params = params.set('clientId', clientId);
    if (status) params = params.set('status', status);
    return this.http.get<PagedResult<Invoice>>(`${this.baseUrl}/invoices`, { params });
  }

  createInvoice(invoice: any): Observable<Invoice> {
    return this.http.post<Invoice>(`${this.baseUrl}/invoices`, invoice);
  }

  markInvoiceSent(id: string): Observable<Invoice> {
    return this.http.post<Invoice>(`${this.baseUrl}/invoices/${id}/send`, {});
  }

  markInvoicePaid(id: string): Observable<Invoice> {
    return this.http.post<Invoice>(`${this.baseUrl}/invoices/${id}/pay`, {});
  }

  exportInvoicesCsvUrl(): string {
    return `${this.baseUrl}/invoices/export`;
  }
}
