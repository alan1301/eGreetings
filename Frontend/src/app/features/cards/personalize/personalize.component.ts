import { Component, signal, computed, inject, OnInit, OnDestroy, HostListener, ViewChild, ElementRef } from '@angular/core';
import { RouterLink, ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { NavbarComponent } from '../../../shared/components/navbar/navbar.component';
import { FooterComponent } from '../../../shared/components/footer/footer.component';
import { ColorWheelComponent } from './components/color-wheel/color-wheel.component';
import { AuthService } from '../../../core/services/auth.service';
import { SubscriptionService } from '../../../core/services/subscription.service';
import { CARD_BACKGROUNDS, CARD_DECORATIONS, BG_CATEGORIES, CardBackground, CardDecoration } from './card-themes.data';
import { environment } from '../../../../environments/environment';
import { ApiResponse } from '../../../shared/models/api-response.model';

export interface DraggableElement {
  id: string;
  type: 'emoji' | 'image';
  content: string; 
  top: number; 
  left: number; 
  scale: number;
  rotate: number;
  baseSize?: string;
  opacity?: string;
}

type Align = 'left' | 'center' | 'right';

interface CardTemplateContent {
  heading?: string;
  message?: string;
  fontFamily?: string;
  fontSize?: string;
  headingSize?: string;
  textColor?: string;
  align?: Align;
  bgId?: string | null;
  decorId?: string | null;
}

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
  isPremium: boolean;
  categoryName: string;
  categorySlug: string;
}

const FONTS = [
  { label: 'Noto Serif (Default)', value: "'Noto Serif', serif" },
  { label: 'Manrope (Modern)',    value: "'Manrope', sans-serif" },
  { label: 'Georgia (Classic)',     value: 'Georgia, serif' },
  { label: 'Palatino (Elegant)', value: "'Palatino Linotype', serif" },
  { label: 'Courier New (Typewriter)', value: "'Courier New', monospace" },
];

const FONT_SIZES = [
  { label: 'Small',    value: '14px' },
  { label: 'Medium',    value: '18px' },
  { label: 'Large',    value: '24px' },
  { label: 'X-Large', value: '32px' },
  { label: 'Huge', value: '42px' },
];

// Color palette grid: mỗi row là 1 màu, mỗi cột là 1 shade (từ sáng → tối)
const COLOR_PALETTE: { label: string; shades: string[] }[] = [
  { label: 'Black/Gray',  shades: ['#f5f5f5','#d4d4d4','#a3a3a3','#737373','#525252','#404040','#262626','#171717','#0a0a0a'] },
  { label: 'Cream',  shades: ['#fdf8f0','#f5e8c7','#e6c88a','#c9a44e','#a07830','#7c5900','#5c4000','#3d2900','#1e1400'] },
  { label: 'Teal',     shades: ['#e0f7f7','#b2ebec','#7dd8da','#4ec0c2','#32a4a6','#26686a','#1a5052','#0f3a3b','#071e1f'] },
  { label: 'Brown',  shades: ['#fdf3ea','#f5d9bb','#e8b88a','#d49260','#b07240','#81552e','#623e20','#432814','#241408'] },
  { label: 'Red',       shades: ['#fdf0ee','#f8cdc7','#f09a8e','#e56a5a','#cc4430','#a73b21','#852d18','#621f0f','#3d1008'] },
  { label: 'Blue', shades: ['#eff6ff','#bfdbfe','#93c5fd','#60a5fa','#3b82f6','#2563eb','#1d4ed8','#1e40af','#1e3a8a'] },
  { label: 'Purple',      shades: ['#faf5ff','#e9d5ff','#d8b4fe','#c084fc','#a855f7','#9333ea','#7e22ce','#6b21a8','#4c1d95'] },
  { label: 'Pink',     shades: ['#fdf2f8','#fce7f3','#fbcfe8','#f9a8d4','#f472b6','#ec4899','#db2777','#be185d','#9d174d'] },
  { label: 'Green',  shades: ['#f0fdf4','#bbf7d0','#86efac','#4ade80','#22c55e','#16a34a','#15803d','#166534','#14532d'] },
  { label: 'Orange',      shades: ['#fff7ed','#fed7aa','#fdba74','#fb923c','#f97316','#ea580c','#c2410c','#9a3412','#7c2d12'] },
  { label: 'Yellow',     shades: ['#fefce8','#fef9c3','#fef08a','#fde047','#facc15','#eab308','#ca8a04','#a16207','#713f12'] },
  { label: 'Cyan', shades: ['#f0fdfa','#ccfbf1','#99f6e4','#5eead4','#2dd4bf','#14b8a6','#0d9488','#0f766e','#134e4a'] },
];

@Component({
  selector: 'app-personalize',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule, NavbarComponent, FooterComponent, ColorWheelComponent],
  styleUrl: './personalize.component.css',
  templateUrl: './personalize.component.html'
})
export class PersonalizeComponent implements OnInit, OnDestroy {
  private route  = inject(ActivatedRoute);
  private router = inject(Router);
  private http   = inject(HttpClient);
  private auth   = inject(AuthService);
  private subscriptionService = inject(SubscriptionService);
  private readonly base = environment.apiBaseUrl;

  @ViewChild('previewContainer') previewContainer!: ElementRef<HTMLDivElement>;

  // ── Interactive Elements ──
  elements = signal<DraggableElement[]>([]);
  activeElementId = signal<string | null>(null);

  // Drag state
  private isDragging = false;
  private isResizing = false;
  private startX = 0;
  private startY = 0;
  private startTop = 0;
  private startLeft = 0;
  private startScale = 1;

  // Card ID from route
  cardId = '';
  // Draft tracking: ID của draft hiện tại (sau lần POST đầu tiên)
  draftId = signal<string | null>(null);
  private autoSaveIntervalId: ReturnType<typeof setInterval> | null = null;
  hasSubscription = signal(false);
  composerMode = signal<'template' | 'fresh'>('template');
  cardLoading = signal(false);
  cardLoadError = signal('');
  pendingTemplateCheck = signal(false);
  subscriptionResolved = signal(false);
  templateRequiresPremium = signal(false);

  ngOnInit(): void {
    this.cardId = this.route.snapshot.paramMap.get('id') ?? '';
    this.composerMode.set(this.route.snapshot.queryParamMap.get('mode') === 'fresh' ? 'fresh' : 'template');

    if (!this.cardId) {
      this.resolveDefaultCard();
    } else if (this.composerMode() === 'template') {
      this.loadCardTemplate(this.cardId);
    }
    
    // Check subscription status
    this.subscriptionService.getCurrentSubscription().subscribe({
      next: (res) => {
        if (res.data && res.data.status === 'Active') {
          this.hasSubscription.set(true);
        } else {
          this.checkLocalSubscription();
        }
        this.subscriptionResolved.set(true);
        this.enforceTemplateAccess();
      },
      error: () => {
        this.checkLocalSubscription();
        this.subscriptionResolved.set(true);
        this.enforceTemplateAccess();
      }
    });
  }

  ngOnDestroy(): void {
    this.stopAutoSave();
  }

  // ── Auto-save interval (BR-09) ──────────────────────────────
  private startAutoSave(): void {
    if (this.autoSaveIntervalId !== null) return; // already running
    this.autoSaveIntervalId = setInterval(() => {
      this.autoSaveDraft();
    }, 30_000); // 30 seconds
  }

  private stopAutoSave(): void {
    if (this.autoSaveIntervalId !== null) {
      clearInterval(this.autoSaveIntervalId);
      this.autoSaveIntervalId = null;
    }
  }

  /** Silent auto-save: chỉ gọi PATCH nếu đã có draftId. Không hiện toast. */
  private autoSaveDraft(): void {
    const id = this.draftId();
    if (!id || !this.auth.getToken()) return;

    const token = this.auth.getToken();
    const body = {
      personalMessage:   this.message() || null,
      customJsonContent: this.buildCustomJson()
    };
    this.http.patch<any>(`${this.base}/drafts/${id}`, body, {
      headers: { Authorization: `Bearer ${token}` }
    }).subscribe(); // fire-and-forget, errors silently ignored
  }

  private buildCustomJson(): string {
    return JSON.stringify({
      heading:     this.heading(),
      message:     this.message(),
      fontFamily:  this.fontFamily(),
      fontSize:    this.fontSize(),
      headingSize: this.headingSize(),
      textColor:   this.textColor(),
      align:       this.align(),
      bgId:        this.selectedBg()?.id ?? null,
      decorId:     this.selectedDecor()?.id ?? null,
    });
  }

  private resolveDefaultCard() {
    this.cardLoading.set(true);
    this.cardLoadError.set('');

    this.http.get<ApiResponse<any>>(`${this.base}/cards?pageSize=1&sort=featured`).subscribe({
      next: (res) => {
        this.cardLoading.set(false);
        const items = Array.isArray(res.data) ? res.data : (res.data?.items ?? []);
        const fallbackCard = items[0];

        if (!res.success || !fallbackCard?.id) {
          this.cardLoadError.set('Unable to initialize a new design right now.');
          return;
        }

        this.cardId = fallbackCard.id;
      },
      error: () => {
        this.cardLoading.set(false);
        this.cardLoadError.set('Unable to initialize a new design right now.');
      }
    });
  }

  private loadCardTemplate(id: string) {
    this.pendingTemplateCheck.set(true);
    this.http.get<ApiResponse<CardDetailDto>>(`${this.base}/cards/${id}`).subscribe({
      next: (res) => {
        this.pendingTemplateCheck.set(false);
        this.templateRequiresPremium.set(!!res.data?.isPremium);
        this.enforceTemplateAccess();

        if (res.success && res.data?.customJsonContent) {
          this.applyTemplateContent(res.data.customJsonContent);
        }
      },
      error: () => {
        this.pendingTemplateCheck.set(false);
      }
    });
  }

  private enforceTemplateAccess() {
    if (!this.subscriptionResolved() || !this.templateRequiresPremium() || this.hasSubscription() || this.composerMode() !== 'template' || !this.cardId) {
      return;
    }

    this.router.navigate(['/cards', this.cardId], {
      queryParams: { premium: 'required' }
    });
  }

  private applyTemplateContent(rawContent: string) {
    try {
      const content = JSON.parse(rawContent) as CardTemplateContent;
      this.heading.set(content.heading ?? '');
      this.message.set(content.message ?? '');
      this.fontFamily.set(content.fontFamily ?? FONTS[0].value);
      this.fontSize.set(content.fontSize ?? '18px');
      this.headingSize.set(content.headingSize ?? '42px');
      this.textColor.set(content.textColor ?? '#383221');
      this.hexInput.set((content.textColor ?? '#383221').replace('#', ''));
      this.align.set(content.align ?? 'center');

      const selectedBackground = CARD_BACKGROUNDS.find(bg => bg.id === content.bgId) ?? null;
      this.selectedBg.set(selectedBackground);

      const selectedDecoration = CARD_DECORATIONS.find(decor => decor.id === content.decorId) ?? null;
      this.selectDecor(selectedDecoration);
    } catch {
      // Ignore malformed template content and keep editor defaults.
    }
  }

  private checkLocalSubscription() {
    const stored = localStorage.getItem('eg_subscription');
    if (stored) {
      try {
        const data = JSON.parse(stored);
        this.hasSubscription.set(true);
      } catch (e) {
        console.error('Error parsing stored subscription:', e);
      }
    }
  }

  // ── Accordion / Dropdown ──
  activePanel   = signal<string>('design');
  dropdownOpen  = signal(false);

  currentPanel = computed(() =>
    this.accordionPanels.find(p => p.id === this.activePanel())
    ?? this.accordionPanels[0]
  );

  accordionPanels = [

    { id: 'design',     icon: 'style',        label: 'Card Design',  summary: 'Background, decorations' },
    { id: 'content',    icon: 'edit_note',     label: 'Content',          summary: 'Heading, message' },
    { id: 'typography', icon: 'text_fields',   label: 'Typography',           summary: 'Font, size, align' },
    { id: 'color',      icon: 'palette',       label: 'Color',            summary: 'Color wheel RGB' },
    { id: 'photo',      icon: 'add_a_photo',   label: 'Personal Photo',         summary: 'Upload your photo', premium: true },
    { id: 'schedule',   icon: 'schedule',      label: 'Schedule',       summary: 'Choose sending time' },
  ];

  selectPanel(id: string) {
    this.activePanel.set(id);
    this.dropdownOpen.set(false);
  }

  togglePanel(id: string) {
    this.activePanel.set(this.activePanel() === id ? '' : id);
  }


  // ── Card Design ──
  bgCategories   = BG_CATEGORIES;
  designTab      = signal<'bg' | 'decor'>('bg');
  bgCat          = signal('all');
  selectedBg     = signal<CardBackground | null>(null);
  selectedDecor  = signal<CardDecoration | null>(null);

  filteredBgs = computed(() => {
    const cat = this.bgCat();
    return CARD_BACKGROUNDS.filter(b =>
      cat === 'all' ? true : b.categories.includes(cat) || b.categories.includes('all')
    );
  });

  filteredDecors = computed(() =>
    CARD_DECORATIONS.filter(d =>
      d.categories.includes('all') || d.categories.includes(this.bgCat())
    )
  );

  selectDecor(d: CardDecoration | null) {
    this.selectedDecor.set(d);
    this.activeElementId.set(null);
    
    // Remove all emojis first
    this.elements.update(arr => arr.filter(e => e.type !== 'emoji'));
    
    if (d && d.id !== 'none') {
      const newEmojis: DraggableElement[] = d.elements.map((el, i) => {
        return {
          id: `emoji_${Date.now()}_${i}`,
          type: 'emoji',
          content: el.emoji,
          top: parseFloat(el.top) || 0,
          left: parseFloat(el.left) || 0,
          scale: 1,
          rotate: parseFloat(el.rotate) || 0,
          baseSize: el.size,
          opacity: el.opacity
        };
      });
      this.elements.update(arr => [...arr, ...newEmojis]);
    }
  }

  // ── File Upload ──
  onPhotoSelected(event: any) {
    const file = event.target.files[0];
    if (!file) return;
    
    if (file.size > 5 * 1024 * 1024) {
      this.toastMsg.set('File is too large (max 5MB)');
      this.toastSuccess.set(false);
      setTimeout(() => this.toastMsg.set(''), 3000);
      return;
    }

    const reader = new FileReader();
    reader.onload = (e) => {
      const result = e.target?.result as string;
      if (result) {
        const newId = `img_${Date.now()}`;
        this.elements.update(arr => [
          ...arr,
          {
            id: newId,
            type: 'image',
            content: result,
            top: 50,
            left: 50,
            scale: 1,
            rotate: 0
          }
        ]);
        this.activeElementId.set(newId);
        this.selectPanel('design'); // Switch to design tab so user sees the preview clearly
      }
    };
    reader.readAsDataURL(file);
    event.target.value = ''; 
  }

  // ── Drag & Drop & Resize Logic ──
  onElementPointerDown(event: MouseEvent | TouchEvent, id: string) {
    event.preventDefault();
    event.stopPropagation();
    this.activeElementId.set(id);
    this.isDragging = true;
    this.isResizing = false;
    
    const clientX = 'touches' in event ? event.touches[0].clientX : (event as MouseEvent).clientX;
    const clientY = 'touches' in event ? event.touches[0].clientY : (event as MouseEvent).clientY;
    
    this.startX = clientX;
    this.startY = clientY;
    
    const el = this.elements().find(e => e.id === id);
    if (el) {
      this.startTop = el.top;
      this.startLeft = el.left;
    }
  }

  onResizePointerDown(event: MouseEvent | TouchEvent, id: string) {
    event.preventDefault();
    event.stopPropagation();
    this.activeElementId.set(id);
    this.isDragging = false;
    this.isResizing = true;
    
    const clientX = 'touches' in event ? event.touches[0].clientX : (event as MouseEvent).clientX;
    const clientY = 'touches' in event ? event.touches[0].clientY : (event as MouseEvent).clientY;
    
    this.startX = clientX;
    this.startY = clientY;
    
    const el = this.elements().find(e => e.id === id);
    if (el) {
      this.startScale = el.scale;
    }
  }

  onBgPointerDown(event: MouseEvent | TouchEvent) {
    this.activeElementId.set(null);
  }

  @HostListener('document:mousemove', ['$event'])
  @HostListener('document:touchmove', ['$event'])
  onPointerMove(event: MouseEvent | TouchEvent) {
    if (!this.isDragging && !this.isResizing) return;
    if (!this.activeElementId()) return;

    const clientX = 'touches' in event ? event.touches[0].clientX : (event as MouseEvent).clientX;
    const clientY = 'touches' in event ? event.touches[0].clientY : (event as MouseEvent).clientY;
    
    const dx = clientX - this.startX;
    const dy = clientY - this.startY;

    if (this.isDragging) {
      if (!this.previewContainer) return;
      const rect = this.previewContainer.nativeElement.getBoundingClientRect();
      const dxPercent = (dx / rect.width) * 100;
      const dyPercent = (dy / rect.height) * 100;

      this.elements.update(arr => arr.map(el => {
        if (el.id === this.activeElementId()) {
          return {
            ...el,
            left: this.startLeft + dxPercent,
            top: this.startTop + dyPercent
          };
        }
        return el;
      }));
    } else if (this.isResizing) {
      const scaleDelta = (dx + dy) * 0.005; 
      this.elements.update(arr => arr.map(el => {
        if (el.id === this.activeElementId()) {
          return {
            ...el,
            scale: Math.max(0.2, this.startScale + scaleDelta)
          };
        }
        return el;
      }));
    }
  }

  @HostListener('document:mouseup')
  @HostListener('document:touchend')
  onPointerUp() {
    this.isDragging = false;
    this.isResizing = false;
  }

  removeElement(id: string) {
    this.elements.update(arr => arr.filter(e => e.id !== id));
    if (this.activeElementId() === id) {
      this.activeElementId.set(null);
    }
  }

  heading  = signal('');
  message  = signal('');


  // ── Alignment ──
  align = signal<Align>('center');
  alignButtons = [
    { value: 'left'   as Align, icon: 'format_align_left',   label: 'Align Left' },
    { value: 'center' as Align, icon: 'format_align_center', label: 'Align Center' },
    { value: 'right'  as Align, icon: 'format_align_right',  label: 'Align Right' },
  ];

  // ── Font ──
  fonts = FONTS;
  fontFamily    = signal(FONTS[0].value);

  // ── Font sizes ──
  fontSizes    = FONT_SIZES;
  fontSize     = signal('18px');

  headingSizes = [
    { label: 'Medium',    value: '32px' },
    { label: 'Large',    value: '42px' },
    { label: 'X-Large', value: '56px' },
  ];
  headingSize = signal('42px');

  // ── Color ──
  colorPalette = COLOR_PALETTE;
  textColor = signal('#383221');
  hexInput  = signal('383221');

  setColor(hex: string) {
    this.textColor.set(hex);
    this.hexInput.set(hex.replace('#', ''));
  }

  onColorInput(event: Event) {
    const hex = (event.target as HTMLInputElement).value;
    this.setColor(hex);
  }

  onHexInput(event: Event) {
    const val = (event.target as HTMLInputElement).value.replace('#', '');
    this.hexInput.set(val);
  }

  commitHex() {
    const hex = this.hexInput();
    if (/^[0-9a-fA-F]{6}$/.test(hex)) this.setColor('#' + hex);
  }

  private hexToRgb(hex: string): [number, number, number] {
    const clean = hex.replace('#', '');
    const n = parseInt(clean, 16);
    return [(n >> 16) & 255, (n >> 8) & 255, n & 255];
  }

  private rgbToHex(r: number, g: number, b: number): string {
    return '#' + [r, g, b].map(v => v.toString(16).padStart(2, '0')).join('');
  }

  // ── Schedule ──
  scheduleEnabled = signal(false);
  scheduledAt     = '';
  scheduleError   = signal('');
  minDateTime = new Date(Date.now() + 5 * 60 * 1000).toISOString().slice(0, 16);

  dateLabel = new Date().toLocaleDateString('en-US', {
    weekday: 'long', day: '2-digit', month: '2-digit', year: 'numeric'
  }).toUpperCase();

  // ── Send modal state ──────────────────────────────────
  sendDialogOpen   = signal(false);
  contacts         = signal<{id:string;name:string;email:string}[]>([]);
  selectedContact  = signal<{id:string;name:string;email:string}|null>(null);
  recipientEmail   = '';
  subject          = '';
  sending          = signal(false);
  savingDraft      = signal(false);
  toastMsg         = signal('');
  toastSuccess     = signal(true);
  sendError        = signal('');

  isValidEmail(email: string): boolean {
    return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
  }

  selectContact(c: {id:string;name:string;email:string}): void {
    this.selectedContact.set(c);
    this.recipientEmail = c.email;
  }

  openSendDialog(): void {
    if (this.cardLoading() || this.pendingTemplateCheck()) {
      this.showToast('Please wait while the card editor finishes loading.', false);
      return;
    }
    if (!this.cardId) {
      this.showToast(this.cardLoadError() || 'No card template is available for this design yet.', false);
      return;
    }
    if (!this.auth.getToken()) {
      this.showToast('Please log in to send a card.', false);
      return;
    }
    this.selectedContact.set(null);
    this.recipientEmail = '';
    this.subject        = '';
    this.sendError.set('');
    // Load contacts for quick-pick
    const token = this.auth.getToken()!;
    this.http.get<any>(`${this.base}/contacts`, {
      headers: { Authorization: `Bearer ${token}` }
    }).subscribe({
      next: res => {
        if (res.success && Array.isArray(res.data)) {
          this.contacts.set(res.data.map((c: any) => ({ id: c.id, name: c.name, email: c.email })));
        }
      },
      error: () => this.contacts.set([])
    });
    this.sendDialogOpen.set(true);
  }

  closeSendDialog(): void {
    if (this.sending()) return; // block close while in-flight
    this.sendDialogOpen.set(false);
  }

  confirmSend(): void {
    if (!this.isValidEmail(this.recipientEmail)) {
      this.sendError.set('Please enter a valid recipient email.');
      return;
    }
    if (!this.subject.trim()) {
      this.sendError.set('Please enter a subject.');
      return;
    }
    if (this.scheduleEnabled() && this.scheduledAt) {
      const t = new Date(this.scheduledAt + '+07:00').getTime();
      if (t < Date.now() + 5 * 60 * 1000) {
        this.scheduleError.set('Scheduled time must be at least 5 minutes from now (Vietnam Time)');
        return;
      }
    }
    this.scheduleError.set('');
    this.sendError.set('');
    this.sending.set(true);

    const token = this.auth.getToken();
    const headers = { Authorization: `Bearer ${token}` };
    const isScheduled = this.scheduleEnabled() && this.scheduledAt;

    const url    = isScheduled ? `${this.base}/greetings/schedule` : `${this.base}/greetings`;
    const body: any = {
      cardId:          this.cardId,
      recipientEmail:  this.recipientEmail,
      subject:         this.subject,
      personalMessage: this.message() || null,
    };
    if (isScheduled) body['scheduledSendAt'] = new Date(this.scheduledAt + '+07:00').toISOString();

    this.http.post<any>(url, body, { headers }).subscribe({
      next: _res => {
        this.sending.set(false);
        this.sendDialogOpen.set(false);
        // Navigate history with success state via query params
        this.router.navigate(['/history'], {
          queryParams: {
            status: isScheduled ? 'Scheduled' : 'Sent',
            to:   this.recipientEmail,
          }
        });
      },
      error: err => {
        this.sending.set(false);
        const msg = err.error?.message ?? 'Failed to send card. Please try again.';
        this.sendError.set(msg);
      }
    });
  }

  saveDraft(): void {
    if (this.cardLoading() || this.pendingTemplateCheck()) {
      this.showToast('Please wait while the card editor finishes loading.', false);
      return;
    }
    if (!this.auth.getToken()) {
      this.showToast('Please log in to save a draft.', false);
      return;
    }
    if (!this.cardId) {
      this.showToast('Cannot save draft: card ID not found.', false);
      return;
    }

    const token = this.auth.getToken();
    const headers = { Authorization: `Bearer ${token}` };
    const existingDraftId = this.draftId();

    this.savingDraft.set(true);

    if (existingDraftId) {
      // Draft đã tồn tại → PATCH /drafts/:id (auto-save endpoint, BR-09)
      const body = {
        personalMessage:   this.message() || null,
        customJsonContent: this.buildCustomJson()
      };
      this.http.patch<any>(`${this.base}/drafts/${existingDraftId}`, body, { headers }).subscribe({
        next: () => {
          this.savingDraft.set(false);
          this.showToast('Draft saved!', true);
        },
        error: err => {
          this.savingDraft.set(false);
          const msg = err.error?.message ?? 'Failed to save draft.';
          this.showToast(msg, false);
        }
      });
    } else {
      // Chưa có draft → POST /drafts (tạo mới, lưu ID, bật auto-save)
      const body = {
        cardId:            this.cardId,
        personalMessage:   this.message() || null,
        customJsonContent: this.buildCustomJson()
      };
      this.http.post<any>(`${this.base}/drafts`, body, { headers }).subscribe({
        next: res => {
          this.savingDraft.set(false);
          const id: string | undefined = res?.data?.id;
          if (id) {
            this.draftId.set(id);
            this.startAutoSave(); // BR-09: bật auto-save 30s từ đây
          }
          this.showToast('Draft saved!', true);
        },
        error: err => {
          this.savingDraft.set(false);
          const msg = err.error?.message ?? 'Failed to save draft.';
          this.showToast(msg, false);
        }
      });
    }
  }

  private showToast(msg: string, success: boolean): void {
    this.toastMsg.set(msg);
    this.toastSuccess.set(success);
    setTimeout(() => this.toastMsg.set(''), 4000);
  }
}
