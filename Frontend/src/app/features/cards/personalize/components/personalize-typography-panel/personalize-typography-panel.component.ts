import { ChangeDetectionStrategy, Component, model } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Align, ALIGN_BUTTONS, FONTS, FONT_SIZES, HEADING_SIZES } from '../../personalize.types';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-personalize-typography-panel',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="space-y-5">
      <div>
        <label class="form-label">Alignment</label>
        <div class="flex gap-2">
          @for (btn of alignButtons; track btn.value) {
            <button (click)="align.set(btn.value)"
                    [class]="align()===btn.value
                      ? 'flex-1 py-2.5 rounded-lg bg-primary text-on-primary flex items-center justify-center transition-all shadow-md'
                      : 'flex-1 py-2.5 rounded-lg bg-surface-container text-on-surface-variant hover:bg-surface-container-high flex items-center justify-center transition-all'"
                    [title]="btn.label">
              <span class="material-symbols-outlined text-base">{{ btn.icon }}</span>
            </button>
          }
        </div>
      </div>
      <div>
        <label class="form-label" for="p-font">Font</label>
        <div class="relative">
          <span class="material-symbols-outlined absolute left-3 top-1/2 -translate-y-1/2 text-outline-variant text-base pointer-events-none">font_download</span>
          <select id="p-font" [ngModel]="fontFamily()" (ngModelChange)="fontFamily.set($event)"
                  class="input-field pl-10 appearance-none pr-8 cursor-pointer">
            @for (f of fonts; track f.value) {
              <option [value]="f.value" [style.font-family]="f.value">{{ f.label }}</option>
            }
          </select>
          <span class="material-symbols-outlined absolute right-3 top-1/2 -translate-y-1/2 text-outline-variant text-base pointer-events-none">expand_more</span>
        </div>
        <p class="mt-2 text-sm italic text-on-surface-variant px-1" [style.font-family]="fontFamily()">
          Sample: Send Your Wishes...
        </p>
      </div>
      <div>
        <label class="form-label">Message Size</label>
        <div class="flex gap-2 flex-wrap">
          @for (sz of fontSizes; track sz.value) {
            <button (click)="fontSize.set(sz.value)"
                    [class]="fontSize()===sz.value
                      ? 'px-3 py-1.5 rounded-lg bg-primary text-on-primary text-xs font-bold transition-all shadow-md'
                      : 'px-3 py-1.5 rounded-lg bg-surface-container text-on-surface-variant hover:bg-surface-container-high text-xs font-medium transition-all'">
              {{ sz.label }}
            </button>
          }
        </div>
      </div>
      <div>
        <label class="form-label">Heading Size</label>
        <div class="flex gap-2 flex-wrap">
          @for (sz of headingSizes; track sz.value) {
            <button (click)="headingSize.set(sz.value)"
                    [class]="headingSize()===sz.value
                      ? 'px-3 py-1.5 rounded-lg bg-secondary text-on-secondary text-xs font-bold transition-all shadow-md'
                      : 'px-3 py-1.5 rounded-lg bg-surface-container text-on-surface-variant hover:bg-surface-container-high text-xs font-medium transition-all'">
              {{ sz.label }}
            </button>
          }
        </div>
      </div>
    </div>
  `
})
export class PersonalizeTypographyPanelComponent {
  align = model.required<Align>();
  fontFamily = model.required<string>();
  fontSize = model.required<string>();
  headingSize = model.required<string>();

  alignButtons = ALIGN_BUTTONS;
  fonts = FONTS;
  fontSizes = FONT_SIZES;
  headingSizes = HEADING_SIZES;
}
