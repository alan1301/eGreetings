import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, tap, map } from 'rxjs';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';
import { ApiResponse, LoginResult, RegisterResult, UserInfo } from '../../shared/models/api-response.model';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);
  private readonly base = environment.apiBaseUrl;

  private currentUserSubject = new BehaviorSubject<UserInfo | null>(this.loadFromStorage());
  currentUser$ = this.currentUserSubject.asObservable();

  private loadFromStorage(): UserInfo | null {
    try {
      const raw = localStorage.getItem('user');
      return raw ? JSON.parse(raw) : null;
    } catch {
      return null;
    }
  }

  getToken(): string | null {
    return localStorage.getItem('token');
  }

  /** BR-05: kiểm tra token tồn tại VÀ chưa hết hạn */
  isLoggedIn(): boolean {
    const token = this.getToken();
    if (!token) return false;
    const expiresAt = localStorage.getItem('expiresAt');
    if (expiresAt && new Date(expiresAt) <= new Date()) {
      // Token hết hạn — dọn dẹp storage
      this.clearStorage();
      return false;
    }
    return true;
  }

  isAdmin(): boolean {
    return this.currentUserSubject.value?.role === 'Admin';
  }

  /** BR-05: rememberMe truyền lên backend để quyết định TTL refresh token */
  login(email: string, password: string, rememberMe = false) {
    return this.http.post<ApiResponse<any>>(`${this.base}/auth/login`, { email, password, rememberMe }).pipe(
      tap(res => {
        if (res.success && res.data) {
          const user: UserInfo = {
            id: res.data.userId,
            fullName: res.data.fullName,
            email: res.data.email,
            role: res.data.role,
            status: 'Active'
          };
          localStorage.setItem('token', res.data.token || res.data.accessToken);
          localStorage.setItem('user', JSON.stringify(user));
          // BR-05: lưu expiresAt để isLoggedIn() kiểm tra hết hạn
          if (res.data.expiresAt) {
            localStorage.setItem('expiresAt', res.data.expiresAt);
          }
          this.currentUserSubject.next(user);
        }
      }),
      map(res => {
        if (!res.data) return res.data;
        return {
          token: res.data.token || res.data.accessToken,
          expiresAt: res.data.expiresAt,
          failedLoginCount: res.data.failedLoginCount ?? 0,
          user: {
            id: res.data.userId,
            fullName: res.data.fullName,
            email: res.data.email,
            role: res.data.role,
            status: 'Active'
          }
        } as LoginResult;
      })
    );
  }

  register(fullName: string, email: string, password: string, confirmPassword: string) {
    return this.http.post<ApiResponse<RegisterResult>>(`${this.base}/auth/register`, {
      fullName, email, password, confirmPassword
    }).pipe(map(res => res.data));
  }

  /** UC22: Gửi email đặt lại mật khẩu */
  forgotPassword(email: string) {
    return this.http.post<ApiResponse<null>>(`${this.base}/auth/forgot-password`, { email });
  }

  /** UC22: Đặt mật khẩu mới bằng token từ email */
  resetPassword(token: string, newPassword: string, confirmPassword: string) {
    return this.http.post<ApiResponse<null>>(`${this.base}/auth/reset-password`, {
      token, newPassword, confirmPassword
    });
  }

  logout() {
    this.http.post(`${this.base}/auth/logout`, {}).subscribe({ error: () => {} });
    this.clearStorage();
    this.currentUserSubject.next(null);
    this.router.navigate(['/login']);
  }

  private clearStorage() {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    localStorage.removeItem('expiresAt');
    localStorage.removeItem('eg_subscription');
  }
}
