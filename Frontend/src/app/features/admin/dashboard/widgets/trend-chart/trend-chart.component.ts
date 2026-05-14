import { Component, computed, signal, input, output, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TimeseriesDto, TimeRange } from '../../admin-dashboard.types';

interface Point { x: number; y: number; date: string; sent: number; revenue: number; }

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-trend-chart',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="bg-surface-container-lowest rounded-xl p-8 h-full">
      <div class="flex justify-between items-start mb-6">
        <div>
          <h3 class="text-xl font-headline mb-1 text-on-surface">Greetings & Revenue Trend</h3>
          <p class="text-sm text-on-surface-variant">Daily volume and revenue over time</p>
        </div>
        <div class="inline-flex bg-surface-container rounded-full p-1">
          @for (r of ranges; track r) {
            <button (click)="rangeChange.emit(r)"
                    class="px-3 py-1 text-xs font-bold uppercase tracking-widest rounded-full transition-colors"
                    [class]="r === range() ? 'bg-primary text-on-primary' : 'text-on-surface-variant hover:text-on-surface'">
              {{ r }}
            </button>
          }
        </div>
      </div>
      @if (!data() || data()!.points.length === 0) {
        <div class="h-56 flex items-center justify-center text-on-surface-variant text-sm">No data</div>
      } @else {
        <svg [attr.viewBox]="'0 0 ' + W + ' ' + H" class="w-full h-56" preserveAspectRatio="none"
             (mousemove)="onMove($event)" (mouseleave)="hoverIdx.set(null)">
          <!-- gridlines -->
          @for (g of [0.25, 0.5, 0.75]; track g) {
            <line [attr.x1]="0" [attr.x2]="W" [attr.y1]="H * g" [attr.y2]="H * g"
                  stroke="currentColor" class="text-on-surface-variant" opacity="0.1" stroke-dasharray="2 4" />
          }
          <!-- sent area -->
          <path [attr.d]="sentArea()" fill="currentColor" class="text-primary" opacity="0.12" />
          <path [attr.d]="sentLine()" fill="none" stroke="currentColor" class="text-primary" stroke-width="2" stroke-linejoin="round" />
          <!-- revenue line -->
          <path [attr.d]="revenueLine()" fill="none" stroke="currentColor" class="text-tertiary" stroke-width="2" stroke-dasharray="4 3" stroke-linejoin="round" />
          <!-- crosshair -->
          @if (hoverIdx() !== null) {
            <line [attr.x1]="sentPoints()[hoverIdx()!].x" [attr.x2]="sentPoints()[hoverIdx()!].x"
                  y1="0" [attr.y2]="H" stroke="currentColor" class="text-on-surface-variant" opacity="0.4" />
            <circle [attr.cx]="sentPoints()[hoverIdx()!].x" [attr.cy]="sentPoints()[hoverIdx()!].y"
                    r="4" fill="currentColor" class="text-primary" />
          }
        </svg>
        <div class="flex justify-between items-center mt-4">
          <div class="flex gap-4 text-xs">
            <span class="flex items-center gap-1.5"><span class="w-2 h-2 rounded-full bg-primary"></span> Greetings</span>
            <span class="flex items-center gap-1.5"><span class="w-2 h-2 rounded-full bg-tertiary"></span> Revenue ($)</span>
          </div>
          @if (hoverIdx() !== null) {
            <div class="text-xs text-on-surface-variant">
              <span class="font-bold text-on-surface">{{ formatDate(data()!.points[hoverIdx()!].date) }}</span>
              · {{ data()!.points[hoverIdx()!].greetingsSent }} sent
              · {{ formatRevenue(data()!.points[hoverIdx()!].revenue) }}
            </div>
          }
        </div>
      }
    </div>
  `
})
export class TrendChartComponent {
  data = input<TimeseriesDto | null>(null);
  range = input<TimeRange>('7d');
  rangeChange = output<TimeRange>();

  ranges: TimeRange[] = ['7d', '30d'];
  W = 600;
  H = 220;
  hoverIdx = signal<number | null>(null);

  private points = computed<Point[]>(() => {
    const d = this.data();
    if (!d) return [];
    const pts = d.points;
    if (pts.length === 0) return [];
    const maxSent = Math.max(...pts.map(p => p.greetingsSent), 1);
    const step = pts.length === 1 ? 0 : this.W / (pts.length - 1);
    return pts.map((p, i) => ({
      x: pts.length === 1 ? this.W / 2 : i * step,
      y: this.H - (p.greetingsSent / maxSent) * (this.H - 16) - 8,
      date: p.date,
      sent: p.greetingsSent,
      revenue: p.revenue,
    }));
  });

  sentPoints = computed(() => this.points());

  sentLine = computed(() => {
    const pts = this.points();
    return pts.map((p, i) => `${i === 0 ? 'M' : 'L'}${p.x.toFixed(2)},${p.y.toFixed(2)}`).join(' ');
  });

  sentArea = computed(() => {
    const pts = this.points();
    if (pts.length < 2) return '';
    const line = this.sentLine();
    return `${line} L${pts[pts.length - 1].x.toFixed(2)},${this.H} L${pts[0].x.toFixed(2)},${this.H} Z`;
  });

  revenueLine = computed(() => {
    const d = this.data();
    if (!d) return '';
    const pts = d.points;
    if (pts.length === 0) return '';
    const maxRev = Math.max(...pts.map(p => p.revenue), 1);
    const step = pts.length === 1 ? 0 : this.W / (pts.length - 1);
    return pts.map((p, i) => {
      const x = pts.length === 1 ? this.W / 2 : i * step;
      const y = this.H - (p.revenue / maxRev) * (this.H - 16) - 8;
      return `${i === 0 ? 'M' : 'L'}${x.toFixed(2)},${y.toFixed(2)}`;
    }).join(' ');
  });

  onMove(ev: MouseEvent) {
    const d = this.data();
    if (!d || d.points.length === 0) return;
    const svg = ev.currentTarget as SVGSVGElement;
    const rect = svg.getBoundingClientRect();
    const xRatio = (ev.clientX - rect.left) / rect.width;
    const idx = Math.round(xRatio * (d.points.length - 1));
    this.hoverIdx.set(Math.max(0, Math.min(d.points.length - 1, idx)));
  }

  formatDate(d: string): string {
    return new Date(d).toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
  }
  formatRevenue(v: number): string {
    return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(v);
  }
}
