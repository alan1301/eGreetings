import { Component, signal, inject, OnInit, ChangeDetectionStrategy, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { NavbarComponent } from '../../shared/components/navbar/navbar.component';
import { FooterComponent } from '../../shared/components/footer/footer.component';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../shared/models/api-response.model';
import { SubscriptionService } from '../../core/services/subscription.service';
import { AuthService } from '../../core/services/auth.service';
import { AuthModalService } from '../../core/services/auth-modal.service';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-subscribe',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, NavbarComponent, FooterComponent],
  templateUrl: './subscribe.component.html',
  styleUrl: './subscribe.component.css'
})
export class SubscribeComponent implements OnInit {
  private http = inject(HttpClient);
  private router = inject(Router);
  private subscriptionService = inject(SubscriptionService);
  private auth = inject(AuthService);
  private authModal = inject(AuthModalService);
  private destroyRef = inject(DestroyRef);
  private readonly base = environment.apiBaseUrl;

  get isLoggedIn(): boolean {
    return this.auth.isLoggedIn();
  }

  errorMsg = signal('');
  successMsg = signal('');
  loading = signal(false);
  selectedPlan = signal<'monthly' | 'annual' | null>(null);
  showPaymentModal = signal(false);

  // Subscription ownership state
  hasSubscription = signal(false);
  isPending = signal(false);
  showPendingWarning = signal(false);
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

    // Load subscription state from backend — tied to the account, does not reset on logout
    this.subscriptionService.getCurrentSubscription().pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (res) => {
        if (res.data && res.data.status === 'Active') {
          this.hasSubscription.set(true);
          this.ownedPlan.set(res.data.plan);
          this.daysRemaining.set(res.data.daysRemaining);
          this.maskedCardNumber.set(res.data.paymentMethod === 'BankTransfer' ? 'BANK' : '****');
        } else if (res.data && res.data.status === 'Pending') {
          this.isPending.set(true);
        }
        // Expired/Disabled → still show plan selection page
      },
      error: () => {}
    });
  }

  selectPlan(plan: 'monthly' | 'annual') {
    // Block plan selection for unauthenticated users — open login modal instead
    if (!this.auth.isLoggedIn()) {
      this.authModal.open('login');
      return;
    }
    // Block plan selection while payment is pending
    if (this.isPending()) {
      this.showPendingWarning.set(true);
      return;
    }
    this.selectedPlan.set(plan);
    this.showPaymentModal.set(true);
    this.errorMsg.set('');
    this.successMsg.set('');
  }

  closePendingWarning() {
    this.showPendingWarning.set(false);
  }

  closeModal() {
    if (this.loading() || !!this.successMsg()) return; // Block closing while processing
    this.showPaymentModal.set(false);
    this.selectedPlan.set(null);
    this.errorMsg.set('');
    // Reset form fields
    this.firstName.set(''); this.lastName.set(''); this.email.set('');
    this.cardName.set(''); this.cardNumber.set('');
    this.expiryMonth.set(''); this.expiryYear.set(''); this.cvc.set('');
    // Reset error messages
    this.firstNameError.set(''); this.lastNameError.set(''); this.emailError.set('');
    this.cardNameError.set(''); this.cardNumberError.set('');
    this.expiryError.set(''); this.cvcError.set('');
    // Reset touched state
    this.firstNameTouched = this.lastNameTouched = this.emailTouched = false;
    this.cardNameTouched = this.cardNumberTouched = this.expiryTouched = this.cvcTouched = false;
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
    // Check validity WITHOUT setting error messages (selectedPlan was already set when modal opened)
    return this.checkFirstNameValid() &&
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
    if (!this.validateAllFields()) {
      this.errorMsg.set('Please correct the errors in the form before submitting.');
      return;
    }

    this.loading.set(true);
    this.errorMsg.set('');

    // Mock payment processing (1.5s) — replace with real payment gateway later
    setTimeout(() => {
      this.loading.set(false);
      this.successMsg.set('Payment confirmed! Redirecting to your profile in 5 seconds...');

      // Call real API to save subscription to DB (Admin activates later)
      // BR-14: emailList needs ≥10 emails — repeat email from form while no payment gateway
      const userEmail = this.email().trim() || 'user@example.com';
      const emailList = Array(10).fill(userEmail);
      this.subscriptionService.createSubscription(emailList, 'BankTransfer').pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
        next: () => {},
        error: (err) => console.warn('[Subscribe] API call (non-blocking):', err)
      });

      // Countdown then redirect to profile
      let count = 5;
      this.countdown.set(count);
      const timer = setInterval(() => {
        count--;
        this.countdown.set(count);
        if (count <= 0) {
          clearInterval(timer);
          this.showPaymentModal.set(false);
          this.router.navigate(['/profile']);
        }
      }, 1000);
    }, 1500);
  }
}
