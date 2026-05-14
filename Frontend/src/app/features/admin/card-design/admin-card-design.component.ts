import { Component, signal, computed, inject, OnInit, ChangeDetectionStrategy, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { AdminSidebarComponent } from '../components/admin-sidebar/admin-sidebar.component';
import { environment } from '../../../../environments/environment';

// ── Interfaces ────────────────────────────────────────────────────────

interface BgItem {
  id: string;
  label: string;
  bgStyle: string;
  categories: string;      // JSON string from API
  isPremium: boolean;
  isActive: boolean;
  sortOrder: number;
}

interface DecoElement {
  emoji: string;
  top: string;
  left: string;
  rotate: string;
  size: string;
  opacity?: string;
}

interface DecorItem {
  id: string;
  label: string;
  preview: string;
  categories: string;      // JSON string
  elements: string;        // JSON string
  isActive: boolean;
  sortOrder: number;
}

const CATEGORIES = ['all','birthday','wedding','newyear','festival','graduation'];

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-admin-card-design',
  standalone: true,
  imports: [CommonModule, FormsModule, AdminSidebarComponent],
  templateUrl: './admin-card-design.component.html'
})
export class AdminCardDesignComponent implements OnInit {
  private http = inject(HttpClient);
  private destroyRef = inject(DestroyRef);
  private readonly base = environment.apiBaseUrl;

  activeTab = signal<'backgrounds' | 'decorations'>('backgrounds');

  // ── Backgrounds state ─────────────────────────────────────────────
  backgrounds = signal<BgItem[]>([]);
  bgLoading   = signal(false);
  bgSearch    = signal('');

  filteredBgs = computed(() => {
    const q = this.bgSearch().toLowerCase();
    return this.backgrounds().filter(b =>
      !q || b.label.toLowerCase().includes(q) || b.id.includes(q)
    );
  });

  bgModalOpen  = signal(false);
  bgEditMode   = signal(false);
  bgStyleMode  = signal<'upload' | 'url' | 'css'>('url');
  bgUrlInput   = signal('');   // raw URL entered by user (without url("...") wrapper)
  bgForm = signal<Partial<BgItem> & { categoriesArr: string[] }>({
    id: '', label: '', bgStyle: '', categoriesArr: ['all'],
    isPremium: false, isActive: true, sortOrder: 0
  });

  // ── Decorations state ─────────────────────────────────────────────
  decorations = signal<DecorItem[]>([]);
  decorLoading = signal(false);
  decorSearch  = signal('');

  filteredDecors = computed(() => {
    const q = this.decorSearch().toLowerCase();
    return this.decorations().filter(d =>
      !q || d.label.toLowerCase().includes(q) || d.id.includes(q)
    );
  });

  decorModalOpen = signal(false);
  decorEditMode  = signal(false);
  decorForm = signal<Partial<DecorItem> & { categoriesArr: string[]; elementsArr: DecoElement[] }>({
    id: '', label: '', preview: '', categoriesArr: ['all'],
    elementsArr: [], isActive: true, sortOrder: 0
  });

  // ── Shared ────────────────────────────────────────────────────────
  saving    = signal(false);
  toast     = signal('');
  toastOk   = signal(true);
  deleteConfirmId = signal<string | null>(null);
  deleteTarget    = signal<'bg' | 'decor' | null>(null);

  categories = CATEGORIES;

  // ── Lifecycle ─────────────────────────────────────────────────────
  ngOnInit() {
    this.loadBackgrounds();
    this.loadDecorations();
  }

  // ── Background CRUD ───────────────────────────────────────────────
  loadBackgrounds() {
    this.bgLoading.set(true);
    const token = this.getToken();
    this.http.get<any>(`${this.base}/admin/card-backgrounds`, {
      headers: { Authorization: `Bearer ${token}` }
    }).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: res => {
        this.bgLoading.set(false);
        if (res.success) this.backgrounds.set(res.data ?? []);
      },
      error: () => this.bgLoading.set(false)
    });
  }

  openCreateBg() {
    this.bgForm.set({ id: '', label: '', bgStyle: '', categoriesArr: ['all'], isPremium: false, isActive: true, sortOrder: 0 });
    this.bgStyleMode.set('url');
    this.bgUrlInput.set('');
    this.bgEditMode.set(false);
    this.bgModalOpen.set(true);
  }

  openEditBg(bg: BgItem) {
    this.bgForm.set({ ...bg, categoriesArr: this.parseCategories(bg.categories) });
    // Detect what mode the stored style is in
    const style = bg.bgStyle ?? '';
    if (style.startsWith('url("data:')) {
      this.bgStyleMode.set('upload');
      this.bgUrlInput.set('');
    } else if (style.match(/^url\("/)) {
      this.bgStyleMode.set('url');
      this.bgUrlInput.set(style.replace(/^url\("/, '').replace(/".*$/, ''));
    } else {
      this.bgStyleMode.set('css');
      this.bgUrlInput.set('');
    }
    this.bgEditMode.set(true);
    this.bgModalOpen.set(true);
  }

  onBgStyleModeChange(mode: 'upload' | 'url' | 'css') {
    this.bgStyleMode.set(mode);
    // Clear bgStyle when switching modes (except css which user types directly)
    if (mode !== 'css') {
      this.bgForm.set({ ...this.bgForm(), bgStyle: '' });
    }
    this.bgUrlInput.set('');
  }

  onBgUrlChange(url: string) {
    this.bgUrlInput.set(url);
    const trimmed = url.trim();
    if (trimmed) {
      this.bgForm.set({ ...this.bgForm(), bgStyle: `url("${trimmed}") center/cover` });
    } else {
      this.bgForm.set({ ...this.bgForm(), bgStyle: '' });
    }
  }

  onBgFileSelected(event: Event) {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (!file) return;
    if (file.size > 5 * 1024 * 1024) {
      this.showToast('File too large. Max 5MB.', false);
      return;
    }
    const reader = new FileReader();
    reader.onload = (e) => {
      const dataUrl = e.target?.result as string;
      this.bgForm.set({ ...this.bgForm(), bgStyle: `url("${dataUrl}") center/cover` });
    };
    reader.readAsDataURL(file);
  }

  saveBg() {
    const f = this.bgForm();
    if (!f.label?.trim() || !f.bgStyle?.trim()) {
      this.showToast('Label and Background Style are required.', false);
      return;
    }
    if (!f.id?.trim()) {
      this.showToast('ID is required.', false);
      return;
    }
    this.saving.set(true);
    const token = this.getToken();
    const categoriesJson = JSON.stringify(f.categoriesArr ?? ['all']);

    if (this.bgEditMode()) {
      const body = {
        label: f.label, bgStyle: f.bgStyle, categories: categoriesJson,
        isPremium: f.isPremium ?? false, isActive: f.isActive ?? true, sortOrder: f.sortOrder ?? 0
      };
      this.http.put<any>(`${this.base}/admin/card-backgrounds/${f.id}`, body, {
        headers: { Authorization: `Bearer ${token}` }
      }).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
        next: () => {
          this.saving.set(false);
          this.bgModalOpen.set(false);
          this.loadBackgrounds();
          this.showToast('Background updated.', true);
        },
        error: err => {
          this.saving.set(false);
          this.showToast(err.error?.message ?? 'Failed to update.', false);
        }
      });
    } else {
      const body = {
        id: f.id.trim().toLowerCase(), label: f.label, bgStyle: f.bgStyle,
        categories: categoriesJson, isPremium: f.isPremium ?? false, sortOrder: f.sortOrder ?? 0
      };
      this.http.post<any>(`${this.base}/admin/card-backgrounds`, body, {
        headers: { Authorization: `Bearer ${token}` }
      }).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
        next: () => {
          this.saving.set(false);
          this.bgModalOpen.set(false);
          this.loadBackgrounds();
          this.showToast('Background created.', true);
        },
        error: err => {
          this.saving.set(false);
          this.showToast(err.error?.message ?? 'Failed to create.', false);
        }
      });
    }
  }

  toggleBg(id: string) {
    const token = this.getToken();
    this.http.patch<any>(`${this.base}/admin/card-backgrounds/${id}/toggle`, {}, {
      headers: { Authorization: `Bearer ${token}` }
    }).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: res => {
        this.backgrounds.update(list =>
          list.map(b => b.id === id ? { ...b, isActive: res.data?.isActive ?? !b.isActive } : b)
        );
      }
    });
  }

  confirmDeleteBg(id: string) {
    this.deleteConfirmId.set(id);
    this.deleteTarget.set('bg');
  }

  // ── Decoration CRUD ───────────────────────────────────────────────
  loadDecorations() {
    this.decorLoading.set(true);
    const token = this.getToken();
    this.http.get<any>(`${this.base}/admin/card-decorations`, {
      headers: { Authorization: `Bearer ${token}` }
    }).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: res => {
        this.decorLoading.set(false);
        if (res.success) this.decorations.set(res.data ?? []);
      },
      error: () => this.decorLoading.set(false)
    });
  }

  openCreateDecor() {
    this.decorForm.set({
      id: '', label: '', preview: '', categoriesArr: ['all'],
      elementsArr: [], isActive: true, sortOrder: 0
    });
    this.decorEditMode.set(false);
    this.decorModalOpen.set(true);
  }

  openEditDecor(d: DecorItem) {
    let elements: DecoElement[] = [];
    try { elements = JSON.parse(d.elements); } catch { elements = []; }
    this.decorForm.set({
      ...d,
      categoriesArr: this.parseCategories(d.categories),
      elementsArr: elements
    });
    this.decorEditMode.set(true);
    this.decorModalOpen.set(true);
  }

  addElement() {
    const f = this.decorForm();
    this.decorForm.set({
      ...f,
      elementsArr: [...(f.elementsArr ?? []), { emoji: '✨', top: '50%', left: '50%', rotate: '0deg', size: '1.5rem', opacity: '1' }]
    });
  }

  removeElement(i: number) {
    const f = this.decorForm();
    const arr = [...(f.elementsArr ?? [])];
    arr.splice(i, 1);
    this.decorForm.set({ ...f, elementsArr: arr });
  }

  updateElement(i: number, field: keyof DecoElement, value: string) {
    const f = this.decorForm();
    const arr = (f.elementsArr ?? []).map((el, idx) =>
      idx === i ? { ...el, [field]: value } : el
    );
    this.decorForm.set({ ...f, elementsArr: arr });
  }

  saveDecor() {
    const f = this.decorForm();
    if (!f.label?.trim() || !f.preview?.trim()) {
      this.showToast('Label and Preview emoji are required.', false);
      return;
    }
    if (!f.id?.trim()) {
      this.showToast('ID is required.', false);
      return;
    }
    this.saving.set(true);
    const token = this.getToken();
    const categoriesJson = JSON.stringify(f.categoriesArr ?? ['all']);
    const elementsJson   = JSON.stringify(f.elementsArr ?? []);

    if (this.decorEditMode()) {
      const body = {
        label: f.label, preview: f.preview, categories: categoriesJson,
        elements: elementsJson, isActive: f.isActive ?? true, sortOrder: f.sortOrder ?? 0
      };
      this.http.put<any>(`${this.base}/admin/card-decorations/${f.id}`, body, {
        headers: { Authorization: `Bearer ${token}` }
      }).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
        next: () => {
          this.saving.set(false);
          this.decorModalOpen.set(false);
          this.loadDecorations();
          this.showToast('Decoration updated.', true);
        },
        error: err => {
          this.saving.set(false);
          this.showToast(err.error?.message ?? 'Failed to update.', false);
        }
      });
    } else {
      const body = {
        id: f.id.trim().toLowerCase(), label: f.label, preview: f.preview,
        categories: categoriesJson, elements: elementsJson, sortOrder: f.sortOrder ?? 0
      };
      this.http.post<any>(`${this.base}/admin/card-decorations`, body, {
        headers: { Authorization: `Bearer ${token}` }
      }).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
        next: () => {
          this.saving.set(false);
          this.decorModalOpen.set(false);
          this.loadDecorations();
          this.showToast('Decoration created.', true);
        },
        error: err => {
          this.saving.set(false);
          this.showToast(err.error?.message ?? 'Failed to create.', false);
        }
      });
    }
  }

  toggleDecor(id: string) {
    const token = this.getToken();
    this.http.patch<any>(`${this.base}/admin/card-decorations/${id}/toggle`, {}, {
      headers: { Authorization: `Bearer ${token}` }
    }).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: res => {
        this.decorations.update(list =>
          list.map(d => d.id === id ? { ...d, isActive: res.data?.isActive ?? !d.isActive } : d)
        );
      }
    });
  }

  confirmDeleteDecor(id: string) {
    this.deleteConfirmId.set(id);
    this.deleteTarget.set('decor');
  }

  // ── Shared delete ─────────────────────────────────────────────────
  cancelDelete() {
    this.deleteConfirmId.set(null);
    this.deleteTarget.set(null);
  }

  executeDelete() {
    const id     = this.deleteConfirmId();
    const target = this.deleteTarget();
    if (!id || !target) return;

    const token  = this.getToken();
    const url    = target === 'bg'
      ? `${this.base}/admin/card-backgrounds/${id}`
      : `${this.base}/admin/card-decorations/${id}`;

    this.http.delete<any>(url, { headers: { Authorization: `Bearer ${token}` } }).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        if (target === 'bg') this.backgrounds.update(list => list.filter(b => b.id !== id));
        else                 this.decorations.update(list => list.filter(d => d.id !== id));
        this.cancelDelete();
        this.showToast('Deleted successfully.', true);
      },
      error: err => {
        this.showToast(err.error?.message ?? 'Failed to delete.', false);
        this.cancelDelete();
      }
    });
  }

  // ── Category checkbox helpers ─────────────────────────────────────
  isCategoryChecked(cat: string, arr: string[]): boolean {
    return arr.includes(cat);
  }

  toggleCategory(cat: string, arr: string[]): string[] {
    return arr.includes(cat) ? arr.filter(c => c !== cat) : [...arr, cat];
  }

  toggleBgCategory(cat: string) {
    const f = this.bgForm();
    this.bgForm.set({ ...f, categoriesArr: this.toggleCategory(cat, f.categoriesArr ?? []) });
  }

  toggleDecorCategory(cat: string) {
    const f = this.decorForm();
    this.decorForm.set({ ...f, categoriesArr: this.toggleCategory(cat, f.categoriesArr ?? []) });
  }

  // ── Helpers ───────────────────────────────────────────────────────
  parseCategories(json: string): string[] {
    try { return JSON.parse(json); } catch { return ['all']; }
  }

  categoriesDisplay(json: string): string {
    return this.parseCategories(json).join(', ');
  }

  elementsCount(json: string): number {
    try { return JSON.parse(json).length; } catch { return 0; }
  }

  autoSlug(label: string): string {
    return label.toLowerCase().replace(/\s+/g, '-').replace(/[^a-z0-9-]/g, '');
  }

  onBgLabelChange(label: string) {
    const f = this.bgForm();
    if (!this.bgEditMode() && !f.id) {
      this.bgForm.set({ ...f, label, id: this.autoSlug(label) });
    } else {
      this.bgForm.set({ ...f, label });
    }
  }

  onDecorLabelChange(label: string) {
    const f = this.decorForm();
    if (!this.decorEditMode() && !f.id) {
      this.decorForm.set({ ...f, label, id: this.autoSlug(label) });
    } else {
      this.decorForm.set({ ...f, label });
    }
  }

  private showToast(msg: string, ok: boolean) {
    this.toast.set(msg);
    this.toastOk.set(ok);
    setTimeout(() => this.toast.set(''), 3500);
  }

  private getToken(): string {
    return localStorage.getItem('eg_token') ?? '';
  }

  getElementsArray(): DecoElement[] {
    return this.decorForm().elementsArr ?? [];
  }
}
