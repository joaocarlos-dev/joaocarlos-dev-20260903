import { TestBed } from '@angular/core/testing';
import { ThemeService } from './theme-service';

describe('ThemeService', () => {
  beforeEach(() => {
    vi.restoreAllMocks();
    localStorage.clear();
    document.documentElement.removeAttribute('data-theme');
  });

  it('should switch and persist the selected color theme', () => {
    const service = TestBed.inject(ThemeService);
    const initialTheme = service.theme();

    service.toggle();

    const expectedTheme = initialTheme === 'light' ? 'dark' : 'light';
    expect(service.theme()).toBe(expectedTheme);
    expect(document.documentElement.dataset['theme']).toBe(expectedTheme);
    expect(localStorage.getItem('gestao-color-theme')).toBe(expectedTheme);
  });

  it('should restore a persisted theme', () => {
    localStorage.setItem('gestao-color-theme', 'dark');

    const service = TestBed.inject(ThemeService);

    expect(service.theme()).toBe('dark');
    expect(document.documentElement.dataset['theme']).toBe('dark');
  });

  it('should keep working when theme persistence is unavailable', () => {
    vi.spyOn(Storage.prototype, 'getItem').mockImplementation(() => {
      throw new DOMException('Storage unavailable', 'SecurityError');
    });
    vi.spyOn(Storage.prototype, 'setItem').mockImplementation(() => {
      throw new DOMException('Storage unavailable', 'QuotaExceededError');
    });

    const service = TestBed.inject(ThemeService);

    expect(() => service.toggle()).not.toThrow();
    expect(document.documentElement.dataset['theme']).toBe(service.theme());
  });
});
