import { ChangeDetectionStrategy, Component, input, output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

export interface CreateUserPayload {
  fullName: string;
  email: string;
  password: string;
  role: string;
}

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-user-create-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="fixed inset-0 z-50 bg-on-surface/40 backdrop-blur-sm flex items-center justify-center p-4">
      <div class="bg-surface rounded-2xl shadow-xl max-w-md w-full p-6 relative">
        <h3 class="font-headline text-xl font-bold text-on-surface mb-5">Create New User</h3>

        <div class="flex flex-col gap-4 mb-6">
          <div>
            <label class="block text-xs font-bold text-on-surface-variant uppercase tracking-wider mb-1.5">
              Full Name <span class="text-error">*</span>
            </label>
            <input [(ngModel)]="form.fullName" type="text" placeholder="Nguyen Van A"
                   class="w-full bg-surface-container-lowest border border-outline-variant/30 rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-primary/50 outline-none">
          </div>
          <div>
            <label class="block text-xs font-bold text-on-surface-variant uppercase tracking-wider mb-1.5">
              Email <span class="text-error">*</span>
            </label>
            <input [(ngModel)]="form.email" type="email" placeholder="user@example.com"
                   class="w-full bg-surface-container-lowest border border-outline-variant/30 rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-primary/50 outline-none">
          </div>
          <div>
            <label class="block text-xs font-bold text-on-surface-variant uppercase tracking-wider mb-1.5">
              Password <span class="text-error">*</span>
            </label>
            <input [(ngModel)]="form.password" type="password" placeholder="Min. 8 characters"
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
                  class="px-4 py-2 text-sm font-bold bg-primary text-on-primary hover:bg-primary/90 rounded-lg shadow transition-colors flex items-center gap-2 disabled:opacity-60">
            <span class="material-symbols-outlined text-[18px]">person_add</span>
            {{ loading() ? 'Creating...' : 'Create User' }}
          </button>
        </div>
      </div>
    </div>
  `
})
export class UserCreateModalComponent {
  loading = input<boolean>(false);
  errorMessage = input<string>('');

  submit = output<CreateUserPayload>();
  close = output<void>();

  form: CreateUserPayload = { fullName: '', email: '', password: '', role: 'User' };
}
