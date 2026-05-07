import { Component, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { NavbarComponent } from '../../../shared/components/navbar/navbar.component';
import { FooterComponent } from '../../../shared/components/footer/footer.component';
import { environment } from '../../../../environments/environment';
import { ApiResponse } from '../../../shared/models/api-response.model';

interface CardDto {
  id: string;
  name: string;
  slug: string;
  thumbnailUrl?: string;
  description?: string;
  isFeatured: boolean;
  isPremium: boolean;
  categoryName: string;
  categorySlug: string;
  status: string;
}

interface CategoryDto {
  id: string;
  name: string;
  slug: string;
  iconUrl?: string;
  color?: string;
  cardCount?: number;
}

type CardsResponseData =
  | CardDto[]
  | {
      items?: CardDto[];
    };

@Component({
  selector: 'app-cards',
  standalone: true,
  imports: [CommonModule, NavbarComponent, FooterComponent],
  styleUrl: './card-list.component.css',
  templateUrl: './card-list.component.html'
})
export class CardListComponent {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private http = inject(HttpClient);
  private readonly base = environment.apiBaseUrl;

  categories = signal<CategoryDto[]>([]);
  allCards = signal<CardDto[]>([]);
  loading = signal(true);
  loadError = signal('');
  activeCat = signal('all');

  catLabel = computed(() =>
    this.categories().find(c => c.slug === this.activeCat())?.name ?? 'All'
  );

  filteredCards = computed(() => {
    const activeCategory = this.activeCat().trim().toLowerCase();
    const activeCards = this.allCards().filter(card => card.status === 'Active');

    if (activeCategory === 'all') return activeCards;

    return activeCards.filter(card => card.categorySlug?.trim().toLowerCase() === activeCategory);
  });

  constructor() {
    this.route.queryParamMap.subscribe(params => {
      const cat = params.get('category');
      if (cat) this.activeCat.set(cat);
    });
    this.route.paramMap.subscribe(params => {
      const slug = params.get('slug');
      if (slug) this.activeCat.set(slug);
    });
    this.loadData();
  }

  loadData() {
    this.loading.set(true);
    this.loadError.set('');

    this.http.get<ApiResponse<CategoryDto[]>>(`${this.base}/categories`).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.categories.set(res.data);
          const current = this.activeCat();
          if (current !== 'all' && !res.data.some(c => c.slug === current)) {
            this.activeCat.set('all');
          }
        }
      }
    });

    this.http.get<ApiResponse<CardsResponseData>>(`${this.base}/cards?pageSize=100`).subscribe({
      next: (res) => {
        this.loading.set(false);
        if (!res.success || !res.data) {
          this.allCards.set([]);
          this.loadError.set(res.message || 'Unable to load cards.');
          return;
        }

        const cards = Array.isArray(res.data) ? res.data : (res.data.items ?? []);
        this.allCards.set(cards);
      },
      error: () => {
        this.loading.set(false);
        this.allCards.set([]);
        this.loadError.set('Unable to load cards from the server. Please try again.');
      }
    });
  }

  setCategory(slug: string) {
    this.activeCat.set(slug);
    // Luôn navigate về /cards với queryParam để tránh conflict giữa
    // route param (:slug) và queryParam (category) khi đang ở /categories/:slug
    this.router.navigate(['/cards'], {
      queryParams: slug === 'all' ? {} : { category: slug },
      replaceUrl: true
    });
  }

  viewCard(card: CardDto) {
    this.router.navigate(['/cards', card.id]);
  }
}
