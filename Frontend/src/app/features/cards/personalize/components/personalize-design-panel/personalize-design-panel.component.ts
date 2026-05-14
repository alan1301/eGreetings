import { ChangeDetectionStrategy, Component, computed, input, model, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardBackground, CardDecoration, BG_CATEGORIES } from '../../card-themes.data';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-personalize-design-panel',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="flex gap-1 p-1 bg-surface-container rounded-xl mb-4">
      <button (click)="!isTemplateMode() && designTab.set('bg')"
              [disabled]="isTemplateMode()"
              [title]="isTemplateMode() ? 'Background is fixed for collection templates' : ''"
              [class]="designTab()==='bg' && !isTemplateMode()
                ? 'flex-1 py-2 rounded-lg bg-surface text-on-surface font-bold text-xs font-label shadow-sm transition-all flex items-center justify-center gap-1'
                : isTemplateMode()
                  ? 'flex-1 py-2 rounded-lg text-on-surface-variant/40 text-xs font-label cursor-not-allowed flex items-center justify-center gap-1'
                  : 'flex-1 py-2 rounded-lg text-on-surface-variant text-xs font-label hover:text-on-surface transition-all flex items-center justify-center gap-1'">
        🖼 Background
        @if (isTemplateMode()) {
          <span class="material-symbols-outlined text-sm leading-none">lock</span>
        }
      </button>
      <button (click)="designTab.set('decor')"
              [class]="designTab()==='decor'
                ? 'flex-1 py-2 rounded-lg bg-surface text-on-surface font-bold text-xs font-label shadow-sm transition-all'
                : 'flex-1 py-2 rounded-lg text-on-surface-variant text-xs font-label hover:text-on-surface transition-all'">
        ✨ Decorations
      </button>
    </div>

    @if (isTemplateMode() && designTab() === 'bg') {
      <div class="flex items-start gap-2 p-3 rounded-lg bg-surface-container border border-outline-variant/30 mb-3">
        <span class="material-symbols-outlined text-on-surface-variant text-base mt-0.5">info</span>
        <p class="text-xs text-on-surface-variant leading-relaxed">
          Background is part of this template's design and cannot be changed.
          Switch to <span class="font-bold text-on-surface">Free Design</span> mode to customize backgrounds.
        </p>
      </div>
    }

    @if (designTab() === 'bg' && !isTemplateMode()) {
      <div class="flex gap-2 flex-wrap mb-3">
        @for (cat of bgCategories; track cat.id) {
          <button (click)="bgCat.set(cat.id)"
                  [class]="bgCat()===cat.id
                    ? 'px-3 py-1 rounded-full bg-primary text-on-primary text-xs font-bold font-label'
                    : 'px-3 py-1 rounded-full bg-surface-container text-on-surface-variant text-xs font-label hover:bg-surface-container-high'">
            {{ cat.label }}
          </button>
        }
      </div>
      <div class="grid grid-cols-5 gap-2">
        @for (bg of filteredBgs(); track bg.id) {
          <button (click)="!bg.isPremium && bgSelect.emit(bg)"
                  [title]="bg.label + (bg.isPremium ? ' (Premium)' : '')"
                  class="relative aspect-square rounded-lg overflow-hidden border-2 transition-all group"
                  [class]="selectedBg()?.id === bg.id ? 'border-primary scale-105 shadow-md' : 'border-transparent hover:border-outline-variant hover:scale-105'">
            <div class="w-full h-full" [style.background]="bg.bg"></div>
            @if (bg.isPremium) {
              <div class="absolute inset-0 bg-on-surface/40 flex items-center justify-center">
                <span class="material-symbols-outlined text-white text-sm">lock</span>
              </div>
            }
            @if (selectedBg()?.id === bg.id) {
              <div class="absolute inset-0 flex items-center justify-center">
                <span class="material-symbols-outlined text-white text-base drop-shadow">check_circle</span>
              </div>
            }
            <div class="absolute bottom-0 inset-x-0 bg-on-surface/60 text-white text-[9px] font-bold py-0.5 px-1 text-center opacity-0 group-hover:opacity-100 transition-opacity truncate">
              {{ bg.label }}
            </div>
          </button>
        }
      </div>
    }

    @if (designTab() === 'decor') {
      <div class="grid grid-cols-4 gap-2">
        @for (d of filteredDecors(); track d.id) {
          <button (click)="decorSelect.emit(d.id==='none' ? null : d)"
                  [class]="(d.id==='none' ? !selectedDecor() : selectedDecor()?.id===d.id)
                    ? 'p-2 rounded-xl border-2 border-primary bg-primary/5 flex flex-col items-center gap-1 transition-all shadow-md'
                    : 'p-2 rounded-xl border-2 border-transparent bg-surface-container hover:border-outline-variant flex flex-col items-center gap-1 transition-all'">
            <span class="text-xl">{{ d.preview }}</span>
            <span class="text-[9px] font-bold font-label text-on-surface-variant text-center leading-tight">{{ d.label }}</span>
          </button>
        }
      </div>
    }
  `
})
export class PersonalizeDesignPanelComponent {
  isTemplateMode = input.required<boolean>();
  allBackgrounds = input.required<CardBackground[]>();
  allDecorations = input.required<CardDecoration[]>();
  selectedBg = input<CardBackground | null>(null);
  selectedDecor = input<CardDecoration | null>(null);

  designTab = model<'bg' | 'decor'>('bg');
  bgCat = model<string>('all');

  bgSelect = output<CardBackground>();
  decorSelect = output<CardDecoration | null>();

  bgCategories = BG_CATEGORIES;

  filteredBgs = computed(() => {
    const cat = this.bgCat();
    return this.allBackgrounds().filter(b =>
      cat === 'all' ? true : b.categories.includes(cat) || b.categories.includes('all')
    );
  });

  filteredDecors = computed(() =>
    this.allDecorations().filter(d =>
      d.categories.includes('all') || d.categories.includes(this.bgCat())
    )
  );
}
