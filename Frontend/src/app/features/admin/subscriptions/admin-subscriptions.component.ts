import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule, DatePipe, CurrencyPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AdminSidebarComponent } from '../components/admin-sidebar/admin-sidebar.component';
import { AdminSubscriptionsService, AdminSubscriptionDto } from '../../../core/services/admin-subscriptions.service';
import { PaginationMeta } from '../../../shared/models/api-response.model';

@Component({
  selector: 'app-admin-subscriptions',
  standalone: true,
  imports: [CommonModule, RouterLink, AdminSidebarComponent, FormsModule],
  providers: [DatePipe, CurrencyPipe],
  templateUrl: './admin-subscriptions.component.html'
})
export class AdminSubscriptionsComponent implements OnInit {
  private subsService = inject(AdminSubscriptionsService);
  protected readonly Math = Math;

  subscriptions = signal<AdminSubscriptionDto[]>([]);
  meta = signal<PaginationMeta | null>(null);
  loading = signal<boolean>(false);

  // Filters
  filterStatus = '';
  currentPage = 1;
  pageSize = 20;

  // Disable State
  selectedSubToDisable = signal<AdminSubscriptionDto | null>(null);
  disableReason = '';

  ngOnInit() {
    this.loadSubscriptions();
  }

  loadSubscriptions() {
    this.loading.set(true);
    this.subsService.getSubscriptions(
      this.currentPage,
      this.pageSize,
      this.filterStatus || undefined
    ).subscribe({
      next: (res) => {
        this.subscriptions.set(res.data?.items || []);
        this.meta.set(res.data?.meta || null);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Failed to load subscriptions', err);
        this.loading.set(false);
      }
    });
  }

  applyFilters() {
    this.currentPage = 1;
    this.loadSubscriptions();
  }

  goToPage(page: number) {
    if (page < 1 || (this.meta() && page > Math.ceil(this.meta()!.total / this.meta()!.pageSize))) return;
    this.currentPage = page;
    this.loadSubscriptions();
  }

  getSubColor(status: string): string {
    switch (status) {
      case 'Active': return 'bg-secondary/20 text-secondary';
      case 'Pending': return 'bg-tertiary/20 text-tertiary';
      case 'Expired': return 'bg-outline-variant/30 text-on-surface-variant';
      case 'Disabled': return 'bg-error/20 text-error';
      default: return 'bg-surface-container-highest text-on-surface';
    }
  }

  activateSubscription(sub: AdminSubscriptionDto) {
    if (confirm(`Approve bank transfer and activate subscription for ${sub.userEmail}?`)) {
      this.subsService.activateSubscription(sub.id).subscribe({
        next: () => {
          this.loadSubscriptions();
        },
        error: (err) => {
          console.error('Failed to activate subscription', err);
          alert('Failed to activate: ' + (err.error?.message || 'Unknown error'));
        }
      });
    }
  }

  promptDisable(sub: AdminSubscriptionDto) {
    this.selectedSubToDisable.set(sub);
    this.disableReason = '';
  }

  cancelDisable() {
    this.selectedSubToDisable.set(null);
  }

  confirmDisable() {
    const sub = this.selectedSubToDisable();
    if (!sub || !this.disableReason.trim()) return;

    this.subsService.disableSubscription(sub.id, this.disableReason).subscribe({
      next: () => {
        this.selectedSubToDisable.set(null);
        this.loadSubscriptions();
      },
      error: (err) => {
        console.error('Failed to disable subscription', err);
        alert('Failed to disable: ' + (err.error?.message || 'Unknown error'));
      }
    });
  }
}
