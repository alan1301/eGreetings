import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiResponse, PagedResult } from '../../shared/models/api-response.model';
import { environment } from '../../../environments/environment';

export interface FeedbackDto {
  id: string;
  userEmail: string;
  userFullName: string;
  title: string;
  content: string;
  starRating?: number;
  status: string;
  createdAt: string;
}

@Injectable({
  providedIn: 'root'
})
export class AdminFeedbackService {
  private http = inject(HttpClient);
  private base = environment.apiBaseUrl;

  getFeedbacks(
    page: number = 1,
    pageSize: number = 20,
    status?: string
  ): Observable<ApiResponse<PagedResult<FeedbackDto>>> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    if (status) params = params.set('status', status);

    return this.http.get<ApiResponse<PagedResult<FeedbackDto>>>(`${this.base}/admin/feedbacks`, { params });
  }

  markAsRead(id: string): Observable<ApiResponse> {
    return this.http.patch<ApiResponse>(`${this.base}/admin/feedbacks/${id}/read`, {});
  }
}
