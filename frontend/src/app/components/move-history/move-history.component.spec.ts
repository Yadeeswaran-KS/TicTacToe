import { TestBed } from '@angular/core/testing';
import { MoveHistoryComponent } from './move-history.component';

describe('MoveHistoryComponent', () => {
  it('renders move rows with one-based Row and Column positions', async () => {
    await TestBed.configureTestingModule({
      imports: [MoveHistoryComponent],
    }).compileComponents();

    const fixture = TestBed.createComponent(MoveHistoryComponent);
    fixture.componentRef.setInput('moves', [
      { moveNumber: 1, player: 'X', row: 0, column: 2 },
      { moveNumber: 2, player: 'O', row: 2, column: 1 },
    ]);
    fixture.detectChanges();

    const rows = fixture.nativeElement.querySelectorAll('tbody tr') as NodeListOf<HTMLTableRowElement>;
    expect(rows.length).toBe(2);
    expect(rows[0].textContent).toContain('Row 1, Column 3');
    expect(rows[1].textContent).toContain('Row 3, Column 2');
  });
});
