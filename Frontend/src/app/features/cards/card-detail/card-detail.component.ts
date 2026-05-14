import { Component, OnInit, computed, inject, signal, ChangeDetectionStrategy, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { NavbarComponent } from '../../../shared/components/navbar/navbar.component';
import { FooterComponent } from '../../../shared/components/footer/footer.component';
import { environment } from '../../../../environments/environment';
import { ApiResponse } from '../../../shared/models/api-response.model';
import { AuthService } from '../../../core/services/auth.service';
import { SubscriptionService } from '../../../core/services/subscription.service';
import { AuthModalService } from '../../../core/services/auth-modal.service';

interface CardDetailDto {
  id: string;
  name: string;
  slug: string;
  thumbnailUrl?: string;
  fileUrl?: string;
  description?: string;
  tags?: string;
  customJsonContent?: string;
  isFeatured: boolean;
  isPremium: boolean;
  categoryName: string;
  categorySlug: string;
}

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-card-detail',
  standalone: true,
  imports: [CommonModule, RouterLink, NavbarComponent, FooterComponent],
  templateUrl: './card-detail.component.html'
})
export class CardDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private http = inject(HttpClient);
  private auth = inject(AuthService);
  private authModal = inject(AuthModalService);
  private subscriptionService = inject(SubscriptionService);
  private destroyRef = inject(DestroyRef);
  private readonly base = environment.apiBaseUrl;

  card = signal<CardDetailDto | null>(null);
  loading = signal(true);
  error = signal('');
  hasSubscription = signal(false);
  premiumNotice = signal(false);

  tags = computed(() =>
    (this.card()?.tags ?? '')
      .split(',')
      .map(tag => tag.trim())
      .filter(Boolean)
  );

  get isLoggedIn(): boolean {
    return this.auth.isLoggedIn();
  }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    this.premiumNotice.set(this.route.snapshot.queryParamMap.get('premium') === 'required');
    if (!id) {
      this.loading.set(false);
      this.error.set('Card not found.');
      return;
    }

    this.loadCard(id);
    this.loadSubscriptionState();
  }

  private loadCard(id: string) {
    this.loading.set(true);
    this.error.set('');

    this.http.get<ApiResponse<CardDetailDto>>(`${this.base}/cards/${id}`).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data) {
          this.card.set(res.data);
          return;
        }

        this.error.set(res.message || 'Unable to load this template.');
      },
      error: () => {
        this.loading.set(false);
        this.error.set('Unable to load this template.');
      }
    });
  }

  private loadSubscriptionState() {
    if (!this.auth.isLoggedIn()) {
      this.hasSubscription.set(false);
      return;
    }

    this.subscriptionService.getCurrentSubscription().pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (res) => {
        if (res.data?.status === 'Active') {
          this.hasSubscription.set(true);
          return;
        }

        this.loadLocalSubscription();
      },
      error: () => this.loadLocalSubscription()
    });
  }

  private loadLocalSubscription() {
    const stored = localStorage.getItem('eg_subscription');
    if (!stored) {
      this.hasSubscription.set(false);
      return;
    }

    try {
      JSON.parse(stored);
      this.hasSubscription.set(true);
    } catch {
      this.hasSubscription.set(false);
    }
  }

  useTemplate() {
    const activeCard = this.card();
    if (!activeCard) return;

    if (!this.isLoggedIn) {
      this.authModal.open('login');
      return;
    }

    if (activeCard.isPremium && !this.hasSubscription()) {
      this.router.navigate(['/subscribe']);
      return;
    }

    this.router.navigate(['/cards', activeCard.id, 'personalize']);
  }
}
