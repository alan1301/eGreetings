import { Component, inject, signal, OnInit, ChangeDetectionStrategy, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminSidebarComponent } from '../components/admin-sidebar/admin-sidebar.component';
import { AdminUsersService, AdminUserDto } from '../../../core/services/admin-users.service';
import { PaginationMeta } from '../../../shared/models/api-response.model';
import { UserListTableComponent, UserActionEvent } from './components/user-list-table/user-list-table.component';
import { UserCreateModalComponent, CreateUserPayload } from './components/user-create-modal/user-create-modal.component';
import { UserEditModalComponent, EditUserPayload } from './components/user-edit-modal/user-edit-modal.component';
import { UserLockConfirmDialogComponent } from './components/user-lock-confirm-dialog/user-lock-confirm-dialog.component';
import { UserDeleteConfirmDialogComponent } from './components/user-delete-confirm-dialog/user-delete-confirm-dialog.component';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-admin-users',
  standalone: true,
  imports: [
    CommonModule, FormsModule, AdminSidebarComponent,
    UserListTableComponent, UserCreateModalComponent,
    UserEditModalComponent, UserLockConfirmDialogComponent,
    UserDeleteConfirmDialogComponent
  ],
  providers: [DatePipe],
  templateUrl: './admin-users.component.html'
})
export class AdminUsersComponent implements OnInit {
  private usersService = inject(AdminUsersService);
  private destroyRef = inject(DestroyRef);

  users = signal<AdminUserDto[]>([]);
  meta = signal<PaginationMeta | null>(null);
  loading = signal<boolean>(false);

  filterSearch = '';
  filterRole = '';
  filterAccountStatus = '';
  filterSubPlan = '';
  currentPage = 1;
  pageSize = 20;

  selectedUserToLock = signal<AdminUserDto | null>(null);
  selectedUserToEdit = signal<AdminUserDto | null>(null);
  selectedUserToDelete = signal<AdminUserDto | null>(null);

  showCreateModal = signal(false);
  createLoading = signal(false);
  createError = signal('');

  editLoading = signal(false);
  editError = signal('');

  deleteLoading = signal(false);
  deleteError = signal('');

  ngOnInit() { this.loadUsers(); }

  loadUsers() {
    this.loading.set(true);
    this.usersService.getUsers(
      this.currentPage, this.pageSize,
      this.filterSearch || undefined,
      this.filterRole || undefined,
      this.filterAccountStatus || undefined,
      this.filterSubPlan || undefined
    ).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
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

  handleTableAction(e: UserActionEvent) {
    switch (e.action) {
      case 'edit':   this.selectedUserToEdit.set(e.user); this.editError.set(''); break;
      case 'lock':   this.selectedUserToLock.set(e.user); break;
      case 'unlock': this.unlockUser(e.user); break;
      case 'delete': this.selectedUserToDelete.set(e.user); this.deleteError.set(''); break;
    }
  }

  private unlockUser(user: AdminUserDto) {
    if (!confirm(`Are you sure you want to unlock ${user.email}?`)) return;
    this.usersService.unlockUser(user.id).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => this.loadUsers(),
      error: (err) => {
        console.error('Failed to unlock user', err);
        alert('Failed to unlock user: ' + (err.error?.message || 'Unknown error'));
      }
    });
  }

  confirmLock(reason: string) {
    const user = this.selectedUserToLock();
    if (!user) return;
    this.usersService.lockUser(user.id, reason).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => { this.selectedUserToLock.set(null); this.loadUsers(); },
      error: (err) => {
        console.error('Failed to lock user', err);
        alert('Failed to lock user: ' + (err.error?.message || 'Unknown error'));
      }
    });
  }

  openCreateModal() {
    this.createError.set('');
    this.showCreateModal.set(true);
  }

  submitCreate(payload: CreateUserPayload) {
    const { fullName, email, password, role } = payload;
    if (!fullName.trim() || !email.trim() || !password.trim()) {
      this.createError.set('Full name, email, and password are required.');
      return;
    }
    this.createLoading.set(true);
    this.createError.set('');
    this.usersService.createUser(fullName, email, password, role).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        this.createLoading.set(false);
        this.showCreateModal.set(false);
        this.loadUsers();
      },
      error: (err) => {
        this.createLoading.set(false);
        this.createError.set(err.error?.message || err.error?.errors?.[0]?.message || 'Failed to create user.');
      }
    });
  }

  submitEdit(payload: EditUserPayload) {
    const user = this.selectedUserToEdit();
    if (!user) return;
    const { fullName, email, role } = payload;
    if (!fullName.trim() || !email.trim()) {
      this.editError.set('Full name and email are required.');
      return;
    }
    this.editLoading.set(true);
    this.editError.set('');
    this.usersService.updateUser(user.id, fullName, email, role).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        this.editLoading.set(false);
        this.selectedUserToEdit.set(null);
        this.loadUsers();
      },
      error: (err) => {
        this.editLoading.set(false);
        this.editError.set(err.error?.message || err.error?.errors?.[0]?.message || 'Failed to update user.');
      }
    });
  }

  confirmDelete() {
    const user = this.selectedUserToDelete();
    if (!user) return;
    this.deleteLoading.set(true);
    this.deleteError.set('');
    this.usersService.deleteUser(user.id).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        this.deleteLoading.set(false);
        this.selectedUserToDelete.set(null);
        this.loadUsers();
      },
      error: (err) => {
        this.deleteLoading.set(false);
        this.deleteError.set(err.error?.message || err.error?.errors?.[0]?.message || 'Failed to delete user.');
      }
    });
  }
}
