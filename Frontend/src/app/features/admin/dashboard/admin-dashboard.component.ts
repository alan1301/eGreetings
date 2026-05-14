import { Component, inject, OnInit, OnDestroy, signal, computed, ChangeDetectionStrategy, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { AdminSidebarComponent } from '../components/admin-sidebar/admin-sidebar.component';
import { AdminDashboardService } from './admin-dashboard.service';
import { ActivityItemDto, TimeRange } from './admin-dashboard.types';
import { StatCardComponent } from './widgets/stat-card/stat-card.component';
import { TrendChartComponent } from './widgets/trend-chart/trend-chart.component';
import { FunnelWidgetComponent } from './widgets/funnel-widget/funnel-widget.component';
import { ActivityFeedComponent } from './widgets/activity-feed/activity-feed.component';
import { TopCardsWidgetComponent } from './widgets/top-cards-widget/top-cards-widget.component';
import { QuickActionsComponent } from './widgets/quick-actions/quick-actions.component';
import { HealthWidgetComponent } from './widgets/health-widget/health-widget.component';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [
    CommonModule, AdminSidebarComponent,
    StatCardComponent, TrendChartComponent, FunnelWidgetComponent,
    ActivityFeedComponent, TopCardsWidgetComponent, QuickActionsComponent, HealthWidgetComponent
  ],
  templateUrl: './admin-dashboard.component.html'
})
export class AdminDashboardComponent implements OnInit, OnDestroy {
  protected dashboard = inject(AdminDashboardService);
  private destroyRef = inject(DestroyRef);

  today = new Date().toLocaleDateString('en-US', {
    weekday: 'long', day: '2-digit', month: '2-digit', year: 'numeric'
  });

  private now = signal(Date.now());
  private tickHandle?: number;

  lastUpdatedLabel = computed(() => {
    const ts = this.dashboard.lastUpdated();
    if (!ts) return 'Never';
    const _ = this.now();
    const diff = Math.max(0, Math.floor((Date.now() - ts.getTime()) / 1000));
    if (diff < 5) return 'Just now';
    if (diff < 60) return `${diff}s ago`;
    if (diff < 3600) return `${Math.floor(diff / 60)}m ago`;
    return `${Math.floor(diff / 3600)}h ago`;
  });

  ngOnInit(): void {
    this.dashboard.start();
    this.tickHandle = window.setInterval(() => this.now.set(Date.now()), 1000);
  }

  ngOnDestroy(): void {
    this.dashboard.stop();
    if (this.tickHandle) window.clearInterval(this.tickHandle);
  }

  onRangeChange(range: TimeRange) {
    this.dashboard.setRange(range);
  }

  onActivityAction(ev: { item: ActivityItemDto; action: string }) {
    if (!ev.item.targetId) return;
    if (ev.action === 'approve') {
      this.dashboard.approvePayment(ev.item.targetId).pipe(takeUntilDestroyed(this.destroyRef)).subscribe();
    } else if (ev.action === 'mark-read') {
      this.dashboard.markFeedbackRead(ev.item.targetId).pipe(takeUntilDestroyed(this.destroyRef)).subscribe();
    }
  }

  refresh() {
    this.dashboard.refreshNow();
  }
}
