import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { CardDto, getCardBg, getCardProp } from '../../home.types';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-trending-cards-section',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <section class="section trend-section anim-section">
      <div class="trend-header">
        <div>
          <h2 class="trend-h2">Trending Now</h2>
          <p class="trend-sub">What our members are sending today.</p>
        </div>
        <a routerLink="/cards" [queryParams]="{sort:'popular'}" class="view-all">
          View All <span class="material-symbols-outlined" style="font-size:18px;">arrow_forward</span>
        </a>
      </div>

      @if (isLoading()) {
        <div class="trend-grid">
          @for (i of [1,2,3,4]; track i) {
            <div class="trend-skeleton"></div>
          }
        </div>
      } @else if (cards().length > 0) {
        <div class="trend-grid">
          @for (card of cards(); track card.id; let i = $index) {
            <a [routerLink]="['/cards', card.id]" class="trend-card">
              <div class="trend-rank">#{{ i + 1 }}</div>
              <div class="trend-preview">
                <div class="trend-preview-bg" [style.background]="bg(card)"></div>
                <div class="trend-preview-overlay">
                  <p class="trend-preview-heading"
                     [style.font-family]="prop(card,'fontFamily')"
                     [style.color]="prop(card,'textColor')">
                    {{ prop(card,'heading') || card.name }}
                  </p>
                </div>
              </div>
              <div class="trend-body">
                <div class="trend-title">{{ card.name }}</div>
                <div class="trend-cat">{{ card.categoryName }}</div>
              </div>
            </a>
          }
        </div>
      } @else {
        <div class="trend-grid">
          <a routerLink="/cards" [queryParams]="{category:'sinh-nhat'}" class="trend-card">
            <img src="https://images.unsplash.com/photo-1607344645866-009c320b63e0?w=400&q=80&auto=format&fit=crop" alt="Botanical">
            <div class="trend-body"><div class="trend-title">Minimalist Botanical</div><div class="trend-cat">Thank You</div></div>
          </a>
          <a routerLink="/cards" [queryParams]="{category:'dam-cuoi'}" class="trend-card">
            <img src="https://images.unsplash.com/photo-1506905925346-21bda4d32df4?w=400&q=80&auto=format&fit=crop" alt="Gold Foil">
            <div class="trend-body"><div class="trend-title">Gold Foil Monogram</div><div class="trend-cat">Just Because</div></div>
          </a>
          <a routerLink="/cards" [queryParams]="{category:'sinh-nhat'}" class="trend-card">
            <img src="https://images.unsplash.com/photo-1487530811015-780df5f7e7b9?w=400&q=80&auto=format&fit=crop" alt="Watercolor">
            <div class="trend-body"><div class="trend-title">Watercolor Blooms</div><div class="trend-cat">Birthday</div></div>
          </a>
          <a routerLink="/cards" [queryParams]="{category:'dam-cuoi'}" class="trend-card">
            <img src="https://images.unsplash.com/photo-1528360983277-13d401cdc186?w=400&q=80&auto=format&fit=crop" alt="Letterpress">
            <div class="trend-body"><div class="trend-title">Classic Ivory Letterpress</div><div class="trend-cat">Anniversary</div></div>
          </a>
        </div>
      }
    </section>
  `
})
export class TrendingCardsSectionComponent {
  cards = input.required<CardDto[]>();
  isLoading = input<boolean>(false);

  bg = getCardBg;
  prop = getCardProp;
}
