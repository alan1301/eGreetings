import { Component, signal, computed, output, ViewChild, ElementRef, AfterViewInit, OnDestroy, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-color-wheel',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './color-wheel.component.html'
})
export class ColorWheelComponent implements AfterViewInit, OnDestroy {
  colorChange = output<string>();
  @ViewChild('wheelCanvas') canvasRef!: ElementRef<HTMLCanvasElement>;

  // HSL state
  hue        = signal(30);
  saturation = signal(80);
  lightness  = signal(35);

  selectedHex = computed(() =>
    this.hslToHex(this.hue(), this.saturation(), this.lightness())
  );

  hexInput = signal('7c5900');
  recent   = signal<string[]>([]);

  // Handle position on the 200×200 canvas
  handleX = computed(() => {
    const angle = (this.hue() - 90) * Math.PI / 180;
    const dist  = (this.saturation() / 100) * 88; // 88 = radius - margin
    return 100 + dist * Math.cos(angle) - 11; // -11 = half handle width
  });
  handleY = computed(() => {
    const angle = (this.hue() - 90) * Math.PI / 180;
    const dist  = (this.saturation() / 100) * 88;
    return 100 + dist * Math.sin(angle) - 11;
  });

  brightnessGrad = computed(() => {
    const h = this.hue(), s = this.saturation();
    return `linear-gradient(to top, #000 0%, hsl(${h},${s}%,50%) 50%, #fff 100%)`;
  });

  // ── Lifecycle ──────────────────────────────────────
  private isDragging = false;
  private boundMove = this.onMouseMove.bind(this);
  private boundUp   = this.onMouseUp.bind(this);

  ngAfterViewInit() {
    this.drawWheel();
    window.addEventListener('mousemove', this.boundMove);
    window.addEventListener('mouseup',   this.boundUp);
  }

  ngOnDestroy() {
    window.removeEventListener('mousemove', this.boundMove);
    window.removeEventListener('mouseup',   this.boundUp);
  }

  // ── Draw wheel ─────────────────────────────────────
  private drawWheel() {
    const canvas = this.canvasRef.nativeElement;
    const ctx    = canvas.getContext('2d')!;
    const size   = 200;
    const r      = size / 2;
    const img    = ctx.createImageData(size, size);
    const d      = img.data;

    for (let y = 0; y < size; y++) {
      for (let x = 0; x < size; x++) {
        const dx   = x - r;
        const dy   = y - r;
        const dist = Math.sqrt(dx * dx + dy * dy);
        const i    = (y * size + x) * 4;

        if (dist <= r) {
          const hue = ((Math.atan2(dy, dx) * 180 / Math.PI) + 90 + 360) % 360;
          const sat = Math.min(dist / (r - 2), 1) * 100;
          // White center → pure color at edge
          const lig = 100 - sat / 2;
          const [rr, gg, bb] = this.hslToRgb(hue, sat, lig);
          d[i] = rr; d[i+1] = gg; d[i+2] = bb; d[i+3] = 255;
        } else {
          d[i+3] = 0;
        }
      }
    }
    ctx.putImageData(img, 0, 0);

    // Subtle vignette ring
    const grad = ctx.createRadialGradient(r, r, r - 4, r, r, r);
    grad.addColorStop(0, 'transparent');
    grad.addColorStop(1, 'rgba(0,0,0,0.15)');
    ctx.fillStyle = grad;
    ctx.beginPath();
    ctx.arc(r, r, r, 0, Math.PI * 2);
    ctx.fill();
  }

  // ── Mouse / Touch events ───────────────────────────
  onWheelDown(e: MouseEvent) {
    this.isDragging = true;
    this.updateFromXY(e.clientX, e.clientY);
  }
  private onMouseMove(e: MouseEvent) {
    if (this.isDragging) this.updateFromXY(e.clientX, e.clientY);
  }
  private onMouseUp() {
    if (this.isDragging) { this.isDragging = false; this.pushRecent(); }
  }

  onWheelTouch(e: TouchEvent) {
    e.preventDefault();
    const t = e.touches[0];
    this.updateFromXY(t.clientX, t.clientY);
    const move = (ev: TouchEvent) => { ev.preventDefault(); this.updateFromXY(ev.touches[0].clientX, ev.touches[0].clientY); };
    const up   = () => { this.pushRecent(); document.removeEventListener('touchmove', move); document.removeEventListener('touchend', up); };
    document.addEventListener('touchmove', move, { passive: false });
    document.addEventListener('touchend', up);
  }

  private updateFromXY(clientX: number, clientY: number) {
    const rect = this.canvasRef.nativeElement.getBoundingClientRect();
    const dx   = clientX - rect.left - rect.width / 2;
    const dy   = clientY - rect.top  - rect.height / 2;
    const dist = Math.sqrt(dx * dx + dy * dy);
    const r    = rect.width / 2;

    const hue = ((Math.atan2(dy, dx) * 180 / Math.PI) + 90 + 360) % 360;
    const sat = Math.min(dist / (r - 2), 1) * 100;

    this.hue.set(Math.round(hue));
    this.saturation.set(Math.round(sat));
    this.syncHex();
    this.colorChange.emit(this.selectedHex());
  }

  // ── Controls ───────────────────────────────────────
  onLightness(e: Event) {
    this.lightness.set(+(e.target as HTMLInputElement).value);
    this.syncHex();
    this.colorChange.emit(this.selectedHex());
  }

  onNativePick(e: Event) {
    this.pickHex((e.target as HTMLInputElement).value);
  }

  onHexType(e: Event) {
    this.hexInput.set((e.target as HTMLInputElement).value);
  }

  commitHex() {
    const v = this.hexInput();
    if (/^[0-9a-fA-F]{6}$/.test(v)) this.pickHex('#' + v);
  }

  pickHex(hex: string) {
    const [h, s, l] = this.hexToHsl(hex);
    this.hue.set(h); this.saturation.set(s); this.lightness.set(l);
    this.syncHex();
    this.colorChange.emit(hex);
  }

  private syncHex() {
    this.hexInput.set(this.selectedHex().replace('#', '').toUpperCase());
  }

  private pushRecent() {
    const hex = this.selectedHex();
    this.recent.set([hex, ...this.recent().filter(c => c !== hex)].slice(0, 10));
  }

  // ── Color math ─────────────────────────────────────
  private hslToHex(h: number, s: number, l: number): string {
    const [r, g, b] = this.hslToRgb(h, s, l);
    return '#' + [r, g, b].map(v => v.toString(16).padStart(2, '0')).join('');
  }

  private hslToRgb(h: number, s: number, l: number): [number, number, number] {
    s /= 100; l /= 100;
    const k = (n: number) => (n + h / 30) % 12;
    const a = s * Math.min(l, 1 - l);
    const f = (n: number) => l - a * Math.max(-1, Math.min(k(n) - 3, Math.min(9 - k(n), 1)));
    return [Math.round(f(0) * 255), Math.round(f(8) * 255), Math.round(f(4) * 255)];
  }

  private hexToHsl(hex: string): [number, number, number] {
    const n = parseInt(hex.replace('#', ''), 16);
    let r = ((n >> 16) & 255) / 255;
    let g = ((n >>  8) & 255) / 255;
    let b = ( n        & 255) / 255;
    const max = Math.max(r, g, b), min = Math.min(r, g, b);
    let h = 0, s = 0;
    const l = (max + min) / 2;
    if (max !== min) {
      const d = max - min;
      s = l > 0.5 ? d / (2 - max - min) : d / (max + min);
      switch (max) {
        case r: h = ((g - b) / d + (g < b ? 6 : 0)) / 6; break;
        case g: h = ((b - r) / d + 2) / 6; break;
        case b: h = ((r - g) / d + 4) / 6; break;
      }
    }
    return [Math.round(h * 360), Math.round(s * 100), Math.round(l * 100)];
  }
}
