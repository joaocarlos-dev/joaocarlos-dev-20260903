import { DOCUMENT } from '@angular/common';
import { inject, Injectable, signal } from '@angular/core';

export type ColorTheme = 'light' | 'dark';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly document = inject(DOCUMENT);
  private readonly storageKey = 'gestao-color-theme';

  readonly theme = signal<ColorTheme>(this.initialTheme());

  constructor() {
    this.apply(this.theme());
  }

  toggle(): void {
    const theme: ColorTheme = this.theme() === 'light' ? 'dark' : 'light';
    this.theme.set(theme);
    this.apply(theme);
    try {
      this.document.defaultView?.localStorage.setItem(this.storageKey, theme);
    } catch {
      return;
    }
  }

  private initialTheme(): ColorTheme {
    let stored: string | null;
    try {
      stored = this.document.defaultView?.localStorage.getItem(this.storageKey) ?? null;
    } catch {
      stored = null;
    }
    if (stored === 'light' || stored === 'dark') {
      return stored;
    }

    return this.document.defaultView?.matchMedia?.('(prefers-color-scheme: dark)').matches
      ? 'dark'
      : 'light';
  }

  private apply(theme: ColorTheme): void {
    this.document.documentElement.dataset['theme'] = theme;
  }
}
