import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AdminSidebarComponent } from '../components/admin-sidebar/admin-sidebar.component';
import { AdminUsersService, AdminUserDto, SubscriptionPlan } from '../../../core/services/admin-users.service';
import { PaginationMeta } from '../../../shared/models/api-response.model';

@Component({
  selector: 'app-admin-users',
  standalone: true,
  imports: [CommonModule, RouterLink, AdminSidebarComponent, FormsModule],
  providers: [DatePipe],
  templateUrl: './admin-users.component.html'
})
export class AdminUsersComponent implements OnInit {
  private usersService = inject(AdminUsersService);
  protected readonly Math = Math;

  users = signal<AdminUserDto[]>([]);
  meta = signal<PaginationMeta | null>(null);
  loading = signal<boolean>(false);

  // Filters
  filterSearch = '';
  filterSubStatus = '';
  currentPage = 1;
  pageSize = 20;

  // Lock State
  selectedUserToLock = signal<AdminUserDto | null>(null);
  lockReason = '';

  // Subscription Management State
  selectedUserForSub = signal<AdminUserDto | null>(null);
  subDisableReason = '';
  subActionLoading = signal(false);
  subActionError = signal('');

  // Grant Subscription Form
  grantPlan: SubscriptionPlan = 'Monthly';
  grantExpiryDate = '';   // ISO date string yyyy-MM-dd
  grantNotes = '';
  showGrantForm = signal(false);

  // Preset day options
  readonly presetDays = [30, 60, 90, 180, 365];

  // Min expiry date for the date picker (tomorrow)
  get minExpiryDate(): string {
    const d = new Date();
    d.setDate(d.getDate() + 1);
    return d.toISOString().split('T')[0];
  }

  applyPresetDays(days: number) {
    const d = new Date();
    d.setDate(d.getDate() + days);
    this.grantExpiryDate = d.toISOString().split('T')[0];
  }

  getPlanLabel(plan?: string): string {
    switch (plan) {
      case 'Monthly': return '📅 Monthly';
      case 'Annual':  return '🌟 Annual';
      default:        return '🆓 Free Tier';
    }
  }

  getPlanColor(plan?: string): string {
    switch (plan) {
      case 'Monthly': return 'bg-secondary/20 text-secondary';
      case 'Annual':  return 'bg-tertiary/20 text-tertiary';
      default:        return 'bg-surface-container text-on-surface-variant';
    }
  }

  ngOnInit() {
    this.loadUsers();
  }

  loadUsers() {
    this.loading.set(true);
    this.usersService.getUsers(
      this.currentPage,
      this.pageSize,
      this.filterSearch || undefined,
      this.filterSubStatus || undefined
    ).subscribe({
      next: (res) => {
        this.users.set(res.data?.items || []);
        this.meta.set(res.data?.meta || null);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Failed to load users', err);
        this.loading.set(false);
      }
    });
  }

  applyFilters() {
    this.currentPage = 1;
    this.loadUsers();
  }

  goToPage(page: number) {
    if (page < 1 || (this.meta() && page > Math.ceil(this.meta()!.total / this.meta()!.pageSize))) return;
    this.currentPage = page;
    this.loadUsers();
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

  promptLock(user: AdminUserDto) {
    this.selectedUserToLock.set(user);
    this.lockReason = '';
  }

  cancelLock() {
    this.selectedUserToLock.set(null);
  }

  confirmLock() {
    const user = this.selectedUserToLock();
    if (!user) return;

    this.usersService.lockUser(user.id, this.lockReason).subscribe({
      next: () => {
        this.selectedUserToLock.set(null);
        this.loadUsers(); // Refresh
      },
      error: (err) => {
        console.error('Failed to lock user', err);
        alert('Failed to lock user: ' + (err.error?.message || 'Unknown error'));
      }
    });
  }

  unlockUser(user: AdminUserDto) {
    if (confirm(`Are you sure you want to unlock ${user.email}?`)) {
      this.usersService.unlockUser(user.id).subscribe({
        next: () => {
          this.loadUsers();
        },
        error: (err) => {
          console.error('Failed to unlock user', err);
          alert('Failed to unlock user: ' + (err.error?.message || 'Unknown error'));
        }
      });
    }
  }

  // ── Subscription management ──
  openSubModal(user: AdminUserDto) {
    this.selectedUserForSub.set(user);
    this.subDisableReason = '';
    this.subActionError.set('');
    this.showGrantForm.set(false);
    this.grantPlan = 'Monthly';
    this.grantExpiryDate = '';
    this.grantNotes = '';
    // Default: 30 days from now
    this.applyPresetDays(30);
  }

  closeSubModal() {
    this.selectedUserForSub.set(null);
    this.subActionError.set('');
    this.showGrantForm.set(false);
  }

  activateUserSub() {
    const user = this.selectedUserForSub();
    if (!user?.subscriptionId) return;
    this.subActionLoading.set(true);
    this.subActionError.set('');
    this.usersService.activateSubscription(user.subscriptionId).subscribe({
      next: () => {
        this.subActionLoading.set(false);
        this.closeSubModal();
        this.loadUsers();
      },
      error: (err) => {
        this.subActionLoading.set(false);
        this.subActionError.set(err.error?.message || 'Failed to activate subscription');
      }
    });
  }

  disableUserSub() {
    const user = this.selectedUserForSub();
    if (!user?.subscriptionId || !this.subDisableReason.trim()) return;
    this.subActionLoading.set(true);
    this.subActionError.set('');
    this.usersService.disableSubscription(user.subscriptionId, this.subDisableReason).subscribe({
      next: () => {
        this.subActionLoading.set(false);
        this.closeSubModal();
        this.loadUsers();
      },
      error: (err) => {
        this.subActionLoading.set(false);
        this.subActionError.set(err.error?.message || 'Failed to disable subscription');
      }
    });
  }

  grantUserSub() {
    const user = this.selectedUserForSub();
    if (!user || !this.grantExpiryDate) {
      this.subActionError.set('Vui lòng chọn ngày kết thúc.');
      return;
    }
    this.subActionLoading.set(true);
    this.subActionError.set('');
    // Send as ISO datetime
    const expiryIso = new Date(this.grantExpiryDate + 'T23:59:59').toISOString();
    this.usersService.grantSubscription(user.id, this.grantPlan, expiryIso, this.grantNotes || undefined).subscribe({
      next: () => {
        this.subActionLoading.set(false);
        this.closeSubModal();
        this.loadUsers();
      },
      error: (err) => {
        this.subActionLoading.set(false);
        this.subActionError.set(err.error?.message || err.error?.errors?.[0]?.message || 'Failed to grant subscription');
      }
    });
  }
}
