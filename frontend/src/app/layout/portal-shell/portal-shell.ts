import {
  afterRenderEffect,
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  HostListener,
  inject,
  signal,
  viewChild,
  viewChildren,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NavigationEnd, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { filter } from 'rxjs';
import { AuthSession } from '../../core/auth/auth-session';
import { ThemeService } from '../../core/theme/theme-service';

interface NavigationItem {
  readonly icon: 'dashboard' | 'employees' | 'units' | 'users';
  readonly label: string;
  readonly path: string;
}

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, RouterLinkActive, RouterOutlet],
  selector: 'app-portal-shell',
  templateUrl: './portal-shell.html',
  styleUrl: './portal-shell.scss',
})
export class PortalShell {
  private readonly router = inject(Router);
  private readonly session = inject(AuthSession);
  protected readonly theme = inject(ThemeService);
  private readonly menuButton = viewChild.required<ElementRef<HTMLButtonElement>>('menuButton');
  private readonly menuLinks = viewChildren<ElementRef<HTMLElement>>('menuLink');

  protected readonly menuOpen = signal(false);
  protected readonly identity = this.session.identity;
  protected readonly navigation: readonly NavigationItem[] = [
    { icon: 'dashboard', label: 'Visão geral', path: '/dashboard' },
    { icon: 'users', label: 'Usuários', path: '/usuarios' },
    { icon: 'units', label: 'Unidades', path: '/unidades' },
    { icon: 'employees', label: 'Colaboradores', path: '/colaboradores' },
  ];

  constructor() {
    afterRenderEffect(() => {
      if (this.menuOpen()) {
        this.menuLinks()[0]?.nativeElement.focus();
      }
    });

    this.router.events
      .pipe(
        filter((event): event is NavigationEnd => event instanceof NavigationEnd),
        takeUntilDestroyed(),
      )
      .subscribe(() => {
        if (this.menuOpen()) {
          this.closeMenu();
        }
      });
  }

  @HostListener('document:keydown', ['$event'])
  protected handleKeydown(event: KeyboardEvent): void {
    if (event.key === 'Escape') {
      this.closeMenu();
      return;
    }

    if (event.key !== 'Tab' || !this.menuOpen()) {
      return;
    }

    const links = this.menuLinks();
    const first = links[0]?.nativeElement;
    const last = links[links.length - 1]?.nativeElement;

    if (event.shiftKey && document.activeElement === first) {
      event.preventDefault();
      last?.focus();
    } else if (!event.shiftKey && document.activeElement === last) {
      event.preventDefault();
      first?.focus();
    }
  }

  @HostListener('window:resize')
  protected handleResize(): void {
    if (window.innerWidth > 800 && this.menuOpen()) {
      this.menuOpen.set(false);
    }
  }

  protected closeMenu(): void {
    const wasOpen = this.menuOpen();
    this.menuOpen.set(false);
    if (wasOpen) {
      this.menuButton().nativeElement.focus();
    }
  }

  protected toggleMenu(): void {
    this.menuOpen.update((open) => !open);
  }

  protected toggleTheme(): void {
    this.theme.toggle();
  }

  protected logout(): void {
    this.session.clear();
    void this.router.navigate(['/login']);
  }
}
