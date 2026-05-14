import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Subscription, timer, fromEvent, EMPTY } from 'rxjs';
import { switchMap, catchError, tap, filter } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';
import { ApiResponse } from '../../../shared/models/api-response.model';
import {
  DashboardOverviewDto, TimeseriesDto, FunnelDto, TopCardDto,
  ActivityItemDto, HealthDto, TimeRange
} from './admin-dashboard.types';

const FAST_INTERVAL = 10_000;
const SLOW_INTERVAL = 60_000;

@Injectable({ providedIn: 'root' })
export class AdminDashboardService {
  private http = inject(HttpClient);
  private base = `${environment.apiBaseUrl}/admin/dashboard`;

  readonly overview = signal<DashboardOverviewDto | null>(null);
  readonly timeseries = signal<TimeseriesDto | null>(null);
  readonly funnel = signal<FunnelDto | null>(null);
  readonly topCards = signal<TopCardDto[]>([]);
  readonly activity = signal<ActivityItemDto[]>([]);
  readonly health = signal<HealthDto | null>(null);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly lastUpdated = signal<Date | null>(null);
  readonly range = signal<TimeRange>('7d');

  readonly isHealthy = computed(() => this.health()?.db === 'ok');

  private subs: Subscription[] = [];
  private visibilitySub?: Subscription;

  start(): void {
    this.stop();

    this.subs.push(
      timer(0, FAST_INTERVAL).pipe(
        filter(() => document.visibilityState === 'visible'),
        switchMap(() => this.fetchOverview()),
      ).subscribe()
    );

    this.subs.push(
      timer(0, FAST_INTERVAL).pipe(
        filter(() => document.visibilityState === 'visible'),
        switchMap(() => this.fetchActivity()),
      ).subscribe()
    );

    this.subs.push(
      timer(0, SLOW_INTERVAL).pipe(
        filter(() => document.visibilityState === 'visible'),
        switchMap(() => this.fetchTimeseries()),
      ).subscribe()
    );

    this.subs.push(
      timer(0, SLOW_INTERVAL).pipe(
        filter(() => document.visibilityState === 'visible'),
        switchMap(() => this.fetchFunnel()),
      ).subscribe()
    );

    this.subs.push(
      timer(0, SLOW_INTERVAL).pipe(
        filter(() => document.visibilityState === 'visible'),
        switchMap(() => this.fetchTopCards()),
      ).subscribe()
    );

    this.subs.push(
      timer(0, SLOW_INTERVAL).pipe(
        filter(() => document.visibilityState === 'visible'),
        switchMap(() => this.fetchHealth()),
      ).subscribe()
    );

    this.visibilitySub = fromEvent(document, 'visibilitychange').pipe(
      filter(() => document.visibilityState === 'visible'),
      tap(() => this.refreshNow())
    ).subscribe();
  }

  stop(): void {
    this.subs.forEach(s => s.unsubscribe());
    this.subs = [];
    this.visibilitySub?.unsubscribe();
    this.visibilitySub = undefined;
  }

  setRange(range: TimeRange): void {
    this.range.set(range);
    this.fetchTimeseries().subscribe();
    this.fetchTopCards().subscribe();
  }

  refreshNow(): void {
    this.fetchOverview().subscribe();
    this.fetchActivity().subscribe();
    this.fetchTimeseries().subscribe();
    this.fetchFunnel().subscribe();
    this.fetchTopCards().subscribe();
    this.fetchHealth().subscribe();
  }

  approvePayment(subscriptionId: string) {
    return this.http.post<ApiResponse>(
      `${environment.apiBaseUrl}/admin/subscriptions/${subscriptionId}/activate`, {}
    ).pipe(tap(() => {
      this.activity.update(items => items.filter(i => i.targetId !== subscriptionId));
      this.refreshNow();
    }));
  }

  markFeedbackRead(feedbackId: string) {
    return this.http.patch<ApiResponse>(
      `${environment.apiBaseUrl}/admin/feedbacks/${feedbackId}/read`, {}
    ).pipe(tap(() => {
      this.activity.update(items => items.filter(i => i.targetId !== feedbackId));
      this.refreshNow();
    }));
  }

  private fetchOverview() {
    this.loading.set(true);
    return this.http.get<ApiResponse<DashboardOverviewDto>>(`${this.base}/overview`).pipe(
      tap(r => {
        this.overview.set(r.data);
        this.lastUpdated.set(new Date());
        this.loading.set(false);
        this.error.set(null);
      }),
      catchError(err => {
        this.error.set(err?.error?.message ?? 'Failed to load overview');
        this.loading.set(false);
        return EMPTY;
      })
    );
  }

  private fetchTimeseries() {
    const params = new HttpParams().set('range', this.range());
    return this.http.get<ApiResponse<TimeseriesDto>>(`${this.base}/timeseries`, { params }).pipe(
      tap(r => this.timeseries.set(r.data)),
      catchError(() => EMPTY)
    );
  }

  private fetchFunnel() {
    return this.http.get<ApiResponse<FunnelDto>>(`${this.base}/funnel`).pipe(
      tap(r => this.funnel.set(r.data)),
      catchError(() => EMPTY)
    );
  }

  private fetchTopCards() {
    const params = new HttpParams().set('range', this.range()).set('take', '5');
    return this.http.get<ApiResponse<TopCardDto[]>>(`${this.base}/top-cards`, { params }).pipe(
      tap(r => this.topCards.set(r.data ?? [])),
      catchError(() => EMPTY)
    );
  }

  private fetchActivity() {
    const params = new HttpParams().set('take', '20');
    return this.http.get<ApiResponse<ActivityItemDto[]>>(`${this.base}/activity`, { params }).pipe(
      tap(r => this.activity.set(r.data ?? [])),
      catchError(() => EMPTY)
    );
  }

  private fetchHealth() {
    return this.http.get<ApiResponse<HealthDto>>(`${this.base}/health`).pipe(
      tap(r => this.health.set(r.data)),
      catchError(() => EMPTY)
    );
  }
}
