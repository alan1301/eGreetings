import { Component, inject, signal, OnInit, ChangeDetectionStrategy, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AdminSidebarComponent } from '../components/admin-sidebar/admin-sidebar.component';
import { AdminPaymentsService, PaymentTransactionAdminDto } from '../../../core/services/admin-payments.service';
import { PaginationMeta } from '../../../shared/models/api-response.model';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-admin-payments',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, AdminSidebarComponent],
  templateUrl: './admin-payments.component.html',
})
export class AdminPaymentsComponent implements OnInit {
  private service = inject(AdminPaymentsService);
  private destroyRef = inject(DestroyRef);
  protected readonly Math = Math;

  payments = signal<PaymentTransactionAdminDto[]>([]);
  meta = signal<PaginationMeta | null>(null);
  loading = signal(false);

  filterStatus = '';
  searchQuery = '';
  currentPage = 1;
  readonly pageSize = 20;

  ngOnInit() { this.load(); }

  load() {
    this.loading.set(true);
    this.service.getPayments(
      this.currentPage, this.pageSize,
      this.filterStatus || undefined,
      this.searchQuery.trim() || undefined
    ).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: r => {
        this.payments.set(r.data.items);
        this.meta.set(r.data.meta ?? r.meta ?? null);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  applyFilters() {
    this.currentPage = 1;
    this.load();
  }

  goToPage(page: number) {
    this.currentPage = page;
    this.load();
  }

  statusClass(status: string): string {
    switch (status) {
      case 'Paid':    return 'bg-green-100 text-green-700';
      case 'Pending': return 'bg-yellow-100 text-yellow-700';
      case 'Failed':  return 'bg-red-100 text-red-700';
      default:        return 'bg-surface-container text-on-surface-variant';
    }
  }

  planClass(plan: string): string {
    switch (plan) {
      case 'Annual':  return 'bg-primary/10 text-primary';
      case 'Monthly': return 'bg-tertiary/10 text-tertiary';
      default:        return 'bg-surface-container text-on-surface-variant';
    }
  }

  formatPaymentMethod(method: string): string {
    switch (method) {
      case 'CardPayment': return 'Card Payment';
      case 'AdminGrant':  return 'Admin Gift';
      default:            return method;
    }
  }

  formatAmount(amount: number, _currency: string): string {
    return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(amount);
  }

  formatDate(d: string | null): string {
    if (!d) return '—';
    return new Date(d).toLocaleDateString('en-US', {
      month: 'short', day: 'numeric', year: 'numeric',
      hour: '2-digit', minute: '2-digit',
    });
  }

  get totalPages(): number {
    return this.meta() ? Math.ceil(this.meta()!.total / this.pageSize) : 1;
  }
}
