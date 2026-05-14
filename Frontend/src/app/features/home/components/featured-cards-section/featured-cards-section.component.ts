import { ChangeDetectionStrategy, Component, computed, input, model } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { CardDto, getCardBg, getCardProp } from '../../home.types';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-featured-cards-section',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <section class="section feat-section anim-section">
      <div class="sec-center">
        <h2 class="sec-h2">Featured Collections</h2>
        <p class="sec-sub">Discover our curated selection of digital stationery for every occasion.</p>
      </div>

      @if (isLoading()) {
        <div class="feat-carousel">
          <div class="feat-grid">
            @for (i of [1,2,3]; track i) {
              <div class="feat-skeleton"></div>
            }
          </div>
        </div>
      } @else if (cards().length > 0) {
        <div class="feat-carousel">
          <button class="carousel-btn carousel-prev"
                  (click)="slidePrev()" [disabled]="!canSlidePrev()" aria-label="Previous">
            <span class="material-symbols-outlined">chevron_left</span>
          </button>

          <div class="feat-track-clip">
            <div class="feat-track" [style.transform]="trackTranslate()">
              @for (page of pages(); track $index) {
                <div class="feat-page">
                  @for (card of page; track card.id) {
                    <a [routerLink]="['/cards', card.id]" class="feat-card">
                      <div class="feat-img feat-preview">
                        <div class="feat-preview-bg" [style.background]="bg(card)"></div>
                        <div class="feat-preview-overlay">
                          <p class="feat-preview-heading"
                             [style.font-family]="prop(card,'fontFamily')"
                             [style.color]="prop(card,'textColor')">
                            {{ prop(card,'heading') || card.name }}
                          </p>
                          <p class="feat-preview-msg"
                             [style.font-family]="prop(card,'fontFamily')"
                             [style.color]="prop(card,'textColor')">
                            {{ prop(card,'message') }}
                          </p>
                        </div>
                        <div class="feat-preview-badge">{{ card.categoryName }}</div>
                      </div>
                      <div class="feat-body">
                        <div class="feat-title">{{ card.name }}</div>
                        <div class="feat-desc">{{ card.categoryName }}</div>
                      </div>
                    </a>
                  }
                </div>
              }
            </div>
          </div>

          <button class="carousel-btn carousel-next"
                  (click)="slideNext()" [disabled]="!canSlideNext()" aria-label="Next">
            <span class="material-symbols-outlined">chevron_right</span>
          </button>
        </div>
        <div class="feat-dots">
          @for (page of pages(); track $index; let i = $index) {
            <span class="feat-dot" [class.active]="i === pageIndex()"></span>
          }
        </div>
      } @else {
        <div class="feat-grid">
          <a routerLink="/cards" [queryParams]="{category:'sinh-nhat'}" class="feat-card">
            <div class="feat-img"><img src="https://images.unsplash.com/photo-1558636508-e0db3814bd1d?w=600&q=80&auto=format&fit=crop" alt="Birthday"></div>
            <div class="feat-body"><div class="feat-title">Birthday Celebrations</div><div class="feat-desc">Joyful and vibrant designs</div></div>
          </a>
          <a routerLink="/cards" [queryParams]="{category:'dam-cuoi'}" class="feat-card">
            <div class="feat-img"><img src="https://images.unsplash.com/photo-1522673607200-164d1b6ce486?w=600&q=80&auto=format&fit=crop" alt="Wedding"></div>
            <div class="feat-body"><div class="feat-title">Wedding Elegance</div><div class="feat-desc">Timeless and sophisticated</div></div>
          </a>
          <a routerLink="/cards" [queryParams]="{category:'le-hoi'}" class="feat-card">
            <div class="feat-img"><img src="https://images.unsplash.com/photo-1467810563316-b5476525c0f9?w=600&q=80&auto=format&fit=crop" alt="Festival"></div>
            <div class="feat-body"><div class="feat-title">Festive Greetings</div><div class="feat-desc">Warm and celebratory</div></div>
          </a>
        </div>
      }
    </section>
  `
})
export class FeaturedCardsSectionComponent {
  cards = input.required<CardDto[]>();
  isLoading = input<boolean>(false);
  pageIndex = model<number>(0);

  readonly VISIBLE_COUNT = 3;

  pages = computed(() => {
    const out: CardDto[][] = [];
    const arr = this.cards();
    for (let i = 0; i < arr.length; i += this.VISIBLE_COUNT) {
      out.push(arr.slice(i, i + this.VISIBLE_COUNT));
    }
    return out;
  });

  trackTranslate = computed(() => `translateX(-${this.pageIndex() * 100}%)`);
  canSlidePrev = computed(() => this.pageIndex() > 0);
  canSlideNext = computed(() => this.pageIndex() < this.pages().length - 1);

  slidePrev() { if (this.canSlidePrev()) this.pageIndex.set(this.pageIndex() - 1); }
  slideNext() { if (this.canSlideNext()) this.pageIndex.set(this.pageIndex() + 1); }

  bg = getCardBg;
  prop = getCardProp;
}
