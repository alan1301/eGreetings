import { Component, inject, OnInit, signal, ChangeDetectionStrategy, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { NavbarComponent } from '../../shared/components/navbar/navbar.component';
import { FooterComponent } from '../../shared/components/footer/footer.component';
import { AuthService } from '../../core/services/auth.service';
import { environment } from '../../../environments/environment';

interface Contact {
  id: string;
  name: string;
  email: string;
  group: string;
  occasionDate?: string;
  occasionLabel?: string;
}

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-contacts',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule, NavbarComponent, FooterComponent],
  styleUrl: './contacts.component.css',
  templateUrl: './contacts.component.html'
})
export class ContactsComponent implements OnInit {
  private http = inject(HttpClient);
  private auth = inject(AuthService);
  private destroyRef = inject(DestroyRef);
  private readonly base = environment.apiBaseUrl;

  contacts   = signal<Contact[]>([]);
  loading    = signal(true);
  showModal  = signal(false);
  saving     = signal(false);
  toastMsg   = signal('');
  toastSuccess = signal(true);

  form = { name: '', email: '', group: 'Friends', occasionLabel: '', occasionDate: '' };

  ngOnInit(): void { this.loadContacts(); }

  loadContacts(): void {
    this.loading.set(true);
    const token = this.auth.getToken();
    if (!token) { this.loading.set(false); return; }

    this.http.get<any>(`${this.base}/contacts`, {
      headers: { Authorization: `Bearer ${token}` }
    }).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: res => {
        this.loading.set(false);
        if (res.success && res.data) this.contacts.set(res.data);
      },
      error: () => { this.loading.set(false); }
    });
  }

  openAddModal(): void {
    this.form = { name: '', email: '', group: 'Friends', occasionLabel: '', occasionDate: '' };
    this.showModal.set(true);
  }

  closeModal(): void { this.showModal.set(false); }

  saveContact(): void {
    if (!this.form.name.trim() || !this.form.email.trim()) {
      this.showToast('Name and email are required.', false);
      return;
    }
    this.saving.set(true);
    const token = this.auth.getToken();
    const body: any = {
      name:  this.form.name.trim(),
      email: this.form.email.trim(),
      group: this.form.group,
    };
    if (this.form.occasionLabel.trim()) body['occasionLabel'] = this.form.occasionLabel.trim();
    if (this.form.occasionDate)         body['occasionDate']  = this.form.occasionDate;

    this.http.post<any>(`${this.base}/contacts`, body, {
      headers: { Authorization: `Bearer ${token}` }
    }).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        this.saving.set(false);
        this.closeModal();
        this.showToast('Contact added!', true);
        this.loadContacts();
      },
      error: err => {
        this.saving.set(false);
        this.showToast(err.error?.message ?? 'Failed to add contact.', false);
      }
    });
  }

  formatOccasion(dateStr: string): string {
    const d = new Date(dateStr);
    return d.toLocaleDateString('en-US', { month: 'long', day: 'numeric' });
  }

  private showToast(msg: string, ok: boolean): void {
    this.toastMsg.set(msg);
    this.toastSuccess.set(ok);
    setTimeout(() => this.toastMsg.set(''), 3500);
  }
}
