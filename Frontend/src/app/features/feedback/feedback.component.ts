import { Component, OnInit, signal, inject, ChangeDetectionStrategy, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { NavbarComponent } from '../../shared/components/navbar/navbar.component';
import { FooterComponent } from '../../shared/components/footer/footer.component';
import { FeedbackService } from '../../core/services/feedback.service';
import { AuthService } from '../../core/services/auth.service';
import { AuthModalService } from '../../core/services/auth-modal.service';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-feedback',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, NavbarComponent, FooterComponent],
  templateUrl: './feedback.component.html'
})
export class FeedbackComponent implements OnInit {
  private feedbackService = inject(FeedbackService);
  private authService = inject(AuthService);
  private authModal = inject(AuthModalService);
  private destroyRef = inject(DestroyRef);

  name = '';
  email = '';
  subject = '';
  message = '';

  starRating = signal(0);
  hoverRating = signal(0);
  loading = signal(false);
  submitted = signal(false);
  errorMessage = signal('');

  readonly stars = [1, 2, 3, 4, 5];

  ngOnInit() {
    const user = this.authService['currentUserSubject'].value;
    if (user) {
      this.name = user.fullName;
      this.email = user.email;
    }
  }

  setRating(n: number) {
    this.starRating.set(n);
  }

  setHover(n: number) {
    this.hoverRating.set(n);
  }

  clearHover() {
    this.hoverRating.set(0);
  }

  isStarActive(n: number): boolean {
    return n <= (this.hoverRating() || this.starRating());
  }

  onSubmit() {
    this.errorMessage.set('');

    if (!this.authService.isLoggedIn()) {
      this.authModal.open('login');
      return;
    }

    if (!this.name.trim() || !this.email.trim() || !this.message.trim()) {
      this.errorMessage.set('Please fill in all required fields.');
      return;
    }

    this.loading.set(true);
    const payload: any = {
      title: this.subject,
      content: this.message.trim()
    };
    if (this.starRating() > 0) {
      payload.starRating = this.starRating();
    }

    this.feedbackService.submitFeedback(payload).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success) {
          this.submitted.set(true);
          this.subject = 'General Feedback';
          this.message = '';
          this.starRating.set(0);
        } else {
          this.errorMessage.set(res.message || 'Something went wrong. Please try again.');
        }
      },
      error: (err) => {
        this.loading.set(false);
        if (err.status === 422 || err.status === 400) {
          this.errorMessage.set(err.error?.message || 'Invalid submission. Please check your input.');
        } else if (err.status === 429) {
          this.errorMessage.set('You have reached the daily limit of 5 feedbacks. Please try again tomorrow.');
        } else {
          this.errorMessage.set('Unable to send feedback. Please try again later.');
        }
      }
    });
  }

  resetForm() {
    this.submitted.set(false);
    this.errorMessage.set('');
  }
}
