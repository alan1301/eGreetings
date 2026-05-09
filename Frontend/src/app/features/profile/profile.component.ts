import { Component, inject, signal, OnInit, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { NavbarComponent } from '../../shared/components/navbar/navbar.component';
import { FooterComponent } from '../../shared/components/footer/footer.component';
import { AuthService } from '../../core/services/auth.service';
import { SubscriptionService, CurrentSubscriptionDto } from '../../core/services/subscription.service';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../shared/models/api-response.model';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, RouterLink, NavbarComponent, FooterComponent],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.css'
})
export class ProfileComponent implements OnInit {
  @ViewChild('fileInput') fileInput!: ElementRef<HTMLInputElement>;

  private auth = inject(AuthService);
  private http = inject(HttpClient);
  private subscriptionService = inject(SubscriptionService);
  private readonly base = environment.apiBaseUrl;

  currentUser = signal(this.auth['currentUserSubject'].value);
  subscription = signal<CurrentSubscriptionDto | null>(null);

  // Profile photo (base64, stored in localStorage per user)
  profilePhoto = signal<string | null>(null);

  // Edit mode state
  isEditing = signal(false);
  editName = '';
  editBirthday = '';

  // Password change
  showPasswordChange = signal(false);
  currentPassword = '';
  newPassword = '';
  confirmPassword = '';

  // UI feedback
  isSaving = signal(false);
  saveSuccess = signal(false);
  saveError = signal('');

  ngOnInit() {
    this.auth.currentUser$.subscribe(u => this.currentUser.set(u));

    // Load saved profile photo from localStorage
    const userId = this.currentUser()?.id;
    if (userId) {
      const savedPhoto = localStorage.getItem(`profile_photo_${userId}`);
      if (savedPhoto) this.profilePhoto.set(savedPhoto);
    }

    this.subscriptionService.getCurrentSubscription().subscribe({
      next: (res) => { if (res.data) this.subscription.set(res.data); },
      error: () => {}
    });
  }

  // ── "Member since" formatted date ──────────────────────────────────
  get memberSinceDisplay(): string {
    const createdAt = this.currentUser()?.createdAt;
    if (!createdAt) return '2026';
    const d = new Date(createdAt);
    return d.toLocaleDateString('en-US', { day: 'numeric', month: 'long', year: 'numeric' });
  }

  // ── Birthday display ───────────────────────────────────────────────
  get birthdayDisplay(): string {
    const bd = localStorage.getItem(`birthday_${this.currentUser()?.id}`);
    if (!bd) return '';
    const d = new Date(bd);
    return d.toLocaleDateString('en-US', { day: 'numeric', month: 'long', year: 'numeric' });
  }

  get savedBirthday(): string {
    return localStorage.getItem(`birthday_${this.currentUser()?.id}`) ?? '';
  }

  // ── Photo upload ───────────────────────────────────────────────────
  triggerPhotoUpload(): void {
    this.fileInput?.nativeElement.click();
  }

  onPhotoSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files?.length) return;
    const file = input.files[0];
    // Basic size guard: 5 MB max
    if (file.size > 5 * 1024 * 1024) {
      alert('Image must be smaller than 5 MB.');
      return;
    }
    const reader = new FileReader();
    reader.onload = (e) => {
      const base64 = e.target?.result as string;
      this.profilePhoto.set(base64);
      const userId = this.currentUser()?.id;
      if (userId) localStorage.setItem(`profile_photo_${userId}`, base64);
    };
    reader.readAsDataURL(file);
    // Reset input so same file can be re-selected
    input.value = '';
  }

  // ── Edit mode ──────────────────────────────────────────────────────
  startEditing(): void {
    this.editName = this.currentUser()?.fullName ?? '';
    this.editBirthday = this.savedBirthday;
    this.currentPassword = '';
    this.newPassword = '';
    this.confirmPassword = '';
    this.showPasswordChange.set(false);
    this.saveSuccess.set(false);
    this.saveError.set('');
    this.isEditing.set(true);
  }

  cancelEditing(): void {
    this.isEditing.set(false);
    this.showPasswordChange.set(false);
    this.saveError.set('');
  }

  togglePasswordChange(): void {
    this.showPasswordChange.update(v => !v);
    if (!this.showPasswordChange()) {
      this.currentPassword = '';
      this.newPassword = '';
      this.confirmPassword = '';
    }
  }

  saveChanges(): void {
    if (!this.editName.trim()) {
      this.saveError.set('Full name cannot be empty.');
      return;
    }
    if (this.showPasswordChange()) {
      if (!this.currentPassword) { this.saveError.set('Please enter your current password.'); return; }
      if (!this.newPassword)     { this.saveError.set('Please enter a new password.'); return; }
      if (this.newPassword !== this.confirmPassword) { this.saveError.set('Passwords do not match.'); return; }
    }

    this.isSaving.set(true);
    this.saveError.set('');

    const payload: any = {
      userId: '00000000-0000-0000-0000-000000000000', // overridden server-side from JWT
      fullName: this.editName.trim(),
      currentPassword: this.showPasswordChange() ? this.currentPassword : null,
      newPassword: this.showPasswordChange() ? this.newPassword : null,
      confirmNewPassword: this.showPasswordChange() ? this.confirmPassword : null,
    };

    this.http.put<ApiResponse<any>>(`${this.base}/auth/me/profile`, payload).subscribe({
      next: (res) => {
        if (res.success) {
          // Update stored user info
          const updated = { ...this.currentUser()!, fullName: this.editName.trim() };
          localStorage.setItem('user', JSON.stringify(updated));
          this.currentUser.set(updated);
          this.auth['currentUserSubject'].next(updated);

          // Save birthday to localStorage
          const userId = updated.id;
          if (this.editBirthday) {
            localStorage.setItem(`birthday_${userId}`, this.editBirthday);
          } else {
            localStorage.removeItem(`birthday_${userId}`);
          }

          this.isSaving.set(false);
          this.saveSuccess.set(true);
          this.isEditing.set(false);
          this.showPasswordChange.set(false);
          setTimeout(() => this.saveSuccess.set(false), 4000);
        }
      },
      error: (err) => {
        const msg = err?.error?.errors?.[0]?.message
          ?? err?.error?.message
          ?? 'Failed to save changes. Please try again.';
        this.saveError.set(msg);
        this.isSaving.set(false);
      }
    });
  }
}
