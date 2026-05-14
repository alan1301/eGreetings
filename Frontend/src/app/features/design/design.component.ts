import { Component, inject, signal, OnInit, ChangeDetectionStrategy, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { RouterLink, ActivatedRoute, Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { NavbarComponent } from '../../shared/components/navbar/navbar.component';
import { FooterComponent } from '../../shared/components/footer/footer.component';
import { AuthService } from '../../core/services/auth.service';
import { environment } from '../../../environments/environment';

interface UpcomingEventItem {
  type: 'ScheduledGreeting' | 'ContactOccasion';
  id: string;
  title: string;
  subtitle: string;
  eventDate: string;
  daysLeft: number;
  thumbnailUrl?: string;
  cardName?: string;
  recipientEmail?: string;
}
interface Draft { id: string; cardId: string; thumbnailUrl?: string; cardName?: string; }

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
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
  private destroyRef = inject(DestroyRef);
  private readonly base = environment.apiBaseUrl;

  currentUser$   = this.auth.currentUser$;
  drafts         = signal<Draft[]>([]);
  upcomingEvents = signal<UpcomingEventItem[]>([]);
  sentSuccess    = signal(false);
  sentTo         = signal('');
  cancellingId   = signal<string | null>(null);

  // Gift notification popup
  giftMessage    = signal<string | null>(null);
  giftVisible    = signal(false);

  ngOnInit(): void {
    this.route.queryParams.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(params => {
      if (params['sent'] === 'true') {
        this.sentSuccess.set(true);
        this.sentTo.set(params['to'] ?? 'your recipient');
      }
    });
    this.auth.currentUser$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(user => {
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
    this.http.get<any>(`${this.base}/me/gift-notification`, { headers }).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
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

    this.http.get<any>(`${this.base}/me/drafts?pageSize=4`, { headers }).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
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

    this.http.get<any>(`${this.base}/me/upcoming-events`, { headers }).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: res => {
        if (res.success && Array.isArray(res.data)) {
          this.upcomingEvents.set(res.data.map((e: any) => ({
            type:          e.type,
            id:            e.id,
            title:         e.title,
            subtitle:      e.subtitle,
            eventDate:     e.eventDate,
            daysLeft:      e.daysLeft,
            thumbnailUrl:  e.thumbnailUrl ?? null,
            cardName:      e.cardName ?? null,
            recipientEmail: e.recipientEmail ?? null,
          })));
        }
      },
      error: () => {}
    });
  }

  cancelScheduled(id: string): void {
    if (this.cancellingId()) return;
    this.cancellingId.set(id);
    const token = this.auth.getToken();
    const headers = { Authorization: `Bearer ${token}` };
    this.http.delete<any>(`${this.base}/greetings/${id}/schedule`, { headers }).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        this.upcomingEvents.update(list => list.filter(e => e.id !== id));
        this.cancellingId.set(null);
      },
      error: () => this.cancellingId.set(null)
    });
  }

  sendCardForOccasion(ev: UpcomingEventItem): void {
    // Navigate to personalize (fresh mode) with pre-filled recipient + scheduled date
    const scheduledAt = new Date(ev.eventDate).toISOString().slice(0, 16); // datetime-local format
    this.router.navigate(['/send'], {
      queryParams: {
        mode: 'fresh',
        recipientEmail:  ev.recipientEmail ?? ev.subtitle,
        scheduledAt:     scheduledAt,
        scheduleEnabled: 'true'
      }
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
