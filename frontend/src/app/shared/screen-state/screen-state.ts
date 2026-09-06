import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';

export type ScreenStateKind = 'empty' | 'error' | 'loading';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-screen-state',
  templateUrl: './screen-state.html',
  styleUrl: './screen-state.scss',
})
export class ScreenState {
  readonly actionLabel = input<string>();
  readonly description = input.required<string>();
  readonly kind = input.required<ScreenStateKind>();
  readonly title = input.required<string>();
  readonly action = output<void>();
}
