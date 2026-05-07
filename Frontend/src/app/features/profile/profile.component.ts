import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { NavbarComponent } from '../../shared/components/navbar/navbar.component';
import { FooterComponent } from '../../shared/components/footer/footer.component';
import { AuthService } from '../../core/services/auth.service';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../shared/models/api-response.model';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, RouterLink, NavbarComponent, FooterComponent],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.css'
})
export class ProfileComponent implements OnInit {
  private auth = inject(AuthService);
  private http = inject(HttpClient);
  private readonly base = environment.apiBaseUrl;

  currentUser = signal(this.auth['currentUserSubject'].value);
  sentCards = signal<any[]>([]);

  ngOnInit() {
    this.auth.currentUser$.subscribe(u => this.currentUser.set(u));
    this.http.get<ApiResponse<any[]>>(`${this.base}/greetings/history`).subscribe({
      next: (res) => { if (res.success) this.sentCards.set(res.data ?? []); },
      error: () => {}
    });
  }
}
