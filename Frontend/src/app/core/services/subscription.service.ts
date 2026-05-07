import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiResponse } from '../../shared/models/api-response.model';
import { environment } from '../../../environments/environment';

export interface CurrentSubscriptionDto {
  subscriptionId: string;
  status: 'Active' | 'Pending' | 'Expired' | 'Disabled';
  plan: 'monthly' | 'annual';
  startDate?: string;
  expiryDate?: string;
  daysRemaining: number;
  paymentMethod: string;
}

@Injectable({ providedIn: 'root' })
export class SubscriptionService {
  private http = inject(HttpClient);
  private base = environment.apiBaseUrl;

  /** UC19/UC25 – Get current subscription status for the logged-in user */
  getCurrentSubscription(): Observable<ApiResponse<CurrentSubscriptionDto | null>> {
    return this.http.get<ApiResponse<CurrentSubscriptionDto | null>>(
      `${this.base}/subscriptions/current`
    );
  }
}
