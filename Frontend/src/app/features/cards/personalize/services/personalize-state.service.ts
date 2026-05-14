import { Injectable, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../../core/services/auth.service';
import { SubscriptionService } from '../../../../core/services/subscription.service';
import { CardBackground, CardDecoration } from '../card-themes.data';
import {
  Align, CardDetailDto, CardTemplateContent, ContactPick,
  DraggableElement, FONTS
} from '../personalize.types';
import { environment } from '../../../../../environments/environment';
import { ApiResponse } from '../../../../shared/models/api-response.model';

@Injectable()
export class PersonalizeStateService {
  private route  = inject(ActivatedRoute);
  private router = inject(Router);
  private http   = inject(HttpClient);
  private auth   = inject(AuthService);
  private subscriptionService = inject(SubscriptionService);
  private readonly base = environment.apiBaseUrl;

  // Canvas
  elements = signal<DraggableElement[]>([]);
  activeElementId = signal<string | null>(null);

  // Card meta
  cardId = '';
  templateBgUrl = signal<string>('');
  draftId = signal<string | null>(null);
  private autoSaveIntervalId: ReturnType<typeof setInterval> | null = null;
  hasSubscription = signal(false);
  composerMode = signal<'template' | 'fresh'>('template');
  cardLoading = signal(false);
  cardLoadError = signal('');
  pendingTemplateCheck = signal(false);
  subscriptionResolved = signal(false);
  templateRequiresPremium = signal(false);

  prefillRecipientEmail = signal('');
  prefillScheduledAt    = signal('');
  prefillScheduleEnabled = signal(false);

  // Editor
  heading      = signal('');
  message      = signal('');
  align        = signal<Align>('center');
  fontFamily   = signal(FONTS[0].value);
  fontSize     = signal('18px');
  headingSize  = signal('42px');
  textColor    = signal('#383221');
  hexInput     = signal('383221');

  // Design
  selectedBg     = signal<CardBackground | null>(null);
  selectedDecor  = signal<CardDecoration | null>(null);
  allBackgrounds = signal<CardBackground[]>([]);
  allDecorations = signal<CardDecoration[]>([]);
  private pendingBgId    = signal<string | null | undefined>(undefined);
  private pendingDecorId = signal<string | null | undefined>(undefined);

  // Panel
  activePanel  = signal<string>('design');
  dropdownOpen = signal(false);
  accordionPanels = [
    { id: 'design',     icon: 'style',        label: 'Card Design',  summary: 'Background, decorations' },
    { id: 'content',    icon: 'edit_note',    label: 'Content',      summary: 'Heading, message' },
    { id: 'typography', icon: 'text_fields',  label: 'Typography',   summary: 'Font, size, align' },
    { id: 'color',      icon: 'palette',      label: 'Color',        summary: 'Color wheel RGB' },
    { id: 'photo',      icon: 'add_a_photo',  label: 'Personal Photo', summary: 'Upload your photo', premium: true },
    { id: 'schedule',   icon: 'schedule',     label: 'Schedule',     summary: 'Choose sending time' },
  ] as { id: string; icon: string; label: string; summary: string; premium?: boolean }[];

  currentPanel = computed(() =>
    this.accordionPanels.find(p => p.id === this.activePanel()) ?? this.accordionPanels[0]
  );

  // Schedule
  scheduleEnabled = signal(false);
  scheduledAt     = '';
  scheduleError   = signal('');
  minDateTime = new Date(Date.now() + 5 * 60 * 1000).toISOString().slice(0, 16);
  dateLabel = new Date().toLocaleDateString('en-US', {
    weekday: 'long', day: '2-digit', month: '2-digit', year: 'numeric'
  }).toUpperCase();

  // Send modal
  sendDialogOpen   = signal(false);
  contacts         = signal<ContactPick[]>([]);
  selectedContact  = signal<ContactPick | null>(null);
  recipientEmail   = signal('');
  subject          = signal('');
  sending          = signal(false);
  savingDraft      = signal(false);
  toastMsg         = signal('');
  toastSuccess     = signal(true);
  sendError        = signal('');

  isTemplateMode = computed(() => this.composerMode() === 'template');

  previewBgStyle = computed(() => {
    if (this.isTemplateMode()) {
      const url = this.templateBgUrl();
      return url ? `url("${url}") center/cover` : '#fdf8f0';
    }
    return this.selectedBg()?.bg || '#fdf8f0';
  });

  isLastPanel = computed(() => {
    const ids = this.accordionPanels.map(p => p.id);
    const cur = this.activePanel();
    return cur === ids[ids.length - 1] || cur === '';
  });

  init(): void {
    this.cardId = this.route.snapshot.paramMap.get('id') ?? '';
    this.composerMode.set(this.route.snapshot.queryParamMap.get('mode') === 'fresh' ? 'fresh' : 'template');

    const qEmail       = this.route.snapshot.queryParamMap.get('recipientEmail');
    const qScheduledAt = this.route.snapshot.queryParamMap.get('scheduledAt');
    const qSchedule    = this.route.snapshot.queryParamMap.get('scheduleEnabled');
    if (qEmail) this.prefillRecipientEmail.set(qEmail);
    if (qScheduledAt) {
      this.prefillScheduledAt.set(qScheduledAt);
      this.scheduledAt = qScheduledAt;
    }
    if (qSchedule === 'true') {
      this.prefillScheduleEnabled.set(true);
      this.scheduleEnabled.set(true);
    }

    this.loadDesignAssets();

    if (!this.cardId) {
      this.resolveDefaultCard();
    } else if (this.composerMode() === 'template') {
      this.loadCardTemplate(this.cardId);
    }

    this.subscriptionService.getCurrentSubscription().subscribe({
      next: (res) => {
        if (res.data && res.data.status === 'Active') this.hasSubscription.set(true);
        else this.checkLocalSubscription();
        this.subscriptionResolved.set(true);
        this.enforceTemplateAccess();
      },
      error: () => {
        this.checkLocalSubscription();
        this.subscriptionResolved.set(true);
        this.enforceTemplateAccess();
      }
    });

    if (this.auth.getToken()) this.loadExistingDraft();
  }

  dispose(): void {
    this.stopAutoSave();
  }

  selectPanel(id: string) {
    this.activePanel.set(id);
    this.dropdownOpen.set(false);
  }

  goNext(): void {
    const ids = this.accordionPanels.map(p => p.id);
    const idx = ids.indexOf(this.activePanel());
    if (idx >= 0 && idx < ids.length - 1) this.selectPanel(ids[idx + 1]);
  }

  setColor(hex: string) {
    this.textColor.set(hex);
    this.hexInput.set(hex.replace('#', ''));
  }

  selectDecor(d: CardDecoration | null) {
    this.selectedDecor.set(d);
    this.activeElementId.set(null);
    this.elements.update(arr => arr.filter(e => e.type !== 'emoji'));
    if (d && d.id !== 'none') {
      const newEmojis: DraggableElement[] = d.elements.map((el, i) => ({
        id: `emoji_${Date.now()}_${i}`,
        type: 'emoji',
        content: el.emoji,
        top: parseFloat(el.top) || 0,
        left: parseFloat(el.left) || 0,
        scale: 1,
        rotate: parseFloat(el.rotate) || 0,
        baseSize: el.size,
        opacity: el.opacity
      }));
      this.elements.update(arr => [...arr, ...newEmojis]);
    }
  }

  removeElement(id: string) {
    this.elements.update(arr => arr.filter(e => e.id !== id));
    if (this.activeElementId() === id) this.activeElementId.set(null);
  }

  onPhotoSelected(event: any) {
    const file = event.target.files[0];
    if (!file) return;
    if (file.size > 5 * 1024 * 1024) {
      this.showToast('File is too large (max 5MB)', false);
      return;
    }
    const reader = new FileReader();
    reader.onload = (e) => {
      const result = e.target?.result as string;
      if (result) {
        const newId = `img_${Date.now()}`;
        this.elements.update(arr => [...arr, {
          id: newId, type: 'image', content: result,
          top: 50, left: 50, scale: 1, rotate: 0
        }]);
        this.activeElementId.set(newId);
        this.selectPanel('design');
      }
    };
    reader.readAsDataURL(file);
    event.target.value = '';
  }

  isValidEmail(email: string): boolean {
    return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
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
    this.recipientEmail.set(this.prefillRecipientEmail() || '');
    this.subject.set('');
    this.sendError.set('');
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
    if (this.sending()) return;
    this.sendDialogOpen.set(false);
  }

  confirmSend(): void {
    if (!this.isValidEmail(this.recipientEmail())) {
      this.sendError.set('Please enter a valid recipient email.');
      return;
    }
    if (!this.subject().trim()) {
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
      recipientEmail:  this.recipientEmail(),
      subject:         this.subject(),
      personalMessage: this.message() || null,
      isFresh:         this.composerMode() === 'fresh',
    };
    if (isScheduled) body['scheduledSendAt'] = new Date(this.scheduledAt + '+07:00').toISOString();

    this.http.post<any>(url, body, { headers }).subscribe({
      next: () => {
        this.sending.set(false);
        this.sendDialogOpen.set(false);
        const currentDraftId = this.draftId();
        if (currentDraftId) {
          this.stopAutoSave();
          this.http.delete(`${this.base}/drafts/${currentDraftId}`, { headers }).subscribe();
          this.draftId.set(null);
        }
        this.router.navigate(['/history'], {
          queryParams: { status: isScheduled ? 'Scheduled' : 'Sent', to: this.recipientEmail() }
        });
      },
      error: err => {
        this.sending.set(false);
        this.sendError.set(err.error?.message ?? 'Failed to send card. Please try again.');
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
      const body = {
        personalMessage:   this.message() || null,
        customJsonContent: this.buildCustomJson()
      };
      this.http.patch<any>(`${this.base}/drafts/${existingDraftId}`, body, { headers }).subscribe({
        next: () => { this.savingDraft.set(false); this.showToast('Draft saved!', true); },
        error: err => { this.savingDraft.set(false); this.showToast(err.error?.message ?? 'Failed to save draft.', false); }
      });
    } else {
      const body = {
        cardId:            this.cardId,
        personalMessage:   this.message() || null,
        customJsonContent: this.buildCustomJson(),
        isFresh:           this.composerMode() === 'fresh'
      };
      this.http.post<any>(`${this.base}/drafts`, body, { headers }).subscribe({
        next: res => {
          this.savingDraft.set(false);
          const id: string | undefined = res?.data?.id;
          if (id) { this.draftId.set(id); this.startAutoSave(); }
          this.showToast('Draft saved!', true);
        },
        error: err => { this.savingDraft.set(false); this.showToast(err.error?.message ?? 'Failed to save draft.', false); }
      });
    }
  }

  // ── Private ───────────────────────────────────────

  private loadExistingDraft(): void {
    const token = this.auth.getToken();
    if (!token || !this.cardId) return;
    this.http.get<any>(`${this.base}/me/drafts?pageSize=50`, {
      headers: { Authorization: `Bearer ${token}` }
    }).subscribe({
      next: res => {
        if (res.success && res.data) {
          const items: any[] = res.data.items ?? (Array.isArray(res.data) ? res.data : []);
          const existing = items.find(d => d.cardId === this.cardId);
          if (existing?.id && !this.draftId()) {
            this.draftId.set(existing.id);
            this.startAutoSave();
          }
        }
      },
      error: () => {}
    });
  }

  private loadDesignAssets(): void {
    this.http.get<any>(`${this.base}/card-backgrounds`).subscribe({
      next: res => {
        if (res.success && Array.isArray(res.data)) {
          this.allBackgrounds.set(res.data.map((b: any) => ({
            id: b.id, label: b.label,
            categories: JSON.parse(b.categories ?? '[]'),
            bg: b.bgStyle,
            isPremium: b.isPremium
          } as CardBackground)));
          const pendingId = this.pendingBgId();
          if (pendingId !== undefined) {
            this.selectedBg.set(this.allBackgrounds().find(b => b.id === pendingId) ?? null);
            this.pendingBgId.set(undefined);
          }
        }
      },
      error: () => {}
    });

    this.http.get<any>(`${this.base}/card-decorations`).subscribe({
      next: res => {
        if (res.success && Array.isArray(res.data)) {
          this.allDecorations.set(res.data.map((d: any) => ({
            id: d.id, label: d.label, preview: d.preview,
            categories: JSON.parse(d.categories ?? '[]'),
            elements: JSON.parse(d.elements ?? '[]')
          } as CardDecoration)));
          const pendingId = this.pendingDecorId();
          if (pendingId !== undefined) {
            const decor = this.allDecorations().find(d => d.id === pendingId) ?? null;
            this.selectDecor(decor);
            this.pendingDecorId.set(undefined);
          }
        }
      },
      error: () => {}
    });
  }

  private startAutoSave(): void {
    if (this.autoSaveIntervalId !== null) return;
    this.autoSaveIntervalId = setInterval(() => this.autoSaveDraft(), 30_000);
  }

  private stopAutoSave(): void {
    if (this.autoSaveIntervalId !== null) {
      clearInterval(this.autoSaveIntervalId);
      this.autoSaveIntervalId = null;
    }
  }

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
    }).subscribe();
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
    this.http.get<ApiResponse<any>>(`${this.base}/cards?pageSize=100&sort=featured`).subscribe({
      next: (res) => {
        this.cardLoading.set(false);
        const items = Array.isArray(res.data) ? res.data : (res.data?.items ?? []);
        const fallbackCard = items.find((c: any) => !c.isPremium) ?? items[0];
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
        if (res.success && res.data) {
          const url = res.data.thumbnailUrl || res.data.fileUrl || '';
          this.templateBgUrl.set(url);
          if (res.data.customJsonContent) this.applyTemplateContent(res.data.customJsonContent);
        }
      },
      error: () => this.pendingTemplateCheck.set(false)
    });
  }

  private enforceTemplateAccess() {
    if (!this.subscriptionResolved() || !this.templateRequiresPremium() || this.hasSubscription() || this.composerMode() !== 'template' || !this.cardId) {
      return;
    }
    this.router.navigate(['/cards', this.cardId], { queryParams: { premium: 'required' } });
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

      if (this.allBackgrounds().length > 0) {
        this.selectedBg.set(this.allBackgrounds().find(bg => bg.id === content.bgId) ?? null);
      } else {
        this.pendingBgId.set(content.bgId ?? null);
      }
      if (this.allDecorations().length > 0) {
        this.selectDecor(this.allDecorations().find(decor => decor.id === content.decorId) ?? null);
      } else {
        this.pendingDecorId.set(content.decorId ?? null);
      }
    } catch {}
  }

  private checkLocalSubscription() {
    const stored = localStorage.getItem('eg_subscription');
    if (stored) {
      try { JSON.parse(stored); this.hasSubscription.set(true); }
      catch (e) { console.error('Error parsing stored subscription:', e); }
    }
  }

  private showToast(msg: string, success: boolean): void {
    this.toastMsg.set(msg);
    this.toastSuccess.set(success);
    setTimeout(() => this.toastMsg.set(''), 4000);
  }
}
