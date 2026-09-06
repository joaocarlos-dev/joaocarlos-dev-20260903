import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { PageHeader } from '../../shared/page-header/page-header';
import { ScreenState } from '../../shared/screen-state/screen-state';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [PageHeader, ScreenState],
  selector: 'app-feature-preview-page',
  template: `
    <app-page-header
      [eyebrow]="eyebrow"
      [title]="title"
      [description]="description"
    />
    <app-screen-state
      kind="empty"
      title="Área preparada para integração"
      description="Os controles e dados desta seção serão adicionados na etapa funcional correspondente."
    />
  `,
  styles: `
    app-screen-state {
      margin-top: 2.25rem;
    }
  `,
})
export class FeaturePreviewPage {
  private readonly route = inject(ActivatedRoute);

  protected readonly description = String(this.route.snapshot.data['description']);
  protected readonly eyebrow = String(this.route.snapshot.data['eyebrow']);
  protected readonly title = String(this.route.snapshot.data['title']);
}
