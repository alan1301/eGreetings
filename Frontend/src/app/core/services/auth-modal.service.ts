import { Injectable, signal } from '@angular/core';

export type AuthModalMode = 'login' | 'register' | null;

@Injectable({ providedIn: 'root' })
export class AuthModalService {
  mode = signal<AuthModalMode>(null);

  open(mode: 'login' | 'register') {
    this.mode.set(mode);
    document.body.style.overflow = 'hidden';
  }

  close() {
    this.mode.set(null);
    document.body.style.overflow = '';
  }

  switch(mode: 'login' | 'register') {
    this.mode.set(mode);
  }
}
