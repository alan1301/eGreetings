export interface ApiResponse<T = any> {
  success: boolean;
  message: string;
  data: T;
  errors: { field: string; message: string }[] | null;
  meta?: {
    page: number;
    pageSize: number;
    total: number;
    totalPages: number;
  } | null;
}

export interface PaginationMeta {
  page: number;
  pageSize: number;
  total: number;
  totalPages: number;
}

export interface PagedResult<T> {
  items: T[];
  meta: PaginationMeta;
}

export interface LoginResult {
  token: string;
  refreshToken?: string;
  expiresAt?: string;       // ISO string — dùng để check expiry (BR-05)
  failedLoginCount?: number; // dùng cho CAPTCHA trigger >= 3 (BR-04)
  user: UserInfo;
}

export interface UserInfo {
  id: string;
  fullName: string;
  email: string;
  role: 'User' | 'Admin';
  status: string;
  createdAt?: string; // ISO string – used for "Member since" display
}

export interface RegisterResult {
  userId: string;
  email: string;
}
