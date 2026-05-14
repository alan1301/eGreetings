import { Component, AfterViewInit, OnDestroy, ElementRef, Inject, PLATFORM_ID, inject, ChangeDetectionStrategy } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { NavbarComponent } from '../../shared/components/navbar/navbar.component';
import { FooterComponent } from '../../shared/components/footer/footer.component';
import { AuthModalService } from '../../core/services/auth-modal.service';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-about',
  standalone: true,
  imports: [CommonModule, RouterLink, NavbarComponent, FooterComponent],
  styleUrl: './about.component.css',
  templateUrl: './about.component.html'
})
export class AboutComponent implements AfterViewInit, OnDestroy {
  private observer: IntersectionObserver | null = null;
  private isBrowser: boolean;
  private modal = inject(AuthModalService);

  constructor(private el: ElementRef, @Inject(PLATFORM_ID) platformId: Object) {
    this.isBrowser = isPlatformBrowser(platformId);
  }

  openRegister() { this.modal.open('register'); }

  ngAfterViewInit(): void {
    if (!this.isBrowser) return;
    this.observer = new IntersectionObserver(
      (entries) => {
        entries.forEach(entry => {
          if (entry.isIntersecting) {
            entry.target.classList.add('visible');
          }
        });
      },
      { threshold: 0.12 }
    );
    const targets = this.el.nativeElement.querySelectorAll('.reveal, .reveal-left, .reveal-right');
    targets.forEach((t: Element) => this.observer!.observe(t));
  }

  ngOnDestroy(): void {
    this.observer?.disconnect();
  }
}
