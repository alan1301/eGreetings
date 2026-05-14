import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-hero-section',
  standalone: true,
  imports: [RouterLink],
  template: `
    <section class="hero anim-section">
      <img class="hero-bg"
        src="https://images.unsplash.com/photo-1616628188550-808682f3926d?w=1600&q=85&auto=format&fit=crop"
        alt="Premium stationery">
      <div class="hero-tint"></div>
      <div class="hero-card">
        <h1 class="hero-h1"><span class="hero-word">Crafting</span> <span class="hero-word">Modern</span> <span class="hero-word">Heirlooms</span></h1>
        <p class="hero-sub hero-sub-anim">
          Elevate your correspondence with digital stationery that carries
          the weight and warmth of a handwritten letter.
        </p>
        <a routerLink="/cards" class="hero-btn hero-btn-anim">Explore Collections</a>
      </div>
    </section>
  `
})
export class HeroSectionComponent {}
