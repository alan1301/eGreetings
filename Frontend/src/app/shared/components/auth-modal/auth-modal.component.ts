import { Component, inject, signal, DoCheck, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators, AbstractControl } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthModalService } from '../../../core/services/auth-modal.service';
import { AuthService } from '../../../core/services/auth.service';
import { ApiResponse } from '../../models/api-response.model';

function passwordMatchValidator(control: AbstractControl) {
  const pw = control.get('password')?.value;
  const cpw = control.get('confirmPassword')?.value;
  return pw === cpw ? null : { passwordMismatch: true };
}

const REMEMBERED_EMAIL_KEY = 'remembered_email';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-auth-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  styleUrl: './auth-modal.component.css',
  templateUrl: './auth-modal.component.html'
})
export class AuthModalComponent implements DoCheck {
  modalSvc     = inject(AuthModalService);
  private auth   = inject(AuthService);
  private router = inject(Router);
  private fb     = inject(FormBuilder);

  displayMode = signal<string>('login');
  private lastMode = '';

  private restoreRememberedEmail() {
    const savedEmail = localStorage.getItem(REMEMBERED_EMAIL_KEY);
    if (savedEmail) {
      this.loginForm.patchValue({ email: savedEmail });
      this.rememberMe.set(true);
    }
  }

  ngDoCheck() {
    const current = this.modalSvc.mode();
    if (current && this.displayMode() !== current) {
      this.displayMode.set(current);
    }
    // Restore remembered email every time the modal opens in login mode
    if (current === 'login' && this.lastMode !== 'login') {
      this.restoreRememberedEmail();
    }
    this.lastMode = current ?? '';
  }

  switchMode(to: 'login' | 'register') {
    this.displayMode.set(to);
    this.modalSvc.switch(to);
  }

  close() { this.modalSvc.close(); }

  onBackdropClick(e: MouseEvent) {
    if ((e.target as HTMLElement).id === 'auth-modal-backdrop') this.close();
  }

  /* ── Login ── */
  loginError  = signal('');
  loginLoading= signal(false);
  showPw      = signal(false);
  rememberMe  = signal(false);
  isLocked    = signal(false);
  lockedReason= signal('');

  goToForgotPassword() {
    this.close();
    this.router.navigate(['/forgot-password']);
  }

  loginForm = this.fb.group({
    email:    ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]]
  });

  resetLoginState() {
    this.isLocked.set(false);
    this.lockedReason.set('');
    this.loginError.set('');
    this.loginForm.reset();
    this.restoreRememberedEmail();
  }

  onAcknowledgeLock() {
    this.close();
    this.auth.logout();
    this.router.navigate(['/']);
  }

  onLogin() {
    if (this.loginForm.invalid) { this.loginForm.markAllAsTouched(); return; }
    this.loginLoading.set(true);
    this.loginError.set('');

    const { email, password } = this.loginForm.value;

    // BR-05: persist or clear remembered email based on checkbox
    if (this.rememberMe()) {
      localStorage.setItem(REMEMBERED_EMAIL_KEY, email!);
    } else {
      localStorage.removeItem(REMEMBERED_EMAIL_KEY);
    }

    this.auth.login(email!, password!, this.rememberMe()).subscribe({
      next: () => {
        this.loginLoading.set(false);
        this.close();
        this.loginForm.reset();
        if (this.auth.isAdmin()) this.router.navigate(['/admin']);
        else this.router.navigate(['/']);
      },
      error: (err) => {
        this.loginLoading.set(false);
        const body = err.error as ApiResponse;
        const msg  = body?.message ?? body?.errors?.[0]?.message ?? 'Login failed.';
        if (msg.includes('LOCKED') || body?.errors?.[0]?.field === 'LOCKED') {
          this.isLocked.set(true); this.lockedReason.set(msg);
        } else {
          this.loginError.set(msg);
        }
      }
    });
  }

  /* ── Register ── */
  regError   = signal('');
  regSuccess = signal('');
  regLoading = signal(false);

  // Password rule helpers
  get modalPwVal(): string { return this.regForm?.get('password')?.value ?? ''; }
  get modalPwTouched(): boolean { return !!this.regForm?.get('password')?.touched; }
  get modalPwHasMin(): boolean { return this.modalPwVal.length >= 8; }
  get modalPwHasUpper(): boolean { return /[A-Z]/.test(this.modalPwVal); }
  get modalPwHasLower(): boolean { return /[a-z]/.test(this.modalPwVal); }
  get modalPwHasNum(): boolean { return /\d/.test(this.modalPwVal); }
  get modalPwHasSpecial(): boolean { return /[^a-zA-Z0-9]/.test(this.modalPwVal); }

  regForm = this.fb.group({
    fullName:        ['', [Validators.required, Validators.minLength(2)]],
    email:           ['', [Validators.required, Validators.email]],
    password:        ['', [Validators.required, Validators.minLength(8),
      Validators.pattern(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9])/)
    ]],
    confirmPassword: ['', [Validators.required]]
  }, { validators: passwordMatchValidator });

  onRegister() {
    if (this.regForm.invalid) { this.regForm.markAllAsTouched(); return; }
    this.regLoading.set(true);
    this.regError.set('');
    this.regSuccess.set('');

    const { fullName, email, password, confirmPassword } = this.regForm.value;
    this.auth.register(fullName!, email!, password!, confirmPassword!).subscribe({
      next: () => {
        this.regLoading.set(false);
        this.regSuccess.set('Account created! Redirecting to login...');
        this.regForm.reset();
        setTimeout(() => { this.regSuccess.set(''); this.switchMode('login'); }, 1800);
      },
      error: (err) => {
        this.regLoading.set(false);
        const body = err.error as ApiResponse;
        this.regError.set(body?.errors?.[0]?.message ?? body?.message ?? 'Registration failed.');
      }
    });
  }
}
