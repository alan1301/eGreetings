import { ChangeDetectionStrategy, Component, ElementRef, HostListener, ViewChild, input, model, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DraggableElement } from '../../personalize.types';

type Align = 'left' | 'center' | 'right';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-personalize-canvas',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="aspect-[4/5] bg-surface-container-low rounded-xl overflow-hidden shadow-2xl shadow-on-surface/5 relative flex items-center justify-center p-12">
      <div #previewContainer class="w-full h-full shadow-lg flex flex-col items-center justify-center p-10 border border-outline-variant/10 relative overflow-hidden transition-all duration-500"
           [style.background]="bgStyle()"
           (mousedown)="onBgPointerDown()"
           (touchstart)="onBgPointerDown()">

        @for (el of elements(); track el.id) {
          <div class="absolute cursor-move select-none z-20"
               [style.top]="el.top + '%'"
               [style.left]="el.left + '%'"
               [style.transform]="'translate(-50%, -50%) rotate(' + el.rotate + 'deg) scale(' + el.scale + ')'"
               [style.font-size]="el.baseSize || '1rem'"
               [style.opacity]="el.opacity || '1'"
               [class.ring-2]="activeElementId() === el.id"
               [class.ring-primary]="activeElementId() === el.id"
               [class.ring-offset-2]="activeElementId() === el.id"
               (mousedown)="onElementPointerDown($event, el.id)"
               (touchstart)="onElementPointerDown($event, el.id)">

            @if (el.type === 'emoji') {
              {{ el.content }}
            } @else {
              <img [src]="el.content" class="max-w-[150px] max-h-[150px] object-contain drop-shadow-lg pointer-events-none" draggable="false" />
            }

            @if (activeElementId() === el.id) {
              <button (click)="removeElement(el.id); $event.stopPropagation()"
                      (touchstart)="removeElement(el.id); $event.stopPropagation()"
                      class="absolute -top-3 -right-3 w-6 h-6 bg-error text-on-error rounded-full flex items-center justify-center shadow-md z-30 hover:scale-110 transition-transform">
                <span class="material-symbols-outlined text-[14px]">close</span>
              </button>
              <div class="absolute -bottom-2 -right-2 w-5 h-5 bg-primary text-on-primary rounded-full flex items-center justify-center cursor-nwse-resize shadow-md z-30 hover:scale-110 transition-transform"
                   (mousedown)="onResizePointerDown($event, el.id)"
                   (touchstart)="onResizePointerDown($event, el.id)">
                <span class="material-symbols-outlined text-[12px]">open_in_full</span>
              </div>
            }
          </div>
        }

        <div class="absolute inset-0 paper-texture pointer-events-none opacity-40"></div>

        <div class="z-10 flex flex-col gap-6 w-full" [style.text-align]="align()">
          <span class="font-label text-xs uppercase tracking-[0.3em] text-secondary">To</span>

          <h1 class="font-headline italic leading-tight"
              [style.color]="textColor()"
              [style.font-family]="fontFamily()"
              [style.font-size]="headingSize()">
            {{ heading() || 'Recipient Name' }}
          </h1>

          <div class="w-12 h-px bg-primary"
               [class]="align() === 'center' ? 'mx-auto' : align() === 'right' ? 'ml-auto' : ''">
          </div>

          <p class="italic leading-relaxed max-w-xs"
             [style.color]="textColor()"
             [style.font-family]="fontFamily()"
             [style.font-size]="fontSize()">
            {{ message() || 'Your wishes will appear here...' }}
          </p>

          <span class="font-label text-sm font-bold mt-4 text-on-surface-variant">{{ dateLabel() }}</span>
        </div>
      </div>

      <div class="absolute top-6 left-6 bg-secondary/90 backdrop-blur-md text-on-secondary px-4 py-2 rounded-full flex items-center gap-2 shadow-lg">
        <span class="material-symbols-outlined text-sm">visibility</span>
        <span class="text-xs font-label font-bold uppercase tracking-wider">Live Preview</span>
      </div>
    </div>
  `
})
export class PersonalizeCanvasComponent {
  elements = model<DraggableElement[]>([]);
  activeElementId = model<string | null>(null);

  bgStyle = input.required<string>();
  align = input.required<Align>();
  textColor = input.required<string>();
  fontFamily = input.required<string>();
  fontSize = input.required<string>();
  headingSize = input.required<string>();
  heading = input<string>('');
  message = input<string>('');
  dateLabel = input.required<string>();

  elementRemove = output<string>();

  @ViewChild('previewContainer') previewContainer!: ElementRef<HTMLDivElement>;

  private isDragging = false;
  private isResizing = false;
  private startX = 0;
  private startY = 0;
  private startTop = 0;
  private startLeft = 0;
  private startScale = 1;

  onBgPointerDown() { this.activeElementId.set(null); }

  onElementPointerDown(event: MouseEvent | TouchEvent, id: string) {
    event.preventDefault();
    event.stopPropagation();
    this.activeElementId.set(id);
    this.isDragging = true;
    this.isResizing = false;
    const cx = 'touches' in event ? event.touches[0].clientX : (event as MouseEvent).clientX;
    const cy = 'touches' in event ? event.touches[0].clientY : (event as MouseEvent).clientY;
    this.startX = cx;
    this.startY = cy;
    const el = this.elements().find(e => e.id === id);
    if (el) { this.startTop = el.top; this.startLeft = el.left; }
  }

  onResizePointerDown(event: MouseEvent | TouchEvent, id: string) {
    event.preventDefault();
    event.stopPropagation();
    this.activeElementId.set(id);
    this.isDragging = false;
    this.isResizing = true;
    const cx = 'touches' in event ? event.touches[0].clientX : (event as MouseEvent).clientX;
    const cy = 'touches' in event ? event.touches[0].clientY : (event as MouseEvent).clientY;
    this.startX = cx;
    this.startY = cy;
    const el = this.elements().find(e => e.id === id);
    if (el) this.startScale = el.scale;
  }

  removeElement(id: string) {
    this.elementRemove.emit(id);
  }

  @HostListener('document:mousemove', ['$event'])
  @HostListener('document:touchmove', ['$event'])
  onPointerMove(event: MouseEvent | TouchEvent) {
    if (!this.isDragging && !this.isResizing) return;
    const activeId = this.activeElementId();
    if (!activeId) return;

    const cx = 'touches' in event ? event.touches[0].clientX : (event as MouseEvent).clientX;
    const cy = 'touches' in event ? event.touches[0].clientY : (event as MouseEvent).clientY;
    const dx = cx - this.startX;
    const dy = cy - this.startY;

    if (this.isDragging) {
      if (!this.previewContainer) return;
      const rect = this.previewContainer.nativeElement.getBoundingClientRect();
      const dxP = (dx / rect.width) * 100;
      const dyP = (dy / rect.height) * 100;
      this.elements.update(arr => arr.map(el =>
        el.id === activeId ? { ...el, left: this.startLeft + dxP, top: this.startTop + dyP } : el
      ));
    } else if (this.isResizing) {
      const delta = (dx + dy) * 0.005;
      this.elements.update(arr => arr.map(el =>
        el.id === activeId ? { ...el, scale: Math.max(0.2, this.startScale + delta) } : el
      ));
    }
  }

  @HostListener('document:mouseup')
  @HostListener('document:touchend')
  onPointerUp() {
    this.isDragging = false;
    this.isResizing = false;
  }
}
