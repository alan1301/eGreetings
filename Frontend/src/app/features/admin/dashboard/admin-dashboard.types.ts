export interface DashboardOverviewDto {
  sentToday: number;
  sentYesterday: number;
  activeSubscriptions: number;
  pendingPayments: number;
  unreadFeedbacks: number;
  totalUsers: number;
  revenueThisMonth: number;
  revenueLastMonth: number;
  sentSparkline: number[];
  activeSubscriptionsSparkline: number[];
  pendingPaymentsSparkline: number[];
  unreadFeedbacksSparkline: number[];
}

export interface TimeseriesPointDto {
  date: string;
  greetingsSent: number;
  revenue: number;
}

export interface TimeseriesDto {
  range: string;
  points: TimeseriesPointDto[];
}

export interface FunnelDto {
  pending: number;
  active: number;
  expired: number;
  disabled: number;
  conversionRate: number;
}

export interface TopCardDto {
  cardId: string;
  title: string;
  thumbnailUrl: string | null;
  sentCount: number;
  sparkline: number[];
}

export type ActivityType =
  | 'GreetingSent'
  | 'PaymentPending'
  | 'SubscriptionCreated'
  | 'FeedbackSubmitted'
  | 'AdminAction'
  | 'SystemError'
  | string;

export interface ActivityItemDto {
  id: string;
  type: ActivityType;
  title: string;
  subtitle: string | null;
  actorName: string;
  actorInitials: string;
  occurredAt: string;
  status: string;
  targetId: string | null;
  action: 'view' | 'approve' | 'mark-read' | null;
}

export interface HealthDto {
  db: 'ok' | 'down' | string;
  emailQueueDepth: number;
  subscribeJobLastRun: string | null;
  subscribeJobLastStatus: string;
  apiUptimeSeconds: number;
}

export type TimeRange = '7d' | '30d';
