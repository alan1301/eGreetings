import { Routes } from '@angular/router';
import { authGuard, adminGuard, guestGuard, nonAdminGuard } from './core/guards/guards';

export const routes: Routes = [
  // ── Public ──────────────────────────────────────────
  {
    path: '',
    canActivate: [nonAdminGuard],
    loadComponent: () => import('./features/home/home.component').then(m => m.HomeComponent)
  },
  {
    path: 'design',
    canActivate: [nonAdminGuard],
    loadComponent: () => import('./features/design/design.component').then(m => m.DesignComponent)
  },
  {
    path: 'cards',
    canActivate: [nonAdminGuard],
    loadComponent: () => import('./features/cards/card-list/card-list.component').then(m => m.CardListComponent)
  },
  {
    path: 'cards/:id',
    canActivate: [nonAdminGuard],
    loadComponent: () => import('./features/cards/card-detail/card-detail.component').then(m => m.CardDetailComponent)
  },
  {
    // /categories/birthday  → /cards với category=birthday được đọc từ route param
    path: 'categories/:slug',
    canActivate: [nonAdminGuard],
    loadComponent: () => import('./features/cards/card-list/card-list.component').then(m => m.CardListComponent)
  },
  {
    path: 'subscribe',
    canActivate: [nonAdminGuard],
    loadComponent: () => import('./features/subscribe/subscribe.component').then(m => m.SubscribeComponent)
  },


  // ── Auth (chỉ dành cho guest) ────────────────────────
  {
    path: 'login',
    canActivate: [guestGuard],
    loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'register',
    canActivate: [guestGuard],
    loadComponent: () => import('./features/auth/register/register.component').then(m => m.RegisterComponent)
  },
  {
    // UC22: Quên mật khẩu — không cần guard (accessible với guest và user)
    path: 'forgot-password',
    loadComponent: () => import('./features/auth/forgot-password/forgot-password.component').then(m => m.ForgotPasswordComponent)
  },
  {
    // UC22: Đặt lại mật khẩu — nhận token qua query param (?token=...)
    path: 'reset-password',
    loadComponent: () => import('./features/auth/reset-password/reset-password.component').then(m => m.ResetPasswordComponent)
  },

  // ── Protected user routes ────────────────────────────
  {
    path: 'profile',
    canActivate: [authGuard],
    loadComponent: () => import('./features/profile/profile.component').then(m => m.ProfileComponent)
  },
  {
    path: 'cards/:id/personalize',
    canActivate: [authGuard],
    loadComponent: () => import('./features/cards/personalize/personalize.component').then(m => m.PersonalizeComponent)
  },
  {
    path: 'send',
    canActivate: [authGuard],
    loadComponent: () => import('./features/cards/personalize/personalize.component').then(m => m.PersonalizeComponent)
  },
  {
    path: 'history',
    canActivate: [authGuard],
    loadComponent: () => import('./features/cards/history/history.component').then(m => m.HistoryComponent)
  },
  {
    path: 'contacts',
    canActivate: [authGuard],
    loadComponent: () => import('./features/contacts/contacts.component').then(m => m.ContactsComponent)
  },

  // ── Admin routes ─────────────────────────────────────
  {
    path: 'admin',
    canActivate: [adminGuard],
    loadComponent: () => import('./features/admin/dashboard/admin-dashboard.component').then(m => m.AdminDashboardComponent)
  },
  {
    path: 'admin/users',
    canActivate: [adminGuard],
    loadComponent: () => import('./features/admin/users/admin-users.component').then(m => m.AdminUsersComponent)
  },
  {
    path: 'admin/subscriptions',
    canActivate: [adminGuard],
    loadComponent: () => import('./features/admin/subscriptions/admin-subscriptions.component').then(m => m.AdminSubscriptionsComponent)
  },
  {
    path: 'admin/feedback',
    canActivate: [adminGuard],
    loadComponent: () => import('./features/admin/feedback/admin-feedback.component').then(m => m.AdminFeedbackComponent)
  },
  {
    path: 'admin/cards',
    canActivate: [adminGuard],
    loadComponent: () => import('./features/admin/cards/admin-cards.component').then(m => m.AdminCardsComponent)
  },
  {
    path: 'admin/logs',
    canActivate: [adminGuard],
    loadComponent: () => import('./features/admin/logs/admin-logs.component').then(m => m.AdminLogsComponent)
  },

  // ── About ────────────────────────────────────────────
  {
    path: 'about',
    canActivate: [nonAdminGuard],
    loadComponent: () => import('./features/about/about.component').then(m => m.AboutComponent)
  },

  // ── Fallback ─────────────────────────────────────────
  { path: '**', redirectTo: '' }
];
