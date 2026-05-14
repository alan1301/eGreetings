export interface CardBackground {
  id: string;
  label: string;
  categories: string[];
  bg: string;
  isPremium?: boolean;
}

export interface DecoElement {
  emoji: string; top: string; left: string;
  rotate: string; size: string; opacity?: string;
}

export interface CardDecoration {
  id: string; label: string; preview: string;
  categories: string[]; elements: DecoElement[];
}

// ── Backgrounds ──────────────────────────────────────────
export const CARD_BACKGROUNDS: CardBackground[] = [
  // Universal
  { id: 'cream',   label: 'Classic Cream', categories: ['all'],
    bg: '#fdf8f0' },
  { id: 'white',   label: 'Pure White',  categories: ['all'],
    bg: '#ffffff' },
  { id: 'linen',   label: 'Linen',    categories: ['all'],
    bg: 'linear-gradient(160deg,#f5f0e8 0%,#ede8e0 100%)' },

  // Birthday
  { id: 'bday-pink',    label: 'Vibrant Pink',   categories: ['birthday'],
    bg: 'linear-gradient(135deg,#fce4ec 0%,#f8bbd0 50%,#fce4ec 100%)' },
  { id: 'bday-blue',    label: 'Joyful Blue',  categories: ['birthday'],
    bg: 'linear-gradient(135deg,#e3f2fd 0%,#bbdefb 50%,#e8f5e9 100%)' },
  { id: 'bday-gold',    label: 'Golden Sparkle',   categories: ['birthday'],
    bg: 'linear-gradient(135deg,#fff8e1 0%,#ffecb3 40%,#ffe082 100%)' },
  { id: 'bday-rainbow', label: 'Pastel Rainbow', categories: ['birthday'],
    bg: 'linear-gradient(180deg,#fce4ec 0%,#e8eaf6 25%,#e3f2fd 50%,#e8f5e9 75%,#fffde7 100%)' },
  { id: 'bday-purple',  label: 'Dreamy Purple',   categories: ['birthday'],
    bg: 'linear-gradient(135deg,#f3e5f5 0%,#e1bee7 50%,#ce93d8 100%)' },
  { id: 'bday-mint',    label: 'Mint Green',   categories: ['birthday'],
    bg: 'linear-gradient(135deg,#e0f2f1 0%,#b2dfdb 100%)' },

  // Wedding
  { id: 'wed-ivory',  label: 'Elegant Ivory', categories: ['wedding'],
    bg: 'linear-gradient(160deg,#fdf6e3 0%,#f5e6c8 100%)' },
  { id: 'wed-rose',   label: 'Rose Gold', categories: ['wedding'],
    bg: 'linear-gradient(135deg,#fff0f3 0%,#ffe4e1 40%,#ffd7cc 100%)' },
  { id: 'wed-sage',   label: 'Sage Green',      categories: ['wedding'],
    bg: 'linear-gradient(160deg,#f0f4f0 0%,#dce8dc 100%)' },
  { id: 'wed-blush',  label: 'Peach Blush',       categories: ['wedding'],
    bg: 'linear-gradient(160deg,#fff5f5 0%,#ffe0e0 100%)' },
  { id: 'wed-sky',    label: 'Sky Blue',      categories: ['wedding'],
    bg: 'linear-gradient(135deg,#e8f4f8 0%,#d0e8f2 100%)' },
  { id: 'wed-lavender', label: 'Lavender',    categories: ['wedding'],
    bg: 'linear-gradient(160deg,#f5f0ff 0%,#e8d8ff 100%)' },

  // New Year
  { id: 'ny-midnight', label: 'New Year Eve', categories: ['newyear'],
    bg: 'linear-gradient(160deg,#0d1b2a 0%,#1b2a4a 50%,#0d1b2a 100%)' },
  { id: 'ny-gold',     label: 'Champagne Gold', categories: ['newyear'],
    bg: 'linear-gradient(135deg,#2c1810 0%,#8b6914 40%,#d4af37 100%)' },
  { id: 'ny-red',      label: 'Festive Red',         categories: ['newyear'],
    bg: 'linear-gradient(160deg,#7f0000 0%,#c62828 50%,#7f0000 100%)' },
  { id: 'ny-crystal',  label: 'Blue Crystal',   categories: ['newyear'],
    bg: 'linear-gradient(135deg,#e0f7fa 0%,#b2ebf2 50%,#80deea 100%)' },
  { id: 'ny-snow',     label: 'White Snow',   categories: ['newyear'],
    bg: 'linear-gradient(160deg,#ffffff 0%,#e8f4f8 50%,#dce8f0 100%)' },

  // Festival
  { id: 'fest-autumn', label: 'Autumn',   categories: ['festival'],
    bg: 'linear-gradient(135deg,#fff3e0 0%,#ffe0b2 40%,#ffcc80 100%)' },
  { id: 'fest-spring', label: 'Spring',  categories: ['festival'],
    bg: 'linear-gradient(160deg,#fce4ec 0%,#f8bbd0 30%,#e8f5e9 100%)' },
  { id: 'fest-summer', label: 'Summer',    categories: ['festival'],
    bg: 'linear-gradient(135deg,#fff9c4 0%,#b3e5fc 100%)' },
  { id: 'fest-winter', label: 'Christmas', categories: ['festival'],
    bg: 'linear-gradient(160deg,#1a3c2a 0%,#2d5a40 50%,#1a3c2a 100%)' },

  // Premium
  { id: 'prm-marble', label: 'Marble', categories: ['all'], isPremium: true,
    bg: 'linear-gradient(135deg,#f5f5f5 0%,#e0e0e0 25%,#f8f8f8 50%,#e8e8e8 75%,#f5f5f5 100%)' },
  { id: 'prm-velvet', label: 'Purple Velvet',    categories: ['all'], isPremium: true,
    bg: 'linear-gradient(160deg,#2d1b4e 0%,#4a2980 100%)' },
  { id: 'prm-noir',   label: 'Elegant Noir',    categories: ['all'], isPremium: true,
    bg: 'linear-gradient(160deg,#1a1a1a 0%,#2d2d2d 100%)' },

  // --- Image Textures ---
  // Birthday
  { id: 'img-bday-confetti', label: 'Party Confetti',   categories: ['birthday'], bg: 'url("https://images.unsplash.com/photo-1492684223066-81342ee5ff30?w=800") center/cover' },
  { id: 'img-bday-balloons', label: 'Pastel Balloons',  categories: ['birthday'], bg: 'url("https://images.unsplash.com/photo-1574271143515-5cddf8da19be?w=800") center/cover' },
  // Wedding
  { id: 'img-wed-silk',   label: 'White Silk',     categories: ['wedding'], bg: 'url("https://images.unsplash.com/photo-1519225421980-715cb0215aed?w=800") center/cover' },
  { id: 'img-wed-floral', label: 'Floral Abstract', categories: ['wedding'], bg: 'url("https://images.unsplash.com/photo-1518531933037-91b2f5f229cc?w=800") center/cover' },
  // New Year
  { id: 'img-ny-gold',     label: 'Gold Bokeh',      categories: ['newyear'], bg: 'url("https://images.unsplash.com/photo-1513151233558-d860c5398176?w=800") center/cover' },
  { id: 'img-ny-fireworks', label: 'Night Fireworks', categories: ['newyear'], bg: 'url("https://images.unsplash.com/photo-1516912481808-3406841bd33c?w=800") center/cover' },
  // Festival
  { id: 'img-fest-xmas',         label: 'Christmas Magic', categories: ['festival'], bg: 'url("https://images.unsplash.com/photo-1482517967863-00e15c9b44be?w=800") center/cover' },
  { id: 'img-fest-halloween',    label: 'Spooky Night',    categories: ['festival'], bg: 'url("https://images.unsplash.com/photo-1508361001413-7a9dca21d08a?w=800") center/cover' },
  { id: 'img-fest-thanksgiving', label: 'Autumn Harvest',  categories: ['festival'], bg: 'url("https://images.unsplash.com/photo-1505253758473-96b7015fcd40?w=800") center/cover' },
  // Graduation
  { id: 'img-grad-cap',      label: 'Graduation Ceremony', categories: ['graduation'], bg: 'url("https://images.unsplash.com/photo-1498243691581-b145c3f54a5a?w=800") center/cover' },
  { id: 'img-grad-day',      label: 'Graduation Day',      categories: ['graduation'], bg: 'url("https://images.unsplash.com/photo-1541339907198-e08756dedf3f?w=800") center/cover' },
  { id: 'img-grad-diploma',  label: 'Graduation Diploma',  categories: ['graduation'], bg: 'url("https://images.unsplash.com/photo-1627556704290-2b1f5853ff78?w=800") center/cover' },
  { id: 'img-grad-celebrate', label: 'Graduation Celebrate', categories: ['graduation'], bg: 'url("https://images.unsplash.com/photo-1580582932707-520aed937b7b?w=800") center/cover' },
  // Birthday (more)
  { id: 'img-bday-gold',      label: 'Golden Birthday',    categories: ['birthday'], bg: 'url("https://picsum.photos/id/237/800/600") center/cover' },
  { id: 'img-bday-cake',      label: 'Birthday Cake',      categories: ['birthday'], bg: 'url("https://images.unsplash.com/photo-1535141192574-5d4897c12636?w=800") center/cover' },
  { id: 'img-bday-sparklers', label: 'Birthday Sparklers', categories: ['birthday'], bg: 'url("https://images.unsplash.com/photo-1516280440614-37939bbacd81?w=800") center/cover' },
  // Wedding (more)
  { id: 'img-wed-rings',  label: 'Wedding Rings',   categories: ['wedding'], bg: 'url("https://images.unsplash.com/photo-1515934751635-c81c6bc9a2d8?w=800") center/cover' },
  { id: 'img-wed-couple', label: 'Wedding Couple',  categories: ['wedding'], bg: 'url("https://images.unsplash.com/photo-1511285560929-80b456fea0bc?w=800") center/cover' },
  { id: 'img-wed-rustic', label: 'Rustic Garden',   categories: ['wedding'], bg: 'url("https://images.unsplash.com/photo-1520854221256-17451cc331bf?w=800") center/cover' },
  // New Year (more)
  { id: 'img-ny-champagne', label: 'Champagne Toast', categories: ['newyear'], bg: 'url("https://images.unsplash.com/photo-1514362545857-3bc16c4c7d1b?w=800") center/cover' },
  { id: 'img-ny-sparkle',   label: 'Golden Confetti', categories: ['newyear'], bg: 'url("https://images.unsplash.com/photo-1467810563316-b5476525c0f9?w=800") center/cover' },
  // Festival (more)
  { id: 'img-fest-lantern',  label: 'Lantern Night',    categories: ['festival'], bg: 'url("https://picsum.photos/id/355/800/600") center/cover' },
  { id: 'img-fest-carnival', label: 'Colorful Carnival', categories: ['festival'], bg: 'url("https://images.unsplash.com/photo-1533174072545-7a4b6ad7a6c3?w=800") center/cover' },

  // ── Additional Featured Cards ─────────────────────────────────────
  // Birthday (additional featured)
  { id: 'img-bday-confetti2', label: 'Confetti Party', categories: ['birthday'], bg: 'url("https://images.unsplash.com/photo-1514525253161-7a46d19cd819?w=800") center/cover' },
  { id: 'img-bday-bloom',     label: 'Flower Bloom',   categories: ['birthday'], bg: 'url("https://picsum.photos/id/82/800/600") center/cover' },
  // Wedding (additional featured)
  { id: 'img-wed-golden', label: 'Golden Hour Wedding', categories: ['wedding'], bg: 'url("https://images.unsplash.com/photo-1537633552985-df8429e8048b?w=800") center/cover' },
  { id: 'img-wed-petals', label: 'Rose Petals',         categories: ['wedding'], bg: 'url("https://images.unsplash.com/photo-1519741497674-611481863552?w=800") center/cover' },
  // New Year (additional featured)
  { id: 'img-ny-midnight',  label: 'Midnight Countdown', categories: ['newyear'], bg: 'url("https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800") center/cover' },
  { id: 'img-ny-gold-glow', label: 'Golden NY Glow',     categories: ['newyear'], bg: 'url("https://images.unsplash.com/photo-1467810563316-b5476525c0f9?w=800") center/cover' },
  { id: 'img-ny-stars',     label: 'Night Sky Stars',    categories: ['newyear'], bg: 'url("https://images.unsplash.com/photo-1516912481808-3406841bd33c?w=800") center/cover' },
  // Festival (additional featured)
  { id: 'img-fest-concert', label: 'Festival Concert', categories: ['festival'], bg: 'url("https://images.unsplash.com/photo-1533174072545-7a4b6ad7a6c3?w=800") center/cover' },
  { id: 'img-fest-blossom', label: 'Cherry Blossom',   categories: ['festival'], bg: 'url("https://images.unsplash.com/photo-1522383225653-ed111181a951?w=800") center/cover' },
  // Graduation (additional featured)
  { id: 'img-grad-scroll', label: 'Graduation Scroll', categories: ['graduation'], bg: 'url("https://images.unsplash.com/photo-1541339907198-e08756dedf3f?w=800") center/cover' },
  { id: 'img-grad-step',   label: 'Graduation Stage',  categories: ['graduation'], bg: 'url("https://images.unsplash.com/photo-1501386761578-eac5c94b800a?w=800") center/cover' },
];

// ── Decorations ──────────────────────────────────────────
export const CARD_DECORATIONS: CardDecoration[] = [
  { id: 'none', label: 'None', preview: '✕', categories: ['all'], elements: [] },

  // Birthday
  { id: 'bday-balloons', label: 'Balloons', preview: '🎈', categories: ['birthday'],
    elements: [
      { emoji: '🎈', top: '4%',  left: '4%',  rotate: '-15deg', size: '2rem' },
      { emoji: '🎈', top: '6%',  left: '78%', rotate: '10deg',  size: '1.6rem' },
      { emoji: '🎉', top: '83%', left: '8%',  rotate: '20deg',  size: '1.8rem' },
      { emoji: '✨', top: '14%', left: '48%', rotate: '0deg',   size: '1.2rem', opacity: '0.7' },
      { emoji: '🎊', top: '82%', left: '72%', rotate: '-10deg', size: '1.6rem' },
    ]},
  { id: 'bday-cake',    label: 'Cake', preview: '🎂', categories: ['birthday'],
    elements: [
      { emoji: '🎂', top: '78%', left: '44%', rotate: '0deg',   size: '2.4rem' },
      { emoji: '🎁', top: '4%',  left: '8%',  rotate: '-10deg', size: '1.8rem' },
      { emoji: '🌟', top: '5%',  left: '76%', rotate: '20deg',  size: '1.6rem' },
      { emoji: '🎀', top: '80%', left: '10%', rotate: '-5deg',  size: '1.6rem' },
      { emoji: '🍭', top: '82%', left: '75%', rotate: '10deg',  size: '1.6rem' },
    ]},
  { id: 'bday-stars',   label: 'Stars', preview: '⭐', categories: ['birthday'],
    elements: [
      { emoji: '⭐', top: '4%',  left: '12%', rotate: '10deg',  size: '1.6rem' },
      { emoji: '✨', top: '8%',  left: '72%', rotate: '-5deg',  size: '1.2rem', opacity: '0.8' },
      { emoji: '🌟', top: '80%', left: '15%', rotate: '15deg',  size: '1.8rem' },
      { emoji: '💫', top: '76%', left: '74%', rotate: '-10deg', size: '1.6rem' },
      { emoji: '⭐', top: '45%', left: '4%',  rotate: '5deg',   size: '1.2rem', opacity: '0.5' },
    ]},

  // Wedding
  { id: 'wed-flowers', label: 'Flowers', preview: '💐', categories: ['wedding'],
    elements: [
      { emoji: '🌸', top: '4%',  left: '4%',  rotate: '-20deg', size: '2rem' },
      { emoji: '🌺', top: '6%',  left: '74%', rotate: '15deg',  size: '1.8rem' },
      { emoji: '🌹', top: '78%', left: '6%',  rotate: '-10deg', size: '1.8rem' },
      { emoji: '💐', top: '80%', left: '70%', rotate: '10deg',  size: '2rem' },
      { emoji: '🌷', top: '44%', left: '2%',  rotate: '-5deg',  size: '1.4rem', opacity: '0.6' },
    ]},
  { id: 'wed-hearts',  label: 'Hearts', preview: '💕', categories: ['wedding'],
    elements: [
      { emoji: '❤️', top: '4%',  left: '44%', rotate: '0deg',   size: '2rem' },
      { emoji: '💕', top: '80%', left: '40%', rotate: '0deg',   size: '1.8rem' },
      { emoji: '💍', top: '8%',  left: '8%',  rotate: '-10deg', size: '1.6rem' },
      { emoji: '🕊️', top: '10%', left: '74%', rotate: '15deg',  size: '1.6rem' },
      { emoji: '💖', top: '44%', left: '78%', rotate: '5deg',   size: '1.2rem', opacity: '0.6' },
    ]},
  { id: 'wed-butterflies', label: 'Butterflies', preview: '🦋', categories: ['wedding'],
    elements: [
      { emoji: '🦋', top: '5%',  left: '8%',  rotate: '-15deg', size: '1.8rem' },
      { emoji: '🦋', top: '10%', left: '68%', rotate: '20deg',  size: '1.4rem' },
      { emoji: '🌸', top: '78%', left: '12%', rotate: '-5deg',  size: '1.8rem' },
      { emoji: '🌷', top: '80%', left: '68%', rotate: '10deg',  size: '1.8rem' },
      { emoji: '🦋', top: '48%', left: '76%', rotate: '-10deg', size: '1.2rem', opacity: '0.5' },
    ]},

  // New Year
  { id: 'ny-fireworks', label: 'Fireworks',  preview: '🎆', categories: ['newyear'],
    elements: [
      { emoji: '🎆', top: '4%',  left: '8%',  rotate: '0deg',  size: '2.2rem' },
      { emoji: '🎇', top: '6%',  left: '68%', rotate: '0deg',  size: '2rem' },
      { emoji: '✨', top: '80%', left: '18%', rotate: '0deg',  size: '1.6rem', opacity: '0.8' },
      { emoji: '⭐', top: '76%', left: '74%', rotate: '20deg', size: '1.6rem' },
      { emoji: '🌟', top: '44%', left: '76%', rotate: '-5deg', size: '1.2rem', opacity: '0.6' },
    ]},
  { id: 'ny-champagne', label: 'Party', preview: '🥂', categories: ['newyear'],
    elements: [
      { emoji: '🥂', top: '76%', left: '40%', rotate: '0deg',   size: '2.2rem' },
      { emoji: '🎊', top: '4%',  left: '8%',  rotate: '-10deg', size: '2rem' },
      { emoji: '🎉', top: '6%',  left: '70%', rotate: '10deg',  size: '1.8rem' },
      { emoji: '✨', top: '44%', left: '4%',  rotate: '0deg',   size: '1.2rem', opacity: '0.6' },
      { emoji: '🎈', top: '78%', left: '72%', rotate: '5deg',   size: '1.6rem' },
    ]},
  { id: 'ny-lantern', label: 'Lanterns', preview: '🏮', categories: ['newyear'],
    elements: [
      { emoji: '🏮', top: '4%',  left: '12%', rotate: '-5deg',  size: '2rem' },
      { emoji: '🏮', top: '6%',  left: '64%', rotate: '5deg',   size: '1.8rem' },
      { emoji: '🌙', top: '78%', left: '8%',  rotate: '-10deg', size: '1.8rem' },
      { emoji: '⭐', top: '80%', left: '70%', rotate: '15deg',  size: '1.6rem' },
      { emoji: '✨', top: '44%', left: '76%', rotate: '0deg',   size: '1.2rem', opacity: '0.5' },
    ]},

  // Festival
  { id: 'fest-autumn', label: 'Autumn Leaves', preview: '🍂', categories: ['festival'],
    elements: [
      { emoji: '🍂', top: '4%',  left: '8%',  rotate: '-20deg', size: '2rem' },
      { emoji: '🍁', top: '8%',  left: '72%', rotate: '15deg',  size: '1.8rem' },
      { emoji: '🍂', top: '78%', left: '12%', rotate: '10deg',  size: '1.6rem' },
      { emoji: '🍁', top: '80%', left: '68%', rotate: '-15deg', size: '1.8rem' },
      { emoji: '🍄', top: '44%', left: '4%',  rotate: '5deg',   size: '1.2rem', opacity: '0.5' },
    ]},
  { id: 'fest-spring', label: 'Spring Bloom', preview: '🌸', categories: ['festival'],
    elements: [
      { emoji: '🌸', top: '4%',  left: '6%',  rotate: '-15deg', size: '1.8rem' },
      { emoji: '🌷', top: '6%',  left: '70%', rotate: '10deg',  size: '1.8rem' },
      { emoji: '🦋', top: '78%', left: '8%',  rotate: '-10deg', size: '1.8rem' },
      { emoji: '🌺', top: '80%', left: '70%', rotate: '5deg',   size: '1.8rem' },
      { emoji: '🌼', top: '44%', left: '76%', rotate: '-5deg',  size: '1.2rem', opacity: '0.6' },
    ]},
  { id: 'fest-christmas', label: 'Christmas', preview: '🎄', categories: ['festival'],
    elements: [
      { emoji: '🎄', top: '4%',  left: '8%',  rotate: '-5deg',  size: '2.2rem' },
      { emoji: '⭐', top: '3%',  left: '70%', rotate: '10deg',  size: '1.6rem' },
      { emoji: '🦌', top: '80%', left: '8%',  rotate: '-10deg', size: '1.8rem' },
      { emoji: '🎁', top: '78%', left: '68%', rotate: '5deg',   size: '1.8rem' },
      { emoji: '❄️', top: '44%', left: '76%', rotate: '0deg',   size: '1.4rem', opacity: '0.6' },
    ]},

  // Universal
  { id: 'universal-sparkle', label: 'Sparkles', preview: '✨', categories: ['all'],
    elements: [
      { emoji: '✨', top: '4%',  left: '8%',  rotate: '0deg', size: '1.4rem', opacity: '0.7' },
      { emoji: '✨', top: '6%',  left: '76%', rotate: '0deg', size: '1rem',   opacity: '0.5' },
      { emoji: '✨', top: '78%', left: '12%', rotate: '0deg', size: '1.4rem', opacity: '0.7' },
      { emoji: '✨', top: '80%', left: '74%', rotate: '0deg', size: '1rem',   opacity: '0.5' },
      { emoji: '💫', top: '44%', left: '4%',  rotate: '0deg', size: '1.2rem', opacity: '0.4' },
    ]},
  { id: 'universal-corner', label: 'Corner Flowers',  preview: '🌸', categories: ['all'],
    elements: [
      { emoji: '🌸', top: '2%',  left: '2%',  rotate: '-20deg', size: '1.8rem' },
      { emoji: '🌸', top: '3%',  left: '76%', rotate: '20deg',  size: '1.5rem' },
      { emoji: '🌸', top: '80%', left: '2%',  rotate: '-10deg', size: '1.5rem' },
      { emoji: '🌸', top: '82%', left: '76%', rotate: '10deg',  size: '1.8rem' },
    ]},
  { id: 'universal-hearts', label: 'Hearts', preview: '❤️', categories: ['all'],
    elements: [
      { emoji: '❤️', top: '4%',  left: '44%', rotate: '0deg', size: '1.8rem' },
      { emoji: '💕', top: '80%', left: '40%', rotate: '0deg', size: '1.4rem' },
      { emoji: '❤️', top: '44%', left: '4%',  rotate: '-10deg', size: '1rem', opacity: '0.4' },
      { emoji: '❤️', top: '44%', left: '76%', rotate: '10deg',  size: '1rem', opacity: '0.4' },
    ]},
];

export const BG_CATEGORIES = [
  { id: 'all',      label: 'All' },
  { id: 'birthday', label: 'Birthday' },
  { id: 'wedding',  label: 'Wedding' },
  { id: 'newyear',  label: 'New Year' },
  { id: 'festival', label: 'Festival' },
  { id: 'graduation', label: 'Graduation' },
];
