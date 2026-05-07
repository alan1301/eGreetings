import { Component, computed, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { AdminSidebarComponent } from '../components/admin-sidebar/admin-sidebar.component';
import { environment } from '../../../../environments/environment';
import { ApiResponse } from '../../../shared/models/api-response.model';
import { CARD_BACKGROUNDS, CARD_DECORATIONS } from '../../cards/personalize/card-themes.data';

interface CardItem {
  id: string;
  name: string;
  slug?: string;
  category?: string;      // kept for mock data fallback
  categoryName?: string;  // from real API
  categorySlug?: string;  // from real API
  categoryId?: string;    // for edit form
  status: string;
  thumbnailUrl?: string;
  description?: string;
  tags?: string;
  isFeatured?: boolean;
  isPremium?: boolean;
}

type Align = 'left' | 'center' | 'right';

interface CardDetailDto {
  id: string;
  name: string;
  slug: string;
  thumbnailUrl?: string;
  fileUrl?: string;
  description?: string;
  tags?: string;
  customJsonContent?: string;
  isFeatured: boolean;
  categoryName: string;
  categorySlug: string;
}

interface TemplateContentForm {
  heading: string;
  message: string;
  fontFamily: string;
  fontSize: string;
  headingSize: string;
  textColor: string;
  align: Align;
  bgId: string;
  decorId: string;
}

@Component({
  selector: 'app-admin-cards',
  standalone: true,
  imports: [CommonModule, AdminSidebarComponent, FormsModule],
  templateUrl: './admin-cards.component.html'
})
export class AdminCardsComponent implements OnInit {
  private http = inject(HttpClient);
  private readonly base = environment.apiBaseUrl;

  showAddModal = signal(false);
  editingCard = signal<CardItem | null>(null);
  deleteTarget = signal<CardItem | null>(null);
  deleteError = signal('');
  deleting = signal(false);
  saving = signal(false);
  formError = signal('');
  previewError = signal(false);
  imgErrors = signal<Record<string, boolean>>({});
  imageTab = signal<'url' | 'file'>('url');
  isDragging = signal(false);
  uploadFileName = signal('');

  onImgError(cardId: string) {
    this.imgErrors.update(prev => ({ ...prev, [cardId]: true }));
  }

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (file) this.readFile(file);
  }

  onFileDrop(event: DragEvent) {
    event.preventDefault();
    this.isDragging.set(false);
    const file = event.dataTransfer?.files?.[0];
    if (file && file.type.startsWith('image/')) this.readFile(file);
  }

  private readFile(file: File) {
    if (file.size > 5 * 1024 * 1024) {
      this.formError.set('File too large. Max size is 5 MB.');
      return;
    }
    this.uploadFileName.set(file.name);
    const reader = new FileReader();
    reader.onload = (e) => {
      this.cardForm.thumbnailUrl = e.target?.result as string;
      this.previewError.set(false);
    };
    reader.readAsDataURL(file);
  }

  clearFile() {
    this.uploadFileName.set('');
    this.cardForm.thumbnailUrl = '';
    this.previewError.set(false);
  }

  searchQuery = '';
  cards = signal<CardItem[]>([]);
  displayedCards = signal<CardItem[]>([]);
  categories = signal<any[]>([]);
  backgrounds = CARD_BACKGROUNDS.filter(bg => !bg.isPremium);
  decorations = CARD_DECORATIONS;
  fontOptions = [
    { label: 'Noto Serif', value: "'Noto Serif', serif" },
    { label: 'Manrope', value: "'Manrope', sans-serif" },
    { label: 'Georgia', value: 'Georgia, serif' },
    { label: 'Palatino', value: "'Palatino Linotype', serif" },
    { label: 'Courier New', value: "'Courier New', monospace" },
  ];
  messageSizeOptions = ['14px', '18px', '24px', '32px', '42px'];
  headingSizeOptions = ['32px', '42px', '56px'];
  alignOptions: Align[] = ['left', 'center', 'right'];
  selectedBackground = computed(() =>
    this.backgrounds.find(bg => bg.id === this.templateForm.bgId) ?? null
  );
  selectedDecoration = computed(() =>
    this.decorations.find(decor => decor.id === this.templateForm.decorId) ?? this.decorations[0]
  );

  cardForm = {
    name: '',
    categoryId: '',
    description: '',
    tags: '',
    thumbnailUrl: '',
    fileUrl: '',
    isPremium: true,
    isFeatured: false
  };

  templateForm: TemplateContentForm = this.createEmptyTemplateForm();

  mockCards: CardItem[] = [
    { id: '1', name: 'Spring Gold', category: 'Birthday', status: 'Active' },
    { id: '2', name: 'Eternal Meadow', category: 'Wedding', status: 'Active' },
    { id: '3', name: 'Quiet Willow', category: 'Festival', status: 'Inactive' },
    { id: '4', name: 'New Year Sparkle', category: 'New Year', status: 'Active' },
    { id: '5', name: 'Purple Petals', category: 'Birthday', status: 'Active' },
    { id: '6', name: 'Classic Letter', category: 'Wedding', status: 'Active' },
  ];

  ngOnInit() {
    this.loadCards();
    this.loadCategories();
  }

  loadCards() {
    this.http.get<ApiResponse<any>>(`${this.base}/cards?pageSize=100`).subscribe({
      next: (res) => {
        // use /cards (public API) as fallback if /admin/cards does not exist
        if (res.success && res.data?.items?.length) {
          this.cards.set(res.data.items);
          this.displayedCards.set(res.data.items);
        } else if (res.success && Array.isArray(res.data) && res.data.length) {
          this.cards.set(res.data);
          this.displayedCards.set(res.data);
        } else {
          this.displayedCards.set(this.mockCards);
        }
      },
      error: () => this.displayedCards.set(this.mockCards)
    });
  }

  loadCategories() {
    this.http.get<ApiResponse<any[]>>(`${this.base}/categories`).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.categories.set(res.data);
        }
      }
    });
  }

  onSearch() {
    const q = this.searchQuery.toLowerCase();
    const source = this.cards().length ? this.cards() : this.mockCards;
    this.displayedCards.set(source.filter(c =>
      c.name.toLowerCase().includes(q) || (c.category && c.category.toLowerCase().includes(q))
    ));
  }

  openAdd() {
    this.editingCard.set(null);
    this.cardForm = { name: '', categoryId: this.categories()[0]?.id || '', description: '', tags: '', thumbnailUrl: '', fileUrl: '', isPremium: true, isFeatured: false };
    this.templateForm = this.createEmptyTemplateForm();
    this.formError.set('');
    this.previewError.set(false);
    this.imageTab.set('url');
    this.uploadFileName.set('');
    this.showAddModal.set(true);
  }

  openEdit(card: CardItem) {
    this.editingCard.set(card);
    let resolvedCategoryId = card.categoryId || '';
    if (!resolvedCategoryId && card.categorySlug) {
      const match = this.categories().find(c => c.slug === card.categorySlug);
      if (match) resolvedCategoryId = match.id;
    }
    if (!resolvedCategoryId) resolvedCategoryId = this.categories()[0]?.id || '';

    this.cardForm = {
      name: card.name,
      categoryId: resolvedCategoryId,
      description: card.description || '',
      tags: card.tags || '',
      thumbnailUrl: card.thumbnailUrl || '',
      fileUrl: '',
      isPremium: card.isPremium ?? true,
      isFeatured: card.isFeatured || false
    };
    this.formError.set('');
    this.previewError.set(false);
    this.imageTab.set('url');
    this.uploadFileName.set('');
    this.showAddModal.set(true);
    this.loadTemplateContent(card.id);
  }

  private loadTemplateContent(cardId: string) {
    this.http.get<ApiResponse<CardDetailDto>>(`${this.base}/cards/${cardId}`).subscribe({
      next: (res) => {
        if (!res.success || !res.data?.customJsonContent) {
          this.templateForm = this.createEmptyTemplateForm();
          return;
        }

        this.templateForm = this.parseTemplateContent(res.data.customJsonContent);
      },
      error: () => {
        this.templateForm = this.createEmptyTemplateForm();
      }
    });
  }

  closeModal() {
    this.showAddModal.set(false);
  }

  saveCard() {
    if (!this.cardForm.name || !this.cardForm.categoryId) {
      this.formError.set('Name and Category are required');
      return;
    }
    
    this.saving.set(true);
    this.formError.set('');

    const isEdit = !!this.editingCard();
    const url = isEdit ? `${this.base}/admin/cards/${this.editingCard()?.id}` : `${this.base}/admin/cards`;
    const method = isEdit ? 'put' : 'post';

    const payload = {
      ...this.cardForm,
      customJsonContent: JSON.stringify(this.templateForm)
    };

    this.http.request<ApiResponse<any>>(method, url, { body: payload }).subscribe({
      next: (res) => {
        this.saving.set(false);
        if (res.success) {
          this.closeModal();
          this.loadCards(); // refresh
        } else {
          this.formError.set(res.message || 'Failed to save card');
        }
      },
      error: (err) => {
        this.saving.set(false);
        this.formError.set(err.error?.message || err.error?.errors?.[0]?.message || 'An error occurred');
      }
    });
  }

  confirmDelete(card: CardItem) { 
    this.deleteTarget.set(card); 
    this.deleteError.set('');
  }

  cancelDelete() {
    this.deleteTarget.set(null);
  }

  executeDelete() {
    const card = this.deleteTarget();
    if (!card) return;
    
    this.deleting.set(true);
    this.deleteError.set('');

    this.http.delete<ApiResponse<any>>(`${this.base}/admin/cards/${card.id}`).subscribe({
      next: (res) => {
        this.deleting.set(false);
        if (res.success) {
          this.cancelDelete();
          this.loadCards();
        } else {
          this.deleteError.set(res.message || 'Failed to delete card');
        }
      },
      error: (err) => {
        this.deleting.set(false);
        this.deleteError.set(err.error?.message || err.error?.errors?.[0]?.message || 'Cannot delete card. It may have transactions.');
      }
    });
  }

  archiveCard(card: CardItem) {
    this.deleting.set(true);
    this.deleteError.set('');
    
    this.http.patch<ApiResponse<any>>(`${this.base}/admin/cards/${card.id}/archive`, {}).subscribe({
      next: (res) => {
        this.deleting.set(false);
        if (res.success) {
          this.cancelDelete();
          this.loadCards();
        } else {
          this.deleteError.set(res.message || 'Failed to archive card');
        }
      },
      error: (err) => {
        this.deleting.set(false);
        this.deleteError.set(err.error?.message || 'Failed to archive card');
      }
    });
  }

  getRandomViews() { return (Math.floor(Math.random() * 2000) + 100).toLocaleString(); }

  templateMessageAlignClass() {
    if (this.templateForm.align === 'left') return 'text-left';
    if (this.templateForm.align === 'right') return 'text-right';
    return 'text-center';
  }

  private createEmptyTemplateForm(): TemplateContentForm {
    return {
      heading: '',
      message: '',
      fontFamily: "'Noto Serif', serif",
      fontSize: '18px',
      headingSize: '42px',
      textColor: '#383221',
      align: 'center',
      bgId: '',
      decorId: 'none'
    };
  }

  private parseTemplateContent(raw: string): TemplateContentForm {
    try {
      const parsed = JSON.parse(raw);
      return {
        heading: parsed.heading ?? '',
        message: parsed.message ?? '',
        fontFamily: parsed.fontFamily ?? "'Noto Serif', serif",
        fontSize: parsed.fontSize ?? '18px',
        headingSize: parsed.headingSize ?? '42px',
        textColor: parsed.textColor ?? '#383221',
        align: parsed.align ?? 'center',
        bgId: parsed.bgId ?? '',
        decorId: parsed.decorId ?? 'none'
      };
    } catch {
      return this.createEmptyTemplateForm();
    }
  }
}
