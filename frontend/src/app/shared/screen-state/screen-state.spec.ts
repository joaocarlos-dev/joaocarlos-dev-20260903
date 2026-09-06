import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ScreenState } from './screen-state';

describe('ScreenState', () => {
  let fixture: ComponentFixture<ScreenState>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({ imports: [ScreenState] }).compileComponents();
    fixture = TestBed.createComponent(ScreenState);
    fixture.componentRef.setInput('kind', 'error');
    fixture.componentRef.setInput('title', 'Não foi possível carregar');
    fixture.componentRef.setInput('description', 'Tente novamente.');
  });

  it('should expose an assertive alert for errors', () => {
    fixture.detectChanges();
    const element = fixture.nativeElement as HTMLElement;

    expect(element.querySelector('[role="alert"]')).toBeTruthy();
    expect(element.querySelector('h2')?.textContent).toContain('Não foi possível carregar');
  });

  it('should emit the action when the action button is selected', () => {
    fixture.componentRef.setInput('actionLabel', 'Tentar novamente');
    const action = vi.fn();
    fixture.componentInstance.action.subscribe(action);
    fixture.detectChanges();

    (fixture.nativeElement as HTMLElement).querySelector<HTMLButtonElement>('button')?.click();

    expect(action).toHaveBeenCalledOnce();
  });
});
