import { Component, AfterViewInit, OnDestroy, OnInit, ElementRef, Inject, PLATFORM_ID, ChangeDetectionStrategy, ViewEncapsulation, inject, signal, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { isPlatformBrowser } from '@angular/common';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { NavbarComponent } from '../../shared/components/navbar/navbar.component';
import { FooterComponent } from '../../shared/components/footer/footer.component';
import { HeroSectionComponent } from './components/hero-section/hero-section.component';
import { FeaturedCardsSectionComponent } from './components/featured-cards-section/featured-cards-section.component';
import { TrendingCardsSectionComponent } from './components/trending-cards-section/trending-cards-section.component';
import { environment } from '../../../environments/environment';
import { CardDto } from './home.types';

interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
  meta?: { page: number; pageSize: number; total: number; totalPages: number } | null;
}

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-home',
  standalone: true,
  imports: [
    CommonModule, RouterLink,
    NavbarComponent, FooterComponent,
    HeroSectionComponent, FeaturedCardsSectionComponent, TrendingCardsSectionComponent
  ],
  styleUrl: './home.component.css',
  templateUrl: './home.component.html',
  encapsulation: ViewEncapsulation.None
})
export class HomeComponent implements OnInit, AfterViewInit, OnDestroy {
  private observer: IntersectionObserver | null = null;
  private isBrowser: boolean;
  private el = inject(ElementRef);
  private http = inject(HttpClient);
  private destroyRef = inject(DestroyRef);

  featuredCards = signal<CardDto[]>([]);
  trendingCards = signal<CardDto[]>([]);
  isLoadingFeatured = signal(true);
  isLoadingTrending = signal(true);

  constructor(@Inject(PLATFORM_ID) platformId: Object) {
    this.isBrowser = isPlatformBrowser(platformId);
  }

  ngOnInit(): void {
    this.loadFeaturedCards();
    this.loadTrendingCards();
  }

  private parseCards(data: any): CardDto[] {
    const list: CardDto[] = Array.isArray(data) ? data : (data?.items ?? []);
    return list.filter((c: CardDto) => c.status === 'Active');
  }

  private loadFeaturedCards(): void {
    this.http.get<ApiResponse<any>>(
      `${environment.apiBaseUrl}/cards?featured=true&pageSize=18`
    ).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (res) => {
        if (res.success) {
          const cards = this.parseCards(res.data);
          if (cards.length) this.featuredCards.set(cards);
        }
        this.isLoadingFeatured.set(false);
      },
      error: () => this.isLoadingFeatured.set(false)
    });
  }

  private loadTrendingCards(): void {
    this.http.get<ApiResponse<any>>(
      `${environment.apiBaseUrl}/cards?sort=popular&pageSize=4`
    ).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (res) => {
        if (res.success) {
          const cards = this.parseCards(res.data);
          if (cards.length) this.trendingCards.set(cards);
        }
        this.isLoadingTrending.set(false);
      },
      error: () => this.isLoadingTrending.set(false)
    });
  }

  ngAfterViewInit(): void {
    if (!this.isBrowser) return;
    this.observer = new IntersectionObserver((entries) => {
      entries.forEach((entry) => {
        if (entry.isIntersecting) {
          entry.target.classList.add('is-visible');
          entry.target.classList.remove('is-leaving');
        } else {
          // Section left the viewport — mark as leaving so it re-animates on scroll back
          if (entry.target.classList.contains('is-visible')) {
            entry.target.classList.remove('is-visible');
            entry.target.classList.add('is-leaving');
          }
        }
      });
    }, { threshold: 0.15, rootMargin: '0px 0px -18% 0px' });

    const targets = this.el.nativeElement.querySelectorAll('.anim-section');
    targets.forEach((t: Element) => this.observer!.observe(t));

    // Safety net: reveal sections already within (or above) the initial viewport
    // so the hero/first sections do not start hidden on page load.
    setTimeout(() => {
      const vh = window.innerHeight;
      targets.forEach((t: Element) => {
        if (t.classList.contains('is-visible')) return;
        const rect = (t as HTMLElement).getBoundingClientRect();
        if (rect.top < vh * 0.85) t.classList.add('is-visible');
      });
    }, 1200);
  }

  ngOnDestroy(): void {
    this.observer?.disconnect();
  }
}
