import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { PageHeader } from '../../shared/page-header/page-header';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [PageHeader, RouterLink],
  selector: 'app-dashboard-page',
  templateUrl: './dashboard-page.html',
  styleUrl: './dashboard-page.scss',
})
export class DashboardPage {
  protected readonly cards = [
    { label: 'Usuários', path: '/usuarios' },
    { label: 'Unidades', path: '/unidades' },
    { label: 'Colaboradores', path: '/colaboradores' },
  ] as const;
}
