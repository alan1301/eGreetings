import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { AdminSidebarComponent } from '../components/admin-sidebar/admin-sidebar.component';
import { environment } from '../../../../environments/environment';

interface DashboardStats {
  totalUsers: number;
  activeSubscriptions: number;
  pendingPayments: number;
  greetingsSentToday: number;
  unreadFeedbacks: number;
  revenueThisMonth: number;
}

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink, AdminSidebarComponent],
  templateUrl: './admin-dashboard.component.html'
})
export class AdminDashboardComponent implements OnInit {
  private http = inject(HttpClient);

  today = new Date().toLocaleDateString('en-US', { weekday: 'long', day: '2-digit', month: '2-digit', year: 'numeric' });

  chartBars = [
    { height: 40, highlight: false }, { height: 65, highlight: false },
    { height: 50, highlight: false }, { height: 90, highlight: true },
    { height: 55, highlight: false }, { height: 75, highlight: false }, { height: 60, highlight: false }
  ];

  stats = signal([
    { icon: 'mail', label: 'Sent Today', value: '—', iconColor: 'text-primary' },
    { icon: 'group', label: 'Total Users', value: '—', iconColor: 'text-secondary' },
    { icon: 'forum', label: 'New Feedback', value: '—', iconColor: 'text-tertiary' },
    { icon: 'pending_actions', label: 'Pending Payments', value: '—', iconColor: 'text-on-error-container' },
  ]);

  transactions = [
    { id: 1, initials: 'NV', name: 'Nguyễn Văn A', type: 'Birthday Card', time: 'Today 14:30', status: 'Success', avatarClass: 'bg-tertiary-container text-on-tertiary-container' },
    { id: 2, initials: 'TH', name: 'Trần Thị B', type: 'Wedding Card', time: 'Today 12:15', status: 'Success', avatarClass: 'bg-secondary-container text-on-secondary-container' },
    { id: 3, initials: 'LP', name: 'Lê Phương C', type: 'Subscription', time: 'Today 09:00', status: 'Failed', avatarClass: 'bg-surface-container-highest text-on-surface' },
    { id: 4, initials: 'HM', name: 'Hoàng Minh D', type: 'New Year Card', time: 'Yesterday 20:45', status: 'Success', avatarClass: 'bg-primary-container text-on-primary-container' },
  ];

  ngOnInit(): void {
    this.http.get<DashboardStats>(`${environment.apiBaseUrl}/admin/dashboard`).subscribe({
      next: (data) => {
        this.stats.set([
          { icon: 'mail', label: 'Sent Today', value: data.greetingsSentToday.toLocaleString(), iconColor: 'text-primary' },
          { icon: 'group', label: 'Total Users', value: data.totalUsers.toLocaleString(), iconColor: 'text-secondary' },
          { icon: 'forum', label: 'New Feedback', value: data.unreadFeedbacks.toLocaleString(), iconColor: 'text-tertiary' },
          { icon: 'pending_actions', label: 'Pending Payments', value: data.pendingPayments.toLocaleString(), iconColor: 'text-on-error-container' },
        ]);
      }
    });
  }
}
