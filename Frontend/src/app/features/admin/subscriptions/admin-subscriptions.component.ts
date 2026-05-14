import { Component, inject, signal, OnInit, OnDestroy, ChangeDetectionStrategy, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { AdminSidebarComponent } from '../components/admin-sidebar/admin-sidebar.component';
import { AdminSubscriptionsService, AdminSubscriptionDto } from '../../../core/services/admin-subscriptions.service';
import { AdminUsersService, AdminUserDto, SubscriptionPlan } from '../../../core/services/admin-users.service';
import { PaginationMeta } from '../../../shared/models/api-response.model';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-admin-subscriptions',
  standalone: true,
  imports: [CommonModule, RouterLink, AdminSidebarComponent, FormsModule],
  providers: [DatePipe],
  templateUrl: './admin-subscriptions.component.html'
})
export class AdminSubscriptionsComponent implements OnInit, OnDestroy {
  private subsService = inject(AdminSubscriptionsService);
  private usersService = inject(AdminUsersService);
  private destroyRef = inject(DestroyRef);
  protected readonly Math = Math;

  subscriptions = signal<AdminSubscriptionDto[]>([]);
  meta = signal<PaginationMeta | null>(null);
  loading = signal<boolean>(false);

  // Filters
  filterStatus = '';
  currentPage = 1;
  pageSize = 20;

  // ── Disable State ──
  selectedSubToDisable = signal<AdminSubscriptionDto | null>(null);
  disableReason = '';

  // ── Reject State ──
  selectedSubToReject = signal<AdminSubscriptionDto | null>(null);
  rejectReason = '';
  rejectLoading = signal(false);

  // ── Grant Subscription State ──
  showGrantModal = signal(false);
  grantLoading = signal(false);
  grantError = signal('');

  // User search for grant
  userSearchQuery = '';
  userSearchResults = signal<AdminUserDto[]>([]);
  userSearchLoading = signal(false);
  userConfirmed = signal(false);           // true after user clicks Confirm
  selectedUserForGrant = signal<AdminUserDto | null>(null);

  private searchSubject = new Subject<string>();
  private searchSub = this.searchSubject.pipe(
    debounceTime(300),
    distinctUntilChanged(),
    takeUntilDestroyed(this.destroyRef)
  ).subscribe(query => {
    if (query.trim().length >= 2) {
      this.performUserSearch(query);
    } else {
      this.userSearchResults.set([]);
    }
  });

  // Grant form fields
  grantPlan: SubscriptionPlan = 'Monthly';
  grantExpiryDate = '';
  grantNotes = '';

  readonly monthlyPresets = [30, 60, 90, 120, 150];
  readonly annualPresets  = [365, 730, 1095, 1460, 1825];  // 1–5 yr

  get activePresets(): number[] {
    return this.grantPlan === 'Annual' ? this.annualPresets : this.monthlyPresets;
  }

  // null = no limit (auto-renew)
  selectedPresetDays = signal<number | null>(null);

  ngOnInit() {
    this.loadSubscriptions();
  }

  ngOnDestroy() {
    this.searchSub.unsubscribe();
    this.searchSubject.complete();
  }

  loadSubscriptions() {
    this.loading.set(true);
    this.subsService.getSubscriptions(
      this.currentPage,
      this.pageSize,
      this.filterStatus || undefined
    ).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
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

  getPlanLabel(plan?: string): string {
    switch (plan) {
      case 'Monthly': return '📅 Monthly';
      case 'Annual':  return '🌟 Annual';
      default:        return '🆓 Free';
    }
  }

  getPlanColor(plan?: string): string {
    switch (plan) {
      case 'Monthly': return 'bg-secondary/20 text-secondary';
      case 'Annual':  return 'bg-tertiary/20 text-tertiary';
      default:        return 'bg-surface-container text-on-surface-variant';
    }
  }

  // ── Activate ──
  activateSubscription(sub: AdminSubscriptionDto) {
    if (confirm(`Approve bank transfer and activate subscription for ${sub.userEmail}?`)) {
      this.subsService.activateSubscription(sub.id).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
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

  // ── Disable ──
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

    this.subsService.disableSubscription(sub.id, this.disableReason).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
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

  // ── Reject ──
  promptReject(sub: AdminSubscriptionDto) {
    this.selectedSubToReject.set(sub);
    this.rejectReason = '';
  }

  cancelReject() {
    this.selectedSubToReject.set(null);
    this.rejectReason = '';
  }

  confirmReject() {
    const sub = this.selectedSubToReject();
    if (!sub || !this.rejectReason.trim()) return;

    this.rejectLoading.set(true);
    this.subsService.rejectSubscription(sub.id, this.rejectReason).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        this.rejectLoading.set(false);
        this.selectedSubToReject.set(null);
        this.loadSubscriptions();
      },
      error: (err) => {
        this.rejectLoading.set(false);
        console.error('Failed to reject subscription', err);
        alert('Failed to reject: ' + (err.error?.message || 'Unknown error'));
      }
    });
  }

  // ── Grant Modal ──
  openGrantModal() {
    this.showGrantModal.set(true);
    this.grantError.set('');
    this.userSearchQuery = '';
    this.userSearchResults.set([]);
    this.selectedUserForGrant.set(null);
    this.userConfirmed.set(false);
    this.grantPlan = 'Monthly';
    this.grantNotes = '';
    this.grantExpiryDate = '';           // empty = auto-renew
    this.selectedPresetDays.set(null);
  }

  closeGrantModal() {
    this.showGrantModal.set(false);
    this.grantError.set('');
    this.selectedUserForGrant.set(null);
    this.userConfirmed.set(false);
    this.userSearchResults.set([]);
    this.selectedPresetDays.set(null);
  }

  applyPresetDays(days: number) {
    if (this.selectedPresetDays() === days) {
      // Toggle off → back to auto-renew
      this.selectedPresetDays.set(null);
      this.grantExpiryDate = '';
      return;
    }
    this.selectedPresetDays.set(days);
    const d = new Date();
    d.setDate(d.getDate() + days);
    this.grantExpiryDate = d.toISOString().split('T')[0];
  }

  onPlanChange(plan: SubscriptionPlan) {
    this.grantPlan = plan;
    // Reset duration when switching plan
    this.selectedPresetDays.set(null);
    this.grantExpiryDate = '';
  }

  // Called on every keystroke in the search input
  onSearchInput() {
    // Clear confirmed selection when user starts typing again
    if (this.userConfirmed()) {
      this.userConfirmed.set(false);
      this.selectedUserForGrant.set(null);
    }
    this.searchSubject.next(this.userSearchQuery);
  }

  // Internal: perform the actual API call
  performUserSearch(query: string) {
    this.userSearchLoading.set(true);
    this.usersService.getUsers(1, 10, query).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (res) => {
        this.userSearchResults.set((res.data?.items || []).filter(u => u.role !== 'Admin'));
        this.userSearchLoading.set(false);
      },
      error: () => {
        this.userSearchLoading.set(false);
      }
    });
  }

  // Called when user clicks a row in the dropdown
  selectUserForGrant(user: AdminUserDto) {
    this.selectedUserForGrant.set(user);
    this.userConfirmed.set(false);   // pending confirmation
    this.userSearchResults.set([]);
    this.userSearchQuery = user.email;
  }

  // Called when user clicks the Confirm button
  confirmUserSelection() {
    if (this.selectedUserForGrant()) {
      this.userConfirmed.set(true);
    }
  }

  // Old method kept for compatibility, delegates to performUserSearch
  searchUsers() {
    this.performUserSearch(this.userSearchQuery);
  }

  submitGrant() {
    const user = this.selectedUserForGrant();
    if (!user) {
      this.grantError.set('Please search and select a user first.');
      return;
    }
    if (!this.userConfirmed()) {
      this.grantError.set('Please confirm the selected user before granting.');
      return;
    }
    this.grantLoading.set(true);
    this.grantError.set('');
    // null = no expiry (auto-renew) — backend accepts DateTime? nullable
    const expiryIso: string | null = this.grantExpiryDate
      ? new Date(this.grantExpiryDate + 'T23:59:59').toISOString()
      : null;
    this.subsService.grantSubscription(user.id, this.grantPlan, expiryIso, this.grantNotes || undefined).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        this.grantLoading.set(false);
        this.closeGrantModal();
        this.loadSubscriptions();
      },
      error: (err) => {
        this.grantLoading.set(false);
        this.grantError.set(err.error?.message || err.error?.errors?.[0]?.message || 'Failed to grant subscription.');
      }
    });
  }
}
