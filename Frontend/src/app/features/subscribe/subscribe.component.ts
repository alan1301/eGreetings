import { Component, signal, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { NavbarComponent } from '../../shared/components/navbar/navbar.component';
import { FooterComponent } from '../../shared/components/footer/footer.component';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../shared/models/api-response.model';
import { SubscriptionService } from '../../core/services/subscription.service';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-subscribe',
  standalone: true,
  imports: [CommonModule, FormsModule, NavbarComponent, FooterComponent],
  templateUrl: './subscribe.component.html',
  styleUrl: './subscribe.component.css'
})
export class SubscribeComponent implements OnInit {
  private http = inject(HttpClient);
  private router = inject(Router);
  private subscriptionService = inject(SubscriptionService);
  private auth = inject(AuthService);
  private readonly base = environment.apiBaseUrl;

  get isLoggedIn(): boolean {
    return this.auth.isLoggedIn();
  }

  errorMsg = signal('');
  successMsg = signal('');
  loading = signal(false);
  selectedPlan = signal<'monthly' | 'annual' | null>(null);

  // Subscription ownership state
  hasSubscription = signal(false);
  ownedPlan = signal<'monthly' | 'annual'>('annual');
  daysRemaining = signal(0);
  maskedCardNumber = signal('');
  countdown = signal(5);

  // Form fields - using signals for reactivity
  firstName = signal('');
  lastName = signal('');
  email = signal('');
  cardName = signal('');
  cardNumber = signal('');
  expiryMonth = signal('');
  expiryYear = signal('');
  cvc = signal('');

  get availableYears(): number[] {
    const currentYear = new Date().getFullYear();
    return Array.from({length: 15}, (_, i) => currentYear + i);
  }

  // Field-level error messages
  firstNameError = signal('');
  lastNameError = signal('');
  emailError = signal('');
  cardNameError = signal('');
  cardNumberError = signal('');
  expiryError = signal('');
  cvcError = signal('');

  // Track which fields have been touched
  firstNameTouched = false;
  lastNameTouched = false;
  emailTouched = false;
  cardNameTouched = false;
  cardNumberTouched = false;
  expiryTouched = false;
  cvcTouched = false;

  ngOnInit() {
    if (!this.auth.isLoggedIn()) {
      this.hasSubscription.set(false);
      return;
    }

    // Try to load from backend API first
    this.subscriptionService.getCurrentSubscription().subscribe({
      next: (res) => {
        if (res.data && res.data.status === 'Active') {
          this.hasSubscription.set(true);
          this.ownedPlan.set(res.data.plan);
          this.daysRemaining.set(res.data.daysRemaining);
          this.maskedCardNumber.set(res.data.paymentMethod === 'BankTransfer' ? 'BANK' : '****');
        } else {
          // Fallback to localStorage if no active backend subscription
          this.loadFromLocalStorage();
        }
      },
      error: () => {
        // Fallback to localStorage on error
        this.loadFromLocalStorage();
      }
    });
  }

  private loadFromLocalStorage() {
    const stored = localStorage.getItem('eg_subscription');
    if (stored) {
      try {
        const data = JSON.parse(stored);
        this.hasSubscription.set(true);
        this.ownedPlan.set(data.plan);
        // Calculate days remaining from purchasedAt
        const purchasedAt = new Date(data.purchasedAt);
        const totalDays = data.plan === 'annual' ? 365 : 30;
        const elapsed = Math.floor((Date.now() - purchasedAt.getTime()) / (1000 * 60 * 60 * 24));
        this.daysRemaining.set(Math.max(0, totalDays - elapsed));
        this.maskedCardNumber.set(data.cardLast4);
      } catch (e) {
        console.error('Error parsing stored subscription:', e);
      }
    }
  }

  selectPlan(plan: 'monthly' | 'annual') {
    this.selectedPlan.set(plan);
  }

  // Auto-formatting handlers
  onCardNumberChange(value: string) {
    // Strip everything except digits and format with spaces every 4 digits
    const cleaned = value.replace(/\D/g, '');
    const formatted = cleaned.match(/.{1,4}/g)?.join(' ') || cleaned;
    this.cardNumber.set(formatted);
    if (this.cardNumberTouched) this.validateCardNumber();
  }

  onCVCChange(value: string) {
    const cleaned = value.replace(/\D/g, '');
    this.cvc.set(cleaned);
    if (this.cvcTouched) this.validateCVC();
  }

  // Validation methods
  validateFirstName(): boolean {
    this.firstNameTouched = true;
    const value = this.firstName().trim();
    if (!value) {
      this.firstNameError.set('First name is required');
      return false;
    }
    if (value.length > 100) {
      this.firstNameError.set('First name must not exceed 100 characters');
      return false;
    }
    if (!/^[a-zA-ZÀ-ỹ\s\-']+$/.test(value)) {
      this.firstNameError.set('First name can only contain letters, spaces, hyphens, and apostrophes');
      return false;
    }
    this.firstNameError.set('');
    return true;
  }

  validateLastName(): boolean {
    this.lastNameTouched = true;
    const value = this.lastName().trim();
    if (!value) {
      this.lastNameError.set('Last name is required');
      return false;
    }
    if (value.length > 100) {
      this.lastNameError.set('Last name must not exceed 100 characters');
      return false;
    }
    if (!/^[a-zA-ZÀ-ỹ\s\-']+$/.test(value)) {
      this.lastNameError.set('Last name can only contain letters, spaces, hyphens, and apostrophes');
      return false;
    }
    this.lastNameError.set('');
    return true;
  }

  validateEmail(): boolean {
    this.emailTouched = true;
    const value = this.email().trim();
    if (!value) {
      this.emailError.set('Email is required');
      return false;
    }
    if (value.length > 256) {
      this.emailError.set('Email must not exceed 256 characters');
      return false;
    }
    // RFC 5322 simplified email validation
    const emailRegex = /^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$/;
    if (!emailRegex.test(value)) {
      this.emailError.set('Please enter a valid email address');
      return false;
    }
    this.emailError.set('');
    return true;
  }

  validateCardName(): boolean {
    this.cardNameTouched = true;
    const value = this.cardName().trim();
    if (!value) {
      this.cardNameError.set('Card name is required');
      return false;
    }
    if (value.length < 3) {
      this.cardNameError.set('Card name must be at least 3 characters');
      return false;
    }
    if (value.length > 100) {
      this.cardNameError.set('Card name must not exceed 100 characters');
      return false;
    }
    if (!/^[a-zA-ZÀ-ỹ\s]+$/.test(value)) {
      this.cardNameError.set('Card name can only contain letters and spaces');
      return false;
    }
    this.cardNameError.set('');
    return true;
  }

  validateCardNumber(): boolean {
    this.cardNumberTouched = true;
    const value = this.cardNumber().trim().replace(/[\s-]/g, ''); // Strip spaces and dashes
    if (!value) {
      this.cardNumberError.set('Card number is required');
      return false;
    }
    if (!/^\d+$/.test(value)) {
      this.cardNumberError.set('Card number can only contain digits');
      return false;
    }
    if (value.length < 13 || value.length > 19) {
      this.cardNumberError.set('Card number must be between 13 and 19 digits');
      return false;
    }
    // Luhn algorithm validation (Bypassed for testing)
    // if (!this.luhnCheck(value)) {
    //   this.cardNumberError.set('Invalid card number');
    //   return false;
    // }
    this.cardNumberError.set('');
    return true;
  }

  validateExpiry(): boolean {
    this.expiryTouched = true;
    const monthStr = this.expiryMonth();
    const yearStr = this.expiryYear();
    
    if (!monthStr || !yearStr) {
      this.expiryError.set('Expiry date is required');
      return false;
    }
    
    const year = parseInt(yearStr, 10);
    const month = parseInt(monthStr, 10);
    const currentDate = new Date();
    const currentYear = currentDate.getFullYear();
    const currentMonth = currentDate.getMonth() + 1;
    
    if (year < currentYear || (year === currentYear && month < currentMonth)) {
      this.expiryError.set('Card has expired');
      return false;
    }
    this.expiryError.set('');
    return true;
  }

  validateCVC(): boolean {
    this.cvcTouched = true;
    const value = this.cvc().trim();
    if (!value) {
      this.cvcError.set('CVC is required');
      return false;
    }
    if (!/^\d+$/.test(value)) {
      this.cvcError.set('CVC can only contain digits');
      return false;
    }
    if (value.length < 3 || value.length > 4) {
      this.cvcError.set('CVC must be 3 or 4 digits');
      return false;
    }
    this.cvcError.set('');
    return true;
  }

  // Check if individual field is valid (without setting error messages)
  private checkFirstNameValid(): boolean {
    const value = this.firstName().trim();
    return value.length > 0 && value.length <= 100 && /^[a-zA-ZÀ-ỹ\s\-']+$/.test(value);
  }

  private checkLastNameValid(): boolean {
    const value = this.lastName().trim();
    return value.length > 0 && value.length <= 100 && /^[a-zA-ZÀ-ỹ\s\-']+$/.test(value);
  }

  private checkEmailValid(): boolean {
    const value = this.email().trim();
    if (!value || value.length > 256) return false;
    const emailRegex = /^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$/;
    return emailRegex.test(value);
  }

  private checkCardNameValid(): boolean {
    const value = this.cardName().trim();
    return value.length >= 3 && value.length <= 100 && /^[a-zA-ZÀ-ỹ\s]+$/.test(value);
  }

  private checkCardNumberValid(): boolean {
    const value = this.cardNumber().trim().replace(/[\s-]/g, '');
    if (!value || !/^\d+$/.test(value)) return false;
    if (value.length < 13 || value.length > 19) return false;
    return true; // this.luhnCheck(value); bypassed for testing
  }

  private checkExpiryValid(): boolean {
    const monthStr = this.expiryMonth();
    const yearStr = this.expiryYear();
    if (!monthStr || !yearStr) return false;
    const year = parseInt(yearStr, 10);
    const month = parseInt(monthStr, 10);
    const currentDate = new Date();
    return year > currentDate.getFullYear() || (year === currentDate.getFullYear() && month >= currentDate.getMonth() + 1);
  }

  private checkCVCValid(): boolean {
    const value = this.cvc().trim();
    return /^\d{3,4}$/.test(value);
  }

  isFormValid(): boolean {
    // Check validity WITHOUT setting error messages
    return this.selectedPlan() !== null &&
           this.checkFirstNameValid() && 
           this.checkLastNameValid() && 
           this.checkEmailValid() && 
           this.checkCardNameValid() && 
           this.checkCardNumberValid() && 
           this.checkExpiryValid() && 
           this.checkCVCValid();
  }

  // Validate all fields and show errors (called on submit)
  private validateAllFields(): boolean {
    const isFirstNameValid = this.validateFirstName();
    const isLastNameValid = this.validateLastName();
    const isEmailValid = this.validateEmail();
    const isCardNameValid = this.validateCardName();
    const isCardNumberValid = this.validateCardNumber();
    const isExpiryValid = this.validateExpiry();
    const isCVCValid = this.validateCVC();

    return isFirstNameValid && isLastNameValid && isEmailValid && 
           isCardNameValid && isCardNumberValid && isExpiryValid && isCVCValid;
  }

  onSubscribe() {
    if (!this.selectedPlan()) {
      this.errorMsg.set('Please select a subscription plan before continuing.');
      return;
    }

    // Validate all fields and show error messages
    if (!this.validateAllFields()) {
      this.errorMsg.set('Please correct the errors in the form before submitting.');
      return;
    }

    this.loading.set(true);
    this.errorMsg.set('');

    // Simulate payment processing: 1.5s
    setTimeout(() => {
      this.loading.set(false);
      this.successMsg.set(
        `Payment confirmed! Redirecting to homepage in 5 seconds...`
      );

      // Start countdown and redirect
      let count = 5;
      this.countdown.set(count);
      const timer = setInterval(() => {
        count--;
        this.countdown.set(count);
        if (count <= 0) {
          clearInterval(timer);
          // Store subscription state before redirecting
          const last4 = this.cardNumber().replace(/\s/g, '').slice(-4);
          
          localStorage.setItem('eg_subscription', JSON.stringify({
            plan: this.selectedPlan(),
            cardLast4: last4,
            purchasedAt: new Date().toISOString()
          }));

          this.hasSubscription.set(true);
          this.ownedPlan.set(this.selectedPlan()!);
          this.daysRemaining.set(this.selectedPlan() === 'annual' ? 365 : 30);
          this.maskedCardNumber.set(last4);
          this.router.navigate(['/']);
        }
      }, 1000);
    }, 1500);

    /* 
    // Actual API call when backend is updated (FE-BE-02: unwrap ApiResponse):
    const payload = {
      firstName: this.firstName(),
      lastName: this.lastName(),
      email: this.email(),
      plan: this.selectedPlan(),
    };
    
    this.http.post<ApiResponse<any>>(`${this.base}/subscriptions`, payload).subscribe({
      next: (res) => {
        this.loading.set(false);
        this.successMsg.set(res.message);
        setTimeout(() => this.router.navigate(['/profile']), 2000);
      },
      error: (err) => {
        this.loading.set(false);
        // FE-BE-03: Read error correctly from BE response
        const body = err.error as ApiResponse;
        if (body?.errors?.length) {
          this.errorMsg.set(body.errors[0].message);
        } else {
          this.errorMsg.set(body?.message ?? 'Subscription failed. Please try again.');
        }
      }
    });
    */
  }
}
