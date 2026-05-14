import { ChangeDetectionStrategy, Component, input, output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminUserDto } from '../../../../../core/services/admin-users.service';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-user-lock-confirm-dialog',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="fixed inset-0 z-50 bg-on-surface/40 backdrop-blur-sm flex items-center justify-center p-4">
      <div class="bg-surface rounded-2xl shadow-xl max-w-md w-full p-6 relative">
        <h3 class="font-headline text-xl font-bold text-on-surface mb-2">Lock User Account</h3>
        <p class="text-sm text-on-surface-variant mb-6">
          Are you sure you want to lock <strong>{{ user().email }}</strong>? This action will invalidate their active sessions and disable any active subscriptions.
        </p>

        <div class="mb-6">
          <label class="block text-xs font-bold text-on-surface-variant uppercase tracking-wider mb-2">Reason (Optional)</label>
          <textarea [(ngModel)]="reason" rows="3"
                    placeholder="Why is this user being locked?"
                    class="w-full bg-surface-container-lowest border border-outline-variant/30 rounded-lg p-3 text-sm focus:ring-2 focus:ring-error/50 outline-none resize-none"></textarea>
        </div>

        <div class="flex gap-3 justify-end">
          <button (click)="cancel.emit()"
                  class="px-4 py-2 text-sm font-bold text-on-surface-variant hover:bg-surface-container rounded-lg transition-colors">
            Cancel
          </button>
          <button (click)="confirm.emit(reason)"
                  class="px-4 py-2 text-sm font-bold bg-error text-onError hover:bg-error/90 rounded-lg shadow transition-colors flex items-center gap-2">
            <span class="material-symbols-outlined text-[18px]">lock</span>
            Lock User
          </button>
        </div>
      </div>
    </div>
  `
})
export class UserLockConfirmDialogComponent {
  user = input.required<AdminUserDto>();
  confirm = output<string>();
  cancel = output<void>();

  reason = '';
}
