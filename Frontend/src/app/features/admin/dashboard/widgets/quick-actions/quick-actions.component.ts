import { Component, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-quick-actions',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="bg-surface-container-lowest rounded-xl p-5">
      <h3 class="text-sm font-bold uppercase tracking-widest text-on-surface-variant mb-4">Quick Actions</h3>
      <div class="grid grid-cols-2 gap-2">
        @for (a of actions; track a.label) {
          <a [routerLink]="a.link" [queryParams]="a.params ?? null"
             class="flex flex-col items-center text-center gap-1.5 p-3 rounded-lg bg-surface-container-low hover:bg-primary-container hover:text-on-primary-container transition-colors">
            <span class="material-symbols-outlined text-base">{{ a.icon }}</span>
            <span class="text-xs font-bold uppercase tracking-tighter leading-tight">{{ a.label }}</span>
          </a>
        }
      </div>
    </div>
  `
})
export class QuickActionsComponent {
  actions = [
    { icon: 'add_circle', label: 'Add Card', link: '/admin/cards', params: { new: 'true' } },
    { icon: 'payments', label: 'Payments', link: '/admin/payments', params: null },
    { icon: 'forum', label: 'Feedback', link: '/admin/feedback', params: { status: 'Unread' } },
    { icon: 'description', label: 'Logs', link: '/admin/logs' },
  ];
}
