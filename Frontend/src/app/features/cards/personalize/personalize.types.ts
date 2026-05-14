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

export type Align = 'left' | 'center' | 'right';

export interface CardTemplateContent {
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

export interface CardDetailDto {
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

export interface ContactPick { id: string; name: string; email: string; }

export const FONTS = [
  { label: 'Noto Serif (Default)', value: "'Noto Serif', serif" },
  { label: 'Manrope (Modern)',    value: "'Manrope', sans-serif" },
  { label: 'Georgia (Classic)',     value: 'Georgia, serif' },
  { label: 'Palatino (Elegant)', value: "'Palatino Linotype', serif" },
  { label: 'Courier New (Typewriter)', value: "'Courier New', monospace" },
];

export const FONT_SIZES = [
  { label: 'Small',    value: '14px' },
  { label: 'Medium',    value: '18px' },
  { label: 'Large',    value: '24px' },
  { label: 'X-Large', value: '32px' },
  { label: 'Huge', value: '42px' },
];

export const HEADING_SIZES = [
  { label: 'Medium',    value: '32px' },
  { label: 'Large',    value: '42px' },
  { label: 'X-Large', value: '56px' },
];

export const ALIGN_BUTTONS: { value: Align; icon: string; label: string }[] = [
  { value: 'left',   icon: 'format_align_left',   label: 'Align Left' },
  { value: 'center', icon: 'format_align_center', label: 'Align Center' },
  { value: 'right',  icon: 'format_align_right',  label: 'Align Right' },
];
