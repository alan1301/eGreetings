import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AdminUserDto } from '../../../../../core/services/admin-users.service';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-user-delete-confirm-dialog',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="fixed inset-0 z-50 bg-on-surface/40 backdrop-blur-sm flex items-center justify-center p-4">
      <div class="bg-surface rounded-2xl shadow-xl max-w-md w-full p-6 relative">
        <h3 class="font-headline text-xl font-bold text-on-surface mb-2">Delete User Account</h3>
        <p class="text-sm text-on-surface-variant mb-6">
          Are you sure you want to delete <strong>{{ user().fullName }}</strong>
          (<span class="text-outline">{{ user().email }}</span>)?
          This action cannot be undone.
        </p>

        @if (errorMessage()) {
          <p class="text-sm text-error mb-4 bg-error-container/30 rounded-lg px-3 py-2">{{ errorMessage() }}</p>
        }

        <div class="flex gap-3 justify-end">
          <button (click)="cancel.emit()"
                  class="px-4 py-2 text-sm font-bold text-on-surface-variant hover:bg-surface-container rounded-lg transition-colors">
            Cancel
          </button>
          <button (click)="confirm.emit()" [disabled]="loading()"
                  class="px-4 py-2 text-sm font-bold bg-error text-onError hover:bg-error/90 rounded-lg shadow transition-colors flex items-center gap-2 disabled:opacity-60">
            <span class="material-symbols-outlined text-[18px]">delete</span>
            {{ loading() ? 'Deleting...' : 'Delete User' }}
          </button>
        </div>
      </div>
    </div>
  `
})
export class UserDeleteConfirmDialogComponent {
  user = input.required<AdminUserDto>();
  loading = input<boolean>(false);
  errorMessage = input<string>('');

  confirm = output<void>();
  cancel = output<void>();
}
