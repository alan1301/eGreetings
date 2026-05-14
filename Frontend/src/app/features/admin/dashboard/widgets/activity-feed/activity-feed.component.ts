import { Component, input, output, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivityItemDto } from '../../admin-dashboard.types';

interface ActionEvent {
  item: ActivityItemDto;
  action: 'approve' | 'mark-read' | 'view';
}

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-activity-feed',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="bg-surface-container-lowest rounded-xl flex flex-col h-full">
      <div class="p-6 border-b border-surface-container-high flex justify-between items-center">
        <div>
          <h3 class="text-xl font-headline text-on-surface">Live Activity</h3>
          <p class="text-xs text-on-surface-variant">Real-time stream across the platform</p>
        </div>
        <span class="flex items-center gap-2 text-xs font-bold uppercase tracking-widest text-tertiary">
          <span class="w-2 h-2 rounded-full bg-tertiary animate-pulse"></span> Live
        </span>
      </div>
      <div class="overflow-y-auto flex-1 divide-y divide-surface-container-low" style="max-height: 480px;">
        @if (items().length === 0) {
          <div class="p-8 text-center text-sm text-on-surface-variant">No recent activity</div>
        }
        @for (item of items(); track item.id) {
          <div class="p-4 hover:bg-surface-container-low/40 transition-colors flex items-start gap-3">
            <div class="w-9 h-9 rounded-full flex items-center justify-center shrink-0"
                 [class]="iconBg(item.type)">
              <span class="material-symbols-outlined text-base">{{ iconFor(item.type) }}</span>
            </div>
            <div class="flex-1 min-w-0">
              <div class="flex items-center gap-2 mb-0.5">
                <span class="font-medium text-sm text-on-surface truncate">{{ item.title }}</span>
                <span class="text-xs px-2 py-0.5 rounded-full font-bold uppercase tracking-tighter"
                      [class]="statusClass(item.status)">{{ item.status }}</span>
              </div>
              <div class="text-xs text-on-surface-variant truncate">
                {{ item.actorName }}
                @if (item.subtitle) { · {{ item.subtitle }} }
              </div>
              <div class="text-xs text-on-surface-variant mt-0.5">{{ relativeTime(item.occurredAt) }}</div>
            </div>
            @if (item.action) {
              <button (click)="onAction(item)"
                      class="shrink-0 px-3 py-1 rounded-full text-xs font-bold uppercase tracking-widest transition-colors"
                      [class]="actionBtnClass(item.action)">
                {{ actionLabel(item.action) }}
              </button>
            }
          </div>
        }
      </div>
    </div>
  `
})
export class ActivityFeedComponent {
  items = input<ActivityItemDto[]>([]);
  action = output<ActionEvent>();

  onAction(item: ActivityItemDto) {
    if (!item.action) return;
    this.action.emit({ item, action: item.action });
  }

  iconFor(type: string): string {
    switch (type) {
      case 'GreetingSent': return 'mail';
      case 'PaymentPending': return 'pending_actions';
      case 'SubscriptionCreated': return 'workspace_premium';
      case 'FeedbackSubmitted': return 'forum';
      case 'AdminAction': return 'admin_panel_settings';
      case 'SystemError': return 'error';
      default: return 'bolt';
    }
  }

  iconBg(type: string): string {
    switch (type) {
      case 'GreetingSent': return 'bg-primary-container text-on-primary-container';
      case 'PaymentPending': return 'bg-error-container/60 text-on-error-container';
      case 'SubscriptionCreated': return 'bg-secondary-container text-on-secondary-container';
      case 'FeedbackSubmitted': return 'bg-tertiary-container text-on-tertiary-container';
      case 'SystemError': return 'bg-error-container/80 text-error';
      default: return 'bg-surface-container-high text-on-surface-variant';
    }
  }

  statusClass(status: string): string {
    const s = status.toLowerCase();
    if (s === 'success' || s === 'active' || s === 'sent' || s === 'read')
      return 'bg-green-100 text-green-700';
    if (s === 'pending' || s === 'unread' || s === 'scheduled')
      return 'bg-amber-100 text-amber-700';
    if (s === 'failed' || s === 'disabled' || s === 'expired')
      return 'bg-error-container/20 text-error';
    return 'bg-surface-container-high text-on-surface-variant';
  }

  actionLabel(a: string): string {
    if (a === 'approve') return 'Approve';
    if (a === 'mark-read') return 'Read';
    return 'View';
  }

  actionBtnClass(a: string): string {
    if (a === 'approve') return 'bg-primary text-on-primary hover:opacity-90';
    if (a === 'mark-read') return 'bg-tertiary-container text-on-tertiary-container hover:opacity-90';
    return 'bg-surface-container-high text-on-surface hover:bg-surface-container-highest';
  }

  relativeTime(iso: string): string {
    const diff = (Date.now() - new Date(iso).getTime()) / 1000;
    if (diff < 60) return `${Math.floor(diff)}s ago`;
    if (diff < 3600) return `${Math.floor(diff / 60)}m ago`;
    if (diff < 86400) return `${Math.floor(diff / 3600)}h ago`;
    return `${Math.floor(diff / 86400)}d ago`;
  }
}
