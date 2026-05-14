import { CARD_BACKGROUNDS } from '../cards/personalize/card-themes.data';

export interface CardDto {
  id: string;
  name: string;
  slug: string;
  thumbnailUrl?: string;
  description?: string;
  isFeatured: boolean;
  customJsonContent?: string;
  isPremium: boolean;
  categoryName: string;
  categorySlug: string;
  status: string;
}

const FALLBACK_IMGS: Record<string, string> = {
  'sinh-nhat': 'https://images.unsplash.com/photo-1558636508-e0db3814bd1d?w=600&q=80&auto=format&fit=crop',
  'dam-cuoi':  'https://images.unsplash.com/photo-1522673607200-164d1b6ce486?w=600&q=80&auto=format&fit=crop',
  'le-hoi':    'https://images.unsplash.com/photo-1467810563316-b5476525c0f9?w=600&q=80&auto=format&fit=crop',
  'nam-moi':   'https://images.unsplash.com/photo-1516450360452-9312f5e86fc7?w=600&q=80&auto=format&fit=crop',
};

export function getCardBg(card: CardDto): string {
  if (!card.customJsonContent) {
    return card.thumbnailUrl
      ? `url('${card.thumbnailUrl}') center/cover`
      : (FALLBACK_IMGS[card.categorySlug] ? `url('${FALLBACK_IMGS[card.categorySlug]}') center/cover` : '#fdf8f0');
  }
  try {
    const content = JSON.parse(card.customJsonContent);
    const bgId = content.bgId;
    if (bgId) {
      const themeBg = CARD_BACKGROUNDS.find(b => b.id === bgId);
      if (themeBg) return themeBg.bg;
    }
    return card.thumbnailUrl ? `url('${card.thumbnailUrl}') center/cover` : '#fdf8f0';
  } catch {
    return card.thumbnailUrl ? `url('${card.thumbnailUrl}') center/cover` : '#fdf8f0';
  }
}

export function getCardProp(card: CardDto, prop: 'fontFamily' | 'textColor' | 'heading' | 'message'): string {
  if (!card.customJsonContent) return '';
  try {
    return JSON.parse(card.customJsonContent)[prop] || '';
  } catch { return ''; }
}
