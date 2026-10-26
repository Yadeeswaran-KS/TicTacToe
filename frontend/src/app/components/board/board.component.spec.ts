import { TestBed } from '@angular/core/testing';
import { BoardComponent } from './board.component';

describe('BoardComponent', () => {
  it('renders an accessible 3-by-3 grid and disables occupied or completed cells', async () => {
    await TestBed.configureTestingModule({
      imports: [BoardComponent],
    }).compileComponents();

    const fixture = TestBed.createComponent(BoardComponent);
    fixture.componentRef.setInput('board', [
      ['X', null, null],
      [null, 'O', null],
      [null, null, null],
    ]);
    fixture.componentRef.setInput('gameOver', false);
    fixture.detectChanges();

    const buttons = fixture.nativeElement.querySelectorAll('button') as NodeListOf<HTMLButtonElement>;
    expect(buttons.length).toBe(9);
    expect(buttons[0].disabled).toBeTrue();
    expect(buttons[1].disabled).toBeFalse();
    expect(buttons[1].getAttribute('aria-label')).toBe('Row 1, Column 2, empty');

    fixture.componentRef.setInput('gameOver', true);
    fixture.detectChanges();
    expect(buttons[1].disabled).toBeTrue();
  });

  it('highlights winning cells and emits their row-major index when selected', async () => {
    await TestBed.configureTestingModule({
      imports: [BoardComponent],
    }).compileComponents();

    const fixture = TestBed.createComponent(BoardComponent);
    fixture.componentRef.setInput('board', [
      ['X', 'X', 'X'],
      [null, null, null],
      [null, null, null],
    ]);
    fixture.componentRef.setInput('winningCells', [0, 1, 2]);
    fixture.componentRef.setInput('gameOver', true);
    fixture.detectChanges();

    const buttons = fixture.nativeElement.querySelectorAll('button') as NodeListOf<HTMLButtonElement>;
    expect(buttons[2].classList).toContain('winning-cell');

    fixture.componentRef.setInput('gameOver', false);
    fixture.detectChanges();
    let selectedIndex: number | undefined;
    fixture.componentInstance.cellSelected.subscribe((index) => {
      selectedIndex = index;
    });
    buttons[5].click();

    expect(selectedIndex).toBe(5);
  });
});
