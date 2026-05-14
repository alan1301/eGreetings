import { Component, HostListener, inject, signal, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../../../core/services/auth.service';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-admin-sidebar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  styleUrl: './admin-sidebar.component.css',
  templateUrl: './admin-sidebar.component.html'
})
export class AdminSidebarComponent {
  private auth = inject(AuthService);

  currentUser$ = this.auth.currentUser$;
  showLogoutConfirm = signal(false);

  initials(name: string): string {
    return name.split(' ').map(w => w[0]).slice(0, 2).join('').toUpperCase();
  }

  logout() { this.showLogoutConfirm.set(true); }

  confirmLogout() {
    this.showLogoutConfirm.set(false);
    this.auth.logout();
  }

  cancelLogout() { this.showLogoutConfirm.set(false); }

  @HostListener('document:keydown.escape')
  onEscape() { this.showLogoutConfirm.set(false); }
}
