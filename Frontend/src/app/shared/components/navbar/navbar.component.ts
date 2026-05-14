import { Component, HostListener, inject, signal, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { AuthModalService } from '../../../core/services/auth-modal.service';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  styleUrl: './navbar.component.css',
  templateUrl: './navbar.component.html'
})
export class NavbarComponent {
  private auth  = inject(AuthService);
  private modal = inject(AuthModalService);
  private router = inject(Router);

  currentUser$ = this.auth.currentUser$;
  dropdownOpen  = signal(false);

  isCollectionActive(): boolean {
    return this.router.url.startsWith('/cards') || this.router.url.startsWith('/categories');
  }

  initials(name: string): string {
    return name.split(' ').map(w => w[0]).slice(0, 2).join('').toUpperCase();
  }

  showLogoutConfirm = signal(false);

  toggleDropdown() { this.dropdownOpen.update(v => !v); }
  closeDropdown()  { this.dropdownOpen.set(false); }

  @HostListener('document:click', ['$event'])
  onDocumentClick(e: MouseEvent) {
    if (this.showLogoutConfirm()) return;
    const target = e.target as HTMLElement;
    if (!target.closest('.avatar-btn')) this.dropdownOpen.set(false);
  }

  @HostListener('document:keydown.escape')
  onEscape() { this.showLogoutConfirm.set(false); }

  openLogin()    { this.modal.open('login'); }
  openRegister() { this.modal.open('register'); }

  logout() {
    this.dropdownOpen.set(false);
    this.showLogoutConfirm.set(true);
  }

  confirmLogout() {
    this.showLogoutConfirm.set(false);
    this.auth.logout();
  }

  cancelLogout() {
    this.showLogoutConfirm.set(false);
  }
}
