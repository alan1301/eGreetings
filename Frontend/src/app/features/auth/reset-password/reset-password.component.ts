import { Component, inject, signal, OnInit, ChangeDetectionStrategy, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators, AbstractControl } from '@angular/forms';
import { RouterLink, Router, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { ApiResponse } from '../../../shared/models/api-response.model';

function passwordMatchValidator(control: AbstractControl) {
  const pw = control.get('newPassword')?.value;
  const cpw = control.get('confirmPassword')?.value;
  return pw === cpw ? null : { passwordMismatch: true };
}

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-reset-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './reset-password.component.html'
})
export class ResetPasswordComponent implements OnInit {
  private fb = inject(FormBuilder);
  private auth = inject(AuthService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private destroyRef = inject(DestroyRef);

  token = signal('');
  loading = signal(false);
  errorMsg = signal('');
  success = signal(false);
  showPw = signal(false);
  showCpw = signal(false);
  invalidToken = signal(false);

  // Password rule helpers
  get pwVal(): string { return this.form?.get('newPassword')?.value ?? ''; }
  get pwTouched(): boolean { return !!this.form?.get('newPassword')?.touched; }
  get pwHasMin(): boolean { return this.pwVal.length >= 8; }
  get pwHasUpper(): boolean { return /[A-Z]/.test(this.pwVal); }
  get pwHasLower(): boolean { return /[a-z]/.test(this.pwVal); }
  get pwHasNum(): boolean { return /\d/.test(this.pwVal); }
  get pwHasSpecial(): boolean { return /[^a-zA-Z0-9]/.test(this.pwVal); }

  form = this.fb.group({
    // BR-01: password complexity
    newPassword: ['', [Validators.required, Validators.minLength(8),
      Validators.pattern(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9])/)
    ]],
    confirmPassword: ['', [Validators.required]]
  }, { validators: passwordMatchValidator });

  ngOnInit() {
    const t = this.route.snapshot.queryParamMap.get('token');
    if (!t) {
      this.invalidToken.set(true);
    } else {
      this.token.set(t);
    }
  }

  onSubmit() {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.loading.set(true);
    this.errorMsg.set('');

    const { newPassword, confirmPassword } = this.form.value;
    this.auth.resetPassword(this.token(), newPassword!, confirmPassword!).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        this.loading.set(false);
        this.success.set(true);
        setTimeout(() => this.router.navigate(['/login']), 2500);
      },
      error: (err) => {
        this.loading.set(false);
        const body = err.error as ApiResponse;
        this.errorMsg.set(body?.message ?? body?.errors?.[0]?.message ?? 'Failed to reset password. The link may have expired.');
      }
    });
  }
}
