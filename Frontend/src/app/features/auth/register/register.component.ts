import { Component, inject, signal, ChangeDetectionStrategy, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators, AbstractControl } from '@angular/forms';
import { RouterLink, Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { ApiResponse } from '../../../shared/models/api-response.model';

function passwordMatchValidator(control: AbstractControl) {
  const pw = control.get('password')?.value;
  const cpw = control.get('confirmPassword')?.value;
  return pw === cpw ? null : { passwordMismatch: true };
}

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css'
})
export class RegisterComponent {
  private fb = inject(FormBuilder);
  private auth = inject(AuthService);
  private router = inject(Router);
  private destroyRef = inject(DestroyRef);

  errorMsg = signal('');
  successMsg = signal('');
  loading = signal(false);

  // Password rule helpers
  get pwVal(): string { return this.form?.get('password')?.value ?? ''; }
  get pwTouched(): boolean { return !!this.form?.get('password')?.touched; }
  get pwHasMin(): boolean { return this.pwVal.length >= 8; }
  get pwHasUpper(): boolean { return /[A-Z]/.test(this.pwVal); }
  get pwHasLower(): boolean { return /[a-z]/.test(this.pwVal); }
  get pwHasNum(): boolean { return /\d/.test(this.pwVal); }
  get pwHasSpecial(): boolean { return /[^a-zA-Z0-9]/.test(this.pwVal); }

  form = this.fb.group({
    fullName: ['', [Validators.required, Validators.minLength(2)]],
    email: ['', [Validators.required, Validators.email]],
    // BR-01: uppercase + lowercase + digit + special char
    password: ['', [Validators.required, Validators.minLength(8),
      Validators.pattern(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9])/)
    ]],
    confirmPassword: ['', [Validators.required]]
  }, { validators: passwordMatchValidator });

  onSubmit() {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.loading.set(true);
    this.errorMsg.set('');
    this.successMsg.set('');

    const { fullName, email, password, confirmPassword } = this.form.value;
    this.auth.register(fullName!, email!, password!, confirmPassword!).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        this.loading.set(false);
        this.successMsg.set('Registration successful! Account activated. Redirecting...');
        setTimeout(() => this.router.navigate(['/login']), 2000);
      },
      error: (err) => {
        this.loading.set(false);
        const body = err.error as ApiResponse;
        if (body?.errors?.length) {
          this.errorMsg.set(body.errors[0].message);
        } else {
          this.errorMsg.set(body?.message ?? 'Registration failed, please try again.');
        }
      }
    });
  }
}
