import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiResponse } from '../../shared/models/api-response.model';
import { environment } from '../../../environments/environment';

export interface SubmitFeedbackPayload {
  title: string;
  content: string;
  starRating?: number;
}

@Injectable({ providedIn: 'root' })
export class FeedbackService {
  private http = inject(HttpClient);
  private base = environment.apiBaseUrl;

  submitFeedback(payload: SubmitFeedbackPayload): Observable<ApiResponse> {
    return this.http.post<ApiResponse>(`${this.base}/feedbacks`, payload);
  }
}
