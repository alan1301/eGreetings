import { Component, inject, OnInit, signal, ChangeDetectionStrategy, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { RouterLink, ActivatedRoute } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { NavbarComponent } from '../../../shared/components/navbar/navbar.component';
import { FooterComponent } from '../../../shared/components/footer/footer.component';
import { AuthService } from '../../../core/services/auth.service';
import { environment } from '../../../../environments/environment';

interface HistoryItem {
  id: string;
  cardName: string;
  thumbnailUrl?: string;
  recipientEmail: string;
  sentAt?: string;
  status: string;
}

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-history',
  standalone: true,
  imports: [CommonModule, RouterLink, NavbarComponent, FooterComponent],
  styleUrl: './history.component.css',
  templateUrl: './history.component.html'
})
export class HistoryComponent implements OnInit {
  private http = inject(HttpClient);
  private auth = inject(AuthService);
  private route = inject(ActivatedRoute);
  private destroyRef = inject(DestroyRef);
  private readonly base = environment.apiBaseUrl;

  items       = signal<HistoryItem[]>([]);
  loading     = signal(true);
  page        = signal(1);
  totalPages  = signal(1);
  activeFilter = signal('');

  filters = [
    { label: 'All',       value: '' },
    { label: 'Sent',      value: 'Sent' },
    { label: 'Scheduled', value: 'Scheduled' },
    { label: 'Failed',    value: 'Failed' },
  ];

  ngOnInit(): void {
    this.route.queryParamMap.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(params => {
      const status = params.get('status');
      if (status && this.filters.some(f => f.value === status)) {
        this.activeFilter.set(status);
      }
      this.loadHistory();
    });
  }

  loadHistory(): void {
    this.loading.set(true);
    const token = this.auth.getToken();
    if (!token) { this.loading.set(false); return; }

    const params: any = { page: this.page(), pageSize: 10 };
    if (this.activeFilter()) params['status'] = this.activeFilter();

    const query = Object.entries(params).map(([k,v]) => `${k}=${v}`).join('&');
    this.http.get<any>(`${this.base}/me/greeting-history?${query}`, {
      headers: { Authorization: `Bearer ${token}` }
    }).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: res => {
        this.loading.set(false);
        if (res.success && res.data) {
          this.items.set(res.data.items ?? []);
          const meta = res.meta ?? res.data.meta ?? {};
          this.totalPages.set(meta.totalPages ?? 1);
        }
      },
      error: () => { this.loading.set(false); this.items.set([]); }
    });
  }

  prevPage(): void { if (this.page() > 1) { this.page.update(p => p - 1); this.loadHistory(); } }
  nextPage(): void { if (this.page() < this.totalPages()) { this.page.update(p => p + 1); this.loadHistory(); } }

  formatDate(dateStr: string): string {
    return new Date(dateStr).toLocaleDateString('en-US', {
      year: 'numeric', month: 'short', day: 'numeric',
      hour: '2-digit', minute: '2-digit'
    });
  }
}
