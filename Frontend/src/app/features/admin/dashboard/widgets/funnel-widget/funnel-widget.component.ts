import { Component, computed, input, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FunnelDto } from '../../admin-dashboard.types';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-funnel-widget',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="bg-secondary-container text-on-secondary-container rounded-xl p-8 h-full flex flex-col">
      <div class="flex items-start justify-between mb-2">
        <h3 class="text-xl font-headline">Subscription Funnel</h3>
        <span class="material-symbols-outlined opacity-60">tune</span>
      </div>
      <p class="text-sm opacity-70 mb-6">Pending → Active → Expired → Disabled</p>

      @if (data(); as d) {
        <div class="space-y-3 flex-1">
          @for (row of rows(); track row.key) {
            <div>
              <div class="flex justify-between text-xs font-bold mb-1.5">
                <span>{{ row.label }}</span>
                <span>{{ row.value }}</span>
              </div>
              <div class="w-full bg-on-secondary-container/10 h-2 rounded-full overflow-hidden">
                <div class="h-full rounded-full transition-all duration-700"
                     [style.width.%]="row.pct"
                     [class]="row.colorClass"></div>
              </div>
            </div>
          }
        </div>
        <div class="mt-6 pt-4 border-t border-on-secondary-container/20">
          <div class="flex justify-between items-baseline">
            <span class="text-xs uppercase tracking-widest font-bold opacity-70">Conversion</span>
            <span class="text-2xl font-headline">{{ d.conversionRate }}%</span>
          </div>
          <p class="text-xs opacity-60 mt-1">Pending → paid (lifetime)</p>
        </div>
      } @else {
        <div class="flex-1 flex items-center justify-center text-sm opacity-60">Loading…</div>
      }
    </div>
  `
})
export class FunnelWidgetComponent {
  data = input<FunnelDto | null>(null);

  rows = computed(() => {
    const d = this.data();
    if (!d) return [];
    const max = Math.max(d.pending, d.active, d.expired, d.disabled, 1);
    return [
      { key: 'pending', label: 'Pending', value: d.pending, pct: (d.pending / max) * 100, colorClass: 'bg-on-error-container' },
      { key: 'active', label: 'Active', value: d.active, pct: (d.active / max) * 100, colorClass: 'bg-on-secondary-container' },
      { key: 'expired', label: 'Expired', value: d.expired, pct: (d.expired / max) * 100, colorClass: 'bg-on-secondary-container/60' },
      { key: 'disabled', label: 'Disabled', value: d.disabled, pct: (d.disabled / max) * 100, colorClass: 'bg-on-secondary-container/30' },
    ];
  });
}
