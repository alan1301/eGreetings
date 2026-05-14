import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { AdminUserDto } from '../../../../../core/services/admin-users.service';
import { PaginationMeta } from '../../../../../shared/models/api-response.model';

export type UserAction = 'edit' | 'lock' | 'unlock' | 'delete';
export interface UserActionEvent { action: UserAction; user: AdminUserDto; }

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-user-list-table',
  standalone: true,
  imports: [CommonModule, DatePipe],
  template: `
    <div class="bg-surface-container-low rounded-xl border border-outline-variant/20 overflow-hidden">
      <div class="overflow-x-auto">
        <table class="w-full text-left border-collapse">
          <thead>
            <tr class="bg-surface-container text-on-surface-variant text-xs uppercase tracking-widest border-b border-outline-variant/20">
              <th class="py-4 px-6 font-medium whitespace-nowrap">User</th>
              <th class="py-4 px-6 font-medium">Role</th>
              <th class="py-4 px-6 font-medium">Status</th>
              <th class="py-4 px-6 font-medium">Subscription</th>
              <th class="py-4 px-6 font-medium whitespace-nowrap">Joined Date</th>
              <th class="py-4 px-6 font-medium text-right">Actions</th>
            </tr>
          </thead>
          <tbody class="text-sm divide-y divide-outline-variant/10">
            @if (loading() && users().length === 0) {
              <tr>
                <td colspan="6" class="py-12 text-center text-on-surface-variant">
                  <span class="material-symbols-outlined animate-spin text-4xl mb-2 opacity-50">progress_activity</span>
                  <p>Loading users...</p>
                </td>
              </tr>
            } @else if (users().length === 0) {
              <tr>
                <td colspan="6" class="py-12 text-center text-on-surface-variant">
                  <span class="material-symbols-outlined text-4xl mb-2 opacity-50">group_off</span>
                  <p>No users found matching your filters.</p>
                </td>
              </tr>
            } @else {
              @for (user of users(); track user.id) {
                <tr class="hover:bg-surface-container transition-colors group">
                  <td class="py-3 px-6 whitespace-nowrap">
                    <div class="flex flex-col">
                      <span class="font-bold text-on-surface">{{ user.fullName }}</span>
                      <span class="text-xs text-on-surface-variant">{{ user.email }}</span>
                    </div>
                  </td>
                  <td class="py-3 px-6 whitespace-nowrap text-xs font-bold text-on-surface-variant uppercase">{{ user.role }}</td>
                  <td class="py-3 px-6 whitespace-nowrap">
                    <span class="px-2 py-1 rounded text-xs font-bold"
                          [ngClass]="user.status === 'Active' ? 'bg-primary-container text-on-primary-container' : 'bg-error-container text-error'">
                      {{ user.status }}
                    </span>
                  </td>
                  <td class="py-3 px-6 whitespace-nowrap">
                    @if (user.subscriptionStatus && user.subscriptionStatus !== 'Disabled' && user.subscriptionStatus !== 'Expired') {
                      <div class="flex flex-col gap-0.5 items-start">
                        <span class="px-2.5 py-1 rounded-lg text-xs font-bold" [ngClass]="planColor(user.subscriptionPlan)">
                          {{ planLabel(user.subscriptionPlan) }}
                        </span>
                        @if (user.subscriptionExpiry) {
                          <div class="text-[10px] text-on-surface-variant">
                            Exp: {{ user.subscriptionExpiry | date:'dd/MM/yyyy' }}
                          </div>
                        }
                      </div>
                    } @else {
                      <span class="text-xs text-outline italic">🆓 Free Tier</span>
                    }
                  </td>
                  <td class="py-3 px-6 whitespace-nowrap text-on-surface-variant text-xs">
                    {{ user.createdAt | date:'dd/MM/yyyy' }}
                  </td>
                  <td class="py-3 px-6 whitespace-nowrap text-right">
                    @if (user.role !== 'Admin') {
                      <div class="flex items-center justify-end gap-2">
                        @if (user.status === 'Active') {
                          <button (click)="action.emit({ action: 'lock', user })"
                                  class="text-error hover:bg-error-container/50 px-3 py-1.5 rounded-lg transition-colors flex items-center gap-1.5 text-sm font-bold">
                            <span class="material-symbols-outlined text-[18px]">lock</span> Lock
                          </button>
                        } @else if (user.status === 'Locked') {
                          <button (click)="action.emit({ action: 'unlock', user })"
                                  class="text-primary hover:bg-primary-container/50 px-3 py-1.5 rounded-lg transition-colors flex items-center gap-1.5 text-sm font-bold">
                            <span class="material-symbols-outlined text-[18px]">lock_open</span> Unlock
                          </button>
                        }
                        <button (click)="action.emit({ action: 'edit', user })"
                                class="text-secondary hover:bg-secondary-container/50 px-3 py-1.5 rounded-lg transition-colors flex items-center gap-1.5 text-sm font-bold">
                          <span class="material-symbols-outlined text-[18px]">edit</span> Edit
                        </button>
                        <button (click)="action.emit({ action: 'delete', user })"
                                class="text-error hover:bg-error-container/50 px-3 py-1.5 rounded-lg transition-colors flex items-center gap-1.5 text-sm font-bold">
                          <span class="material-symbols-outlined text-[18px]">delete</span> Delete
                        </button>
                      </div>
                    } @else {
                      <span class="text-xs text-outline italic">System</span>
                    }
                  </td>
                </tr>
              }
            }
          </tbody>
        </table>
      </div>

      @if (meta()) {
        <div class="bg-surface-container px-6 py-4 border-t border-outline-variant/20 flex items-center justify-between">
          <span class="text-sm text-on-surface-variant">
            Showing {{ (meta()!.page - 1) * meta()!.pageSize + 1 }} -
            {{ Math.min(meta()!.page * meta()!.pageSize, meta()!.total) }}
            of <span class="font-bold text-on-surface">{{ meta()!.total }}</span> users
          </span>
          <div class="flex gap-2">
            <button [disabled]="meta()!.page <= 1" (click)="pageChange.emit(meta()!.page - 1)"
                    class="w-8 h-8 flex items-center justify-center rounded bg-surface hover:bg-surface-container-high disabled:opacity-50 disabled:cursor-not-allowed border border-outline-variant/30 transition-colors">
              <span class="material-symbols-outlined text-[18px]">chevron_left</span>
            </button>
            <div class="flex gap-1 items-center px-2 text-sm font-medium">Page {{ meta()!.page }}</div>
            <button [disabled]="meta()!.page * meta()!.pageSize >= meta()!.total" (click)="pageChange.emit(meta()!.page + 1)"
                    class="w-8 h-8 flex items-center justify-center rounded bg-surface hover:bg-surface-container-high disabled:opacity-50 disabled:cursor-not-allowed border border-outline-variant/30 transition-colors">
              <span class="material-symbols-outlined text-[18px]">chevron_right</span>
            </button>
          </div>
        </div>
      }
    </div>
  `
})
export class UserListTableComponent {
  users = input.required<AdminUserDto[]>();
  loading = input<boolean>(false);
  meta = input<PaginationMeta | null>(null);

  action = output<UserActionEvent>();
  pageChange = output<number>();

  protected readonly Math = Math;

  planLabel(plan?: string): string {
    switch (plan) {
      case 'Monthly': return '📅 Monthly';
      case 'Annual':  return '🌟 Annual';
      default:        return '🆓 Free Tier';
    }
  }

  planColor(plan?: string): string {
    switch (plan) {
      case 'Monthly': return 'bg-secondary/20 text-secondary';
      case 'Annual':  return 'bg-tertiary/20 text-tertiary';
      default:        return 'bg-surface-container text-on-surface-variant';
    }
  }
}
