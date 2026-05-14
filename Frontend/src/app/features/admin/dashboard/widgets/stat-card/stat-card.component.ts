import { Component, computed, input, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MiniChartComponent } from '../mini-chart/mini-chart.component';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-stat-card',
  standalone: true,
  imports: [CommonModule, RouterLink, MiniChartComponent],
  template: `
    <a [routerLink]="link()"
       [queryParams]="queryParams()"
       class="group block bg-surface-container-low rounded-xl p-6 hover:bg-surface-container transition-colors cursor-pointer">
      <div class="flex items-start justify-between mb-4">
        <span class="material-symbols-outlined" [class]="iconColor()">{{ icon() }}</span>
        <span class="text-xs font-bold uppercase tracking-widest px-2 py-1 rounded-full"
              [ngClass]="deltaClass()">
          {{ deltaLabel() }}
        </span>
      </div>
      <div class="text-3xl font-headline mb-1 text-on-surface">{{ displayValue() }}</div>
      <div class="text-xs uppercase tracking-widest font-bold opacity-60 mb-3">{{ label() }}</div>
      <div class="h-8" [class]="iconColor()">
        <app-mini-chart [data]="sparkline()" [stroke]="'currentColor'" [fill]="'currentColor'" />
      </div>
    </a>
  `
})
export class StatCardComponent {
  icon = input.required<string>();
  label = input.required<string>();
  value = input<number | null>(null);
  previous = input<number | null>(null);
  sparkline = input<number[]>([]);
  iconColor = input<string>('text-primary');
  link = input<string | any[]>('.');
  queryParams = input<Record<string, string> | null>(null);
  format = input<'number' | 'currency'>('number');

  displayValue = computed(() => {
    const v = this.value();
    if (v == null) return '—';
    if (this.format() === 'currency') return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(v);
    return new Intl.NumberFormat('en-US').format(v);
  });

  delta = computed(() => {
    const v = this.value();
    const p = this.previous();
    if (v == null || p == null) return null;
    if (p === 0) return v > 0 ? 100 : 0;
    return ((v - p) / p) * 100;
  });

  deltaLabel = computed(() => {
    const d = this.delta();
    if (d == null) return '—';
    const sign = d > 0 ? '+' : '';
    return `${sign}${d.toFixed(1)}%`;
  });

  deltaClass = computed(() => {
    const d = this.delta();
    if (d == null) return 'bg-surface-container-high text-on-surface-variant';
    if (d > 0) return 'bg-tertiary-container text-on-tertiary-container';
    if (d < 0) return 'bg-error-container/40 text-error';
    return 'bg-surface-container-high text-on-surface-variant';
  });
}
