import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiResponse, PagedResult } from '../../shared/models/api-response.model';
import { environment } from '../../../environments/environment';

export type SubscriptionPlan = 'Free' | 'Monthly' | 'Annual';

export interface AdminUserDto {
  id: string;
  email: string;
  fullName: string;
  role: string;
  status: string;
  isDeleted: boolean;
  failedLoginCount: number;
  lastLoginAt?: string;
  lockoutEndTime?: string;
  createdAt: string;
  subscriptionStatus?: string;
  subscriptionPlan?: SubscriptionPlan;
  subscriptionExpiry?: string;
  subscriptionId?: string;
  totalCardsSent?: number;
}

@Injectable({
  providedIn: 'root'
})
export class AdminUsersService {
  private http = inject(HttpClient);
  private base = environment.apiBaseUrl;

  getUsers(
    page: number = 1,
    pageSize: number = 20,
    search?: string,
    subscriptionStatus?: string
  ): Observable<ApiResponse<PagedResult<AdminUserDto>>> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    if (search) params = params.set('search', search);
    if (subscriptionStatus) params = params.set('subscriptionStatus', subscriptionStatus);

    return this.http.get<ApiResponse<PagedResult<AdminUserDto>>>(`${this.base}/admin/users`, { params });
  }

  lockUser(id: string, reason: string): Observable<ApiResponse> {
    return this.http.post<ApiResponse>(`${this.base}/admin/users/${id}/lock`, { reason });
  }

  unlockUser(id: string): Observable<ApiResponse> {
    return this.http.post<ApiResponse>(`${this.base}/admin/users/${id}/unlock`, {});
  }

  activateSubscription(subscriptionId: string): Observable<ApiResponse> {
    return this.http.post<ApiResponse>(`${this.base}/admin/subscriptions/${subscriptionId}/activate`, {});
  }

  disableSubscription(subscriptionId: string, reason: string): Observable<ApiResponse> {
    return this.http.post<ApiResponse>(`${this.base}/admin/subscriptions/${subscriptionId}/disable`, { reason });
  }

  grantSubscription(userId: string, plan: SubscriptionPlan, expiryDate: string, notes?: string): Observable<ApiResponse> {
    return this.http.post<ApiResponse>(`${this.base}/admin/users/${userId}/subscriptions`, {
      plan,
      expiryDate,
      notes
    });
  }
}
