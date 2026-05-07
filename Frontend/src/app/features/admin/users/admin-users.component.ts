import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AdminSidebarComponent } from '../components/admin-sidebar/admin-sidebar.component';
import { AdminUsersService, AdminUserDto } from '../../../core/services/admin-users.service';
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
          this.loadUsers(); // Refresh
        },
        error: (err) => {
          console.error('Failed to unlock user', err);
          alert('Failed to unlock user: ' + (err.error?.message || 'Unknown error'));
        }
      });
    }
  }
}
