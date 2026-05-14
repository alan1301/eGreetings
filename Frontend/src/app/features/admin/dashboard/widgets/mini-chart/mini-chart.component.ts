import { Component, computed, input, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-mini-chart',
  standalone: true,
  imports: [CommonModule],
  template: `
    <svg [attr.viewBox]="'0 0 ' + width() + ' ' + height()" class="block w-full h-full" preserveAspectRatio="none">
      @if (areaPath()) {
        <path [attr.d]="areaPath()" [attr.fill]="fill()" opacity="0.18" />
      }
      <path [attr.d]="linePath()" fill="none" [attr.stroke]="stroke()" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" />
      @if (showDots()) {
        @for (p of points(); track $index) {
          <circle [attr.cx]="p.x" [attr.cy]="p.y" r="2" [attr.fill]="stroke()" />
        }
      }
    </svg>
  `
})
export class MiniChartComponent {
  data = input.required<number[]>();
  width = input<number>(100);
  height = input<number>(32);
  stroke = input<string>('currentColor');
  fill = input<string>('currentColor');
  showDots = input<boolean>(false);

  points = computed(() => {
    const d = this.data();
    const w = this.width();
    const h = this.height();
    if (!d || d.length === 0) return [];
    const max = Math.max(...d, 1);
    const min = Math.min(...d, 0);
    const range = max - min || 1;
    const stepX = d.length === 1 ? 0 : w / (d.length - 1);
    return d.map((v, i) => ({
      x: d.length === 1 ? w / 2 : i * stepX,
      y: h - ((v - min) / range) * (h - 4) - 2
    }));
  });

  linePath = computed(() => {
    const pts = this.points();
    if (pts.length === 0) return '';
    return pts.map((p, i) => `${i === 0 ? 'M' : 'L'}${p.x.toFixed(2)},${p.y.toFixed(2)}`).join(' ');
  });

  areaPath = computed(() => {
    const pts = this.points();
    const h = this.height();
    if (pts.length < 2) return '';
    const path = pts.map((p, i) => `${i === 0 ? 'M' : 'L'}${p.x.toFixed(2)},${p.y.toFixed(2)}`).join(' ');
    return `${path} L${pts[pts.length - 1].x.toFixed(2)},${h} L${pts[0].x.toFixed(2)},${h} Z`;
  });
}
