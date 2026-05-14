import { ChangeDetectionStrategy, Component, OnChanges, SimpleChanges, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminUserDto } from '../../../../../core/services/admin-users.service';

export interface EditUserPayload { fullName: string; email: string; role: string; }

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-user-edit-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="fixed inset-0 z-50 bg-on-surface/40 backdrop-blur-sm flex items-center justify-center p-4">
      <div class="bg-surface rounded-2xl shadow-xl max-w-md w-full p-6 relative">
        <h3 class="font-headline text-xl font-bold text-on-surface mb-1">Edit User</h3>
        <p class="text-sm text-on-surface-variant mb-5">{{ user().email }}</p>

        <div class="flex flex-col gap-4 mb-6">
          <div>
            <label class="block text-xs font-bold text-on-surface-variant uppercase tracking-wider mb-1.5">
              Full Name <span class="text-error">*</span>
            </label>
            <input [(ngModel)]="form.fullName" type="text"
                   class="w-full bg-surface-container-lowest border border-outline-variant/30 rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-primary/50 outline-none">
          </div>
          <div>
            <label class="block text-xs font-bold text-on-surface-variant uppercase tracking-wider mb-1.5">
              Email <span class="text-error">*</span>
            </label>
            <input [(ngModel)]="form.email" type="email"
                   class="w-full bg-surface-container-lowest border border-outline-variant/30 rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-primary/50 outline-none">
          </div>
          <div>
            <label class="block text-xs font-bold text-on-surface-variant uppercase tracking-wider mb-1.5">Role</label>
            <select [(ngModel)]="form.role"
                    class="w-full bg-surface-container-lowest border border-outline-variant/30 rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-primary/50 outline-none">
              <option value="User">User</option>
              <option value="Admin">Admin</option>
            </select>
          </div>
        </div>

        @if (errorMessage()) {
          <p class="text-sm text-error mb-4 bg-error-container/30 rounded-lg px-3 py-2">{{ errorMessage() }}</p>
        }

        <div class="flex gap-3 justify-end">
          <button (click)="close.emit()"
                  class="px-4 py-2 text-sm font-bold text-on-surface-variant hover:bg-surface-container rounded-lg transition-colors">
            Cancel
          </button>
          <button (click)="submit.emit(form)" [disabled]="loading()"
                  class="px-4 py-2 text-sm font-bold bg-secondary text-on-secondary hover:bg-secondary/90 rounded-lg shadow transition-colors flex items-center gap-2 disabled:opacity-60">
            <span class="material-symbols-outlined text-[18px]">save</span>
            {{ loading() ? 'Saving...' : 'Save Changes' }}
          </button>
        </div>
      </div>
    </div>
  `
})
export class UserEditModalComponent implements OnChanges {
  user = input.required<AdminUserDto>();
  loading = input<boolean>(false);
  errorMessage = input<string>('');

  submit = output<EditUserPayload>();
  close = output<void>();

  form: EditUserPayload = { fullName: '', email: '', role: '' };

  ngOnChanges(changes: SimpleChanges) {
    if (changes['user']) {
      const u = this.user();
      this.form = { fullName: u.fullName, email: u.email, role: u.role };
    }
  }
}
