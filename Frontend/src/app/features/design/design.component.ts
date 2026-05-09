import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, ActivatedRoute, Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { NavbarComponent } from '../../shared/components/navbar/navbar.component';
import { FooterComponent } from '../../shared/components/footer/footer.component';
import { AuthService } from '../../core/services/auth.service';
import { environment } from '../../../environments/environment';

interface UpcomingEvent { name: string; occasionLabel: string; occasionDate: string; daysLeft: number; }
interface Draft { id: string; cardId: string; thumbnailUrl?: string; cardName?: string; }

@Component({
  selector: 'app-design',
  standalone: true,
  imports: [CommonModule, RouterLink, NavbarComponent, FooterComponent],
  styleUrl: './design.component.css',
  templateUrl: './design.component.html'
})
export class DesignComponent implements OnInit {
  private auth   = inject(AuthService);
  private http   = inject(HttpClient);
  private route  = inject(ActivatedRoute);
  private router = inject(Router);
  private readonly base = environment.apiBaseUrl;

  currentUser$   = this.auth.currentUser$;
  drafts         = signal<Draft[]>([]);
  upcomingEvents = signal<UpcomingEvent[]>([]);
  sentSuccess    = signal(false);
  sentTo         = signal('');

  // Gift notification popup
  giftMessage    = signal<string | null>(null);
  giftVisible    = signal(false);

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      if (params['sent'] === 'true') {
        this.sentSuccess.set(true);
        this.sentTo.set(params['to'] ?? 'your recipient');
      }
    });
    this.auth.currentUser$.subscribe(user => {
      if (user) {
        this.loadDashboardData();
        this.checkGiftNotification();
      }
    });
  }

  dismissSuccess(): void {
    this.sentSuccess.set(false);
    this.router.navigate([], { queryParams: {}, replaceUrl: true });
  }

  dismissGift(): void {
    this.giftVisible.set(false);
    setTimeout(() => this.giftMessage.set(null), 400); // wait for fade-out
  }

  private checkGiftNotification(): void {
    const token = this.auth.getToken();
    const headers = { Authorization: `Bearer ${token}` };
    this.http.get<any>(`${this.base}/me/gift-notification`, { headers }).subscribe({
      next: res => {
        if (res?.data?.message) {
          this.giftMessage.set(res.data.message);
          // Small delay so page loads first, then popup appears
          setTimeout(() => this.giftVisible.set(true), 800);
        }
      },
      error: () => {}
    });
  }

  private loadDashboardData(): void {
    const token = this.auth.getToken();
    const headers = { Authorization: `Bearer ${token}` };

    this.http.get<any>(`${this.base}/me/drafts?pageSize=4`, { headers }).subscribe({
      next: res => {
        if (res.success && res.data) {
          const items = res.data.items ?? (Array.isArray(res.data) ? res.data : []);
          this.drafts.set(items.slice(0, 4).map((d: any) => ({
            id:           d.id,
            cardId:       d.cardId,
            thumbnailUrl: d.thumbnailUrl ?? null,
            cardName:     d.cardName ?? 'Draft',
          })));
        }
      },
      error: () => {}
    });

    this.http.get<any>(`${this.base}/contacts/upcoming`, { headers }).subscribe({
      next: res => {
        if (res.success && res.data) this.upcomingEvents.set(res.data.slice(0, 4));
      },
      error: () => {}
    });
  }

  firstName(fullName: string): string {
    return fullName?.split(' ')[0] ?? fullName;
  }

  formatDate(dateStr: string): string {
    if (!dateStr) return '';
    return new Date(dateStr).toLocaleDateString('en-US', { month: 'long', day: 'numeric' });
  }

  onImgError(e: Event): void {
    const img = e.target as HTMLImageElement;
    img.style.display = 'none';
    const parent = img.closest('.archive-banner') as HTMLElement;
    if (parent) parent.style.background = 'linear-gradient(135deg, #C9A96E, #8B6914)';
  }
}
