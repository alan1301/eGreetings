import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AdminSidebarComponent } from '../components/admin-sidebar/admin-sidebar.component';
import { AdminFeedbackService, FeedbackDto } from '../../../core/services/admin-feedback.service';
import { PaginationMeta } from '../../../shared/models/api-response.model';

@Component({
  selector: 'app-admin-feedback',
  standalone: true,
  imports: [CommonModule, RouterLink, AdminSidebarComponent, FormsModule],
  providers: [DatePipe],
  templateUrl: './admin-feedback.component.html'
})
export class AdminFeedbackComponent implements OnInit {
  private feedbackService = inject(AdminFeedbackService);
  protected readonly Math = Math;

  feedbacks = signal<FeedbackDto[]>([]);
  meta = signal<PaginationMeta | null>(null);
  loading = signal<boolean>(false);

  // Filters
  filterStatus = '';
  currentPage = 1;
  pageSize = 20;

  ngOnInit() {
    this.loadFeedbacks();
  }

  loadFeedbacks() {
    this.loading.set(true);
    this.feedbackService.getFeedbacks(
      this.currentPage,
      this.pageSize,
      this.filterStatus || undefined
    ).subscribe({
      next: (res) => {
        this.feedbacks.set(res.data?.items || []);
        this.meta.set(res.data?.meta || null);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Failed to load feedbacks', err);
        this.loading.set(false);
      }
    });
  }

  applyFilters() {
    this.currentPage = 1;
    this.loadFeedbacks();
  }

  goToPage(page: number) {
    if (page < 1 || (this.meta() && page > Math.ceil(this.meta()!.total / this.meta()!.pageSize))) return;
    this.currentPage = page;
    this.loadFeedbacks();
  }

  markAsRead(fb: FeedbackDto) {
    this.feedbackService.markAsRead(fb.id).subscribe({
      next: () => {
        this.loadFeedbacks();
      },
      error: (err) => {
        console.error('Failed to mark feedback as read', err);
        alert('Failed to mark feedback as read');
      }
    });
  }
}
