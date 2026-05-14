import { Component, computed, input, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HealthDto } from '../../admin-dashboard.types';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-health-widget',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="bg-surface-container-lowest rounded-xl p-5">
      <h3 class="text-sm font-bold uppercase tracking-widest text-on-surface-variant mb-4">System Health</h3>
      @if (data(); as d) {
        <ul class="space-y-2.5 text-sm">
          <li class="flex items-center justify-between">
            <span class="text-on-surface-variant">Database</span>
            <span class="px-2 py-0.5 rounded-full text-xs font-bold uppercase"
                  [class]="d.db === 'ok' ? 'bg-green-100 text-green-700' : 'bg-error-container/20 text-error'">
              {{ d.db }}
            </span>
          </li>
          <li class="flex items-center justify-between">
            <span class="text-on-surface-variant">Email queue</span>
            <span class="font-bold text-on-surface">{{ d.emailQueueDepth }}</span>
          </li>
          <li class="flex items-center justify-between">
            <span class="text-on-surface-variant">Subscribe job</span>
            <span class="px-2 py-0.5 rounded-full text-xs font-bold uppercase"
                  [class]="jobClass()">
              {{ d.subscribeJobLastStatus }}
            </span>
          </li>
          @if (d.subscribeJobLastRun) {
            <li class="flex items-center justify-between text-xs">
              <span class="text-on-surface-variant">Last run</span>
              <span class="text-on-surface-variant">{{ formatTime(d.subscribeJobLastRun) }}</span>
            </li>
          }
          <li class="flex items-center justify-between text-xs">
            <span class="text-on-surface-variant">API uptime</span>
            <span class="text-on-surface-variant">{{ uptime() }}</span>
          </li>
        </ul>
      } @else {
        <div class="text-sm text-on-surface-variant">Loading…</div>
      }
    </div>
  `
})
export class HealthWidgetComponent {
  data = input<HealthDto | null>(null);

  uptime = computed(() => {
    const d = this.data();
    if (!d) return '—';
    const s = d.apiUptimeSeconds;
    if (s < 60) return `${s}s`;
    if (s < 3600) return `${Math.floor(s / 60)}m`;
    if (s < 86400) return `${Math.floor(s / 3600)}h`;
    return `${Math.floor(s / 86400)}d`;
  });

  jobClass = computed(() => {
    const s = this.data()?.subscribeJobLastStatus?.toLowerCase();
    if (s === 'success') return 'bg-green-100 text-green-700';
    if (s === 'failed') return 'bg-error-container/20 text-error';
    return 'bg-surface-container-high text-on-surface-variant';
  });

  formatTime(iso: string): string {
    return new Date(iso).toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' });
  }
}
