import { Component, inject, signal, OnInit, ChangeDetectionStrategy, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { ApiResponse } from '../../../shared/models/api-response.model';

const REMEMBERED_EMAIL_KEY = 'remembered_email';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent implements OnInit {
  private fb = inject(FormBuilder);
  private auth = inject(AuthService);
  private router = inject(Router);
  private destroyRef = inject(DestroyRef);

  errorMsg = signal('');
  loading = signal(false);
  showPw = signal(false);

  form = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]],
    rememberMe: [false]   // BR-05
  });

  ngOnInit() {
    // BR-05: restore saved email if user had previously checked "Remember me"
    const savedEmail = localStorage.getItem(REMEMBERED_EMAIL_KEY);
    if (savedEmail) {
      this.form.patchValue({ email: savedEmail, rememberMe: true });
    }
  }

  onSubmit() {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.loading.set(true);
    this.errorMsg.set('');

    const { email, password, rememberMe } = this.form.value;

    // BR-05: persist or clear remembered email based on checkbox
    if (rememberMe) {
      localStorage.setItem(REMEMBERED_EMAIL_KEY, email!);
    } else {
      localStorage.removeItem(REMEMBERED_EMAIL_KEY);
    }

    this.auth.login(email!, password!, rememberMe ?? false)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
      next: (data) => {
        this.loading.set(false);
        const user = data?.user;
        if (user?.role === 'Admin') {
          this.router.navigate(['/admin']);
        } else {
          this.router.navigate(['/']);
        }
      },
      error: (err) => {
        this.loading.set(false);
        const body = err.error as ApiResponse;
        this.errorMsg.set(body?.message ?? body?.errors?.[0]?.message ?? 'Login failed. Please try again.');
      }
    });
  }
}
