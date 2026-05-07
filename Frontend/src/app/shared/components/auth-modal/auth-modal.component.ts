import { Component, inject, signal, DoCheck } from '@angular/core';
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

@Component({
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

  ngDoCheck() {
    const current = this.modalSvc.mode();
    if (current && this.displayMode() !== current) {
      this.displayMode.set(current);
    }
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
  isLocked    = signal(false);
  lockedReason= signal('');

  loginForm = this.fb.group({
    email:    ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]]
  });

  resetLoginState() {
    this.isLocked.set(false);
    this.lockedReason.set('');
    this.loginError.set('');
    this.loginForm.reset();
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
    this.auth.login(email!, password!).subscribe({
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
