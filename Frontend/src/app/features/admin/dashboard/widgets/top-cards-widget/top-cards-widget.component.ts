import { Component, input, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MiniChartComponent } from '../mini-chart/mini-chart.component';
import { TopCardDto } from '../../admin-dashboard.types';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-top-cards-widget',
  standalone: true,
  imports: [CommonModule, RouterLink, MiniChartComponent],
  template: `
    <div class="bg-surface-container-lowest rounded-xl p-6 h-full flex flex-col">
      <div class="flex items-start justify-between mb-4">
        <div>
          <h3 class="text-xl font-headline text-on-surface">Top Cards</h3>
          <p class="text-xs text-on-surface-variant">Most sent this period</p>
        </div>
        <span class="material-symbols-outlined opacity-60">trending_up</span>
      </div>
      @if (items().length === 0) {
        <div class="flex-1 flex items-center justify-center text-sm text-on-surface-variant">No data</div>
      } @else {
        <ul class="space-y-3">
          @for (c of items(); track c.cardId; let i = $index) {
            <li>
              <a [routerLink]="['/admin/cards', c.cardId]"
                 class="flex items-center gap-3 p-2 -mx-2 rounded-lg hover:bg-surface-container-low transition-colors">
                <span class="text-xs font-bold w-5 text-on-surface-variant">#{{ i + 1 }}</span>
                @if (c.thumbnailUrl) {
                  <img [src]="c.thumbnailUrl" alt="" class="w-10 h-10 rounded-lg object-cover" />
                } @else {
                  <div class="w-10 h-10 rounded-lg bg-surface-container-high flex items-center justify-center">
                    <span class="material-symbols-outlined text-on-surface-variant text-base">image</span>
                  </div>
                }
                <div class="flex-1 min-w-0">
                  <div class="text-sm font-medium text-on-surface truncate">{{ c.title }}</div>
                  <div class="text-xs text-on-surface-variant">{{ c.sentCount }} sent</div>
                </div>
                <div class="w-16 h-8 text-primary">
                  <app-mini-chart [data]="c.sparkline" />
                </div>
              </a>
            </li>
          }
        </ul>
      }
    </div>
  `
})
export class TopCardsWidgetComponent {
  items = input<TopCardDto[]>([]);
}
