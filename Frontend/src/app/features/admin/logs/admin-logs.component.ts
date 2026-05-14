import { Component, inject, signal, OnInit, computed, ChangeDetectionStrategy, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule, DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AdminSidebarComponent } from '../components/admin-sidebar/admin-sidebar.component';
import { AdminLogsService, SystemLogDto } from '../../../core/services/admin-logs.service';
import { PaginationMeta } from '../../../shared/models/api-response.model';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-admin-logs',
  standalone: true,
  imports: [CommonModule, RouterLink, AdminSidebarComponent, FormsModule],
  providers: [DatePipe],
  templateUrl: './admin-logs.component.html'
})
export class AdminLogsComponent implements OnInit {
  private logsService = inject(AdminLogsService);
  private destroyRef = inject(DestroyRef);
  protected readonly Math = Math;

  logs = signal<SystemLogDto[]>([]);
  meta = signal<PaginationMeta | null>(null);
  loading = signal<boolean>(false);

  // Filters
  filterEventType = '';
  filterStatus = '';
  filterFrom = '';
  filterTo = '';
  currentPage = 1;
  pageSize = 50;

  ngOnInit() {
    this.loadLogs();
  }

  loadLogs() {
    this.loading.set(true);
    this.logsService.getLogs(
      this.currentPage,
      this.pageSize,
      this.filterEventType || undefined,
      this.filterStatus || undefined,
      this.filterFrom || undefined,
      this.filterTo || undefined
    ).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (res) => {
        this.logs.set(res.data?.items || []);
        this.meta.set(res.data?.meta || null);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Failed to load logs', err);
        this.loading.set(false);
      }
    });
  }

  applyFilters() {
    this.currentPage = 1;
    this.loadLogs();
  }

  goToPage(page: number) {
    if (page < 1 || (this.meta() && page > Math.ceil(this.meta()!.total / this.meta()!.pageSize))) return;
    this.currentPage = page;
    this.loadLogs();
  }

  getEventColor(type: string): string {
    switch (type) {
      case 'Login': return 'bg-tertiary/20 text-tertiary';
      case 'Logout': return 'bg-outline-variant/30 text-on-surface-variant';
      case 'Payment': return 'bg-primary/20 text-primary';
      case 'SendCard': return 'bg-secondary/20 text-secondary';
      case 'AdminAction': return 'bg-[#8b5cf6]/20 text-[#8b5cf6]'; // custom purple
      case 'SystemError': return 'bg-error/20 text-error';
      case 'JobRun': return 'bg-[#0ea5e9]/20 text-[#0ea5e9]'; // custom cyan
      default: return 'bg-surface-container-highest text-on-surface';
    }
  }
}
