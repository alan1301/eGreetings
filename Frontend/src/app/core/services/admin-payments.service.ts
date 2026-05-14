import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiResponse, PagedResult } from '../../shared/models/api-response.model';
import { environment } from '../../../environments/environment';

export interface PaymentTransactionAdminDto {
  id: string;
  subscriptionId: string;
  userEmail: string;
  userFullName: string;
  plan: string;
  amount: number;
  currency: string;
  paymentMethod: string;
  gatewayProvider: string | null;
  gatewayTransactionId: string | null;
  status: string;
  paidAt: string | null;
  createdAt: string;
}

@Injectable({ providedIn: 'root' })
export class AdminPaymentsService {
  private http = inject(HttpClient);
  private base = environment.apiBaseUrl;

  getPayments(
    page = 1,
    pageSize = 20,
    status?: string,
    search?: string
  ): Observable<ApiResponse<PagedResult<PaymentTransactionAdminDto>>> {
    let params = new HttpParams()
      .set('page', page)
      .set('pageSize', pageSize);
    if (status) params = params.set('status', status);
    if (search) params = params.set('search', search);
    return this.http.get<ApiResponse<PagedResult<PaymentTransactionAdminDto>>>(
      `${this.base}/admin/payments`, { params }
    );
  }
}
