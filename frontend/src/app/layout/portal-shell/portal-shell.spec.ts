import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { PortalShell } from './portal-shell';

describe('PortalShell', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PortalShell],
      providers: [provideRouter([])],
    }).compileComponents();
  });

  it('should render every primary navigation entry', () => {
    const fixture = TestBed.createComponent(PortalShell);
    fixture.detectChanges();
    const links = Array.from(
      (fixture.nativeElement as HTMLElement).querySelectorAll<HTMLAnchorElement>('nav a'),
    ).map((link) => link.textContent?.trim());

    expect(links).toEqual(['Visão geral', 'Usuários', 'Unidades', 'Colaboradores']);
  });

  it('should expose the mobile menu state to assistive technologies', () => {
    const fixture = TestBed.createComponent(PortalShell);
    fixture.detectChanges();
    const button = (fixture.nativeElement as HTMLElement).querySelector<HTMLButtonElement>(
      '[aria-controls="navegacao-principal"]',
    );

    expect(button?.getAttribute('aria-expanded')).toBe('false');
    button?.click();
    fixture.detectChanges();
    expect(button?.getAttribute('aria-expanded')).toBe('true');
  });

  it('should close the menu with Escape and return focus to its button', () => {
    const fixture = TestBed.createComponent(PortalShell);
    fixture.detectChanges();
    const button = (fixture.nativeElement as HTMLElement).querySelector<HTMLButtonElement>(
      '[aria-controls="navegacao-principal"]',
    );

    button?.click();
    fixture.detectChanges();
    document.dispatchEvent(new KeyboardEvent('keydown', { key: 'Escape' }));
    fixture.detectChanges();

    expect(button?.getAttribute('aria-expanded')).toBe('false');
    expect(document.activeElement).toBe(button);
  });

  it('should move focus into the opened menu and contain keyboard navigation', async () => {
    const fixture = TestBed.createComponent(PortalShell);
    fixture.detectChanges();
    const element = fixture.nativeElement as HTMLElement;
    const button = element.querySelector<HTMLButtonElement>(
      '[aria-controls="navegacao-principal"]',
    );
    const menuItems = Array.from(element.querySelectorAll<HTMLElement>('nav a, nav button'));

    button?.click();
    fixture.detectChanges();
    await fixture.whenStable();
    expect(document.activeElement).toBe(menuItems[0]);

    menuItems.at(-1)?.focus();
    document.dispatchEvent(new KeyboardEvent('keydown', { key: 'Tab', cancelable: true }));
    expect(document.activeElement).toBe(menuItems[0]);

    document.dispatchEvent(
      new KeyboardEvent('keydown', { key: 'Tab', shiftKey: true, cancelable: true }),
    );
    expect(document.activeElement).toBe(menuItems.at(-1));
  });

  it('should release the mobile menu state when entering the desktop breakpoint', () => {
    const fixture = TestBed.createComponent(PortalShell);
    fixture.detectChanges();
    const button = (fixture.nativeElement as HTMLElement).querySelector<HTMLButtonElement>(
      '[aria-controls="navegacao-principal"]',
    );
    const originalWidth = window.innerWidth;

    button?.click();
    fixture.detectChanges();
    Object.defineProperty(window, 'innerWidth', { configurable: true, value: 1024 });
    window.dispatchEvent(new Event('resize'));
    fixture.detectChanges();

    expect(button?.getAttribute('aria-expanded')).toBe('false');
    Object.defineProperty(window, 'innerWidth', { configurable: true, value: originalWidth });
  });
});
