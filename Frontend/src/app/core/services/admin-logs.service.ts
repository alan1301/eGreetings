import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiResponse, PagedResult } from '../../shared/models/api-response.model';
import { environment } from '../../../environments/environment';

export interface SystemLogDto {
  id: string;
  timestamp: string;
  actorId?: string;
  actorType: string;
  eventType: string;
  description: string;
  status: string;
  ipAddress?: string;
}

@Injectable({
  providedIn: 'root'
})
export class AdminLogsService {
  private http = inject(HttpClient);
  private base = environment.apiBaseUrl;

  getLogs(
    page: number = 1,
    pageSize: number = 50,
    eventType?: string,
    status?: string,
    from?: string,
    to?: string
  ): Observable<ApiResponse<PagedResult<SystemLogDto>>> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    if (eventType) params = params.set('eventType', eventType);
    if (status) params = params.set('status', status);
    if (from) params = params.set('from', from);
    if (to) params = params.set('to', to);

    // Using /api/admin/logs as per existing controller route.
    return this.http.get<ApiResponse<PagedResult<SystemLogDto>>>(`${this.base}/admin/logs`, { params });
  }
}
