import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiResponse, PagedResult } from '../../shared/models/api-response.model';
import { environment } from '../../../environments/environment';

export interface AdminSubscriptionDto {
  id: string;
  userId: string;
  userEmail: string;
  userFullName: string;
  paymentMethod: string;
  status: string;
  createdAt: string;
  startDate?: string;
  expiryDate?: string;
  disabledReason?: string;
}

@Injectable({
  providedIn: 'root'
})
export class AdminSubscriptionsService {
  private http = inject(HttpClient);
  private base = environment.apiBaseUrl;

  getSubscriptions(
    page: number = 1,
    pageSize: number = 20,
    status?: string
  ): Observable<ApiResponse<PagedResult<AdminSubscriptionDto>>> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    if (status) params = params.set('status', status);

    return this.http.get<ApiResponse<PagedResult<AdminSubscriptionDto>>>(`${this.base}/admin/subscriptions`, { params });
  }

  activateSubscription(id: string): Observable<ApiResponse> {
    return this.http.post<ApiResponse>(`${this.base}/admin/subscriptions/${id}/activate`, {});
  }

  disableSubscription(id: string, reason: string): Observable<ApiResponse> {
    return this.http.post<ApiResponse>(`${this.base}/admin/subscriptions/${id}/disable`, { reason });
  }
}
