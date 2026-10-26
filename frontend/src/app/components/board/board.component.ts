import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { Player } from '../../models/game.models';

@Component({
  selector: 'app-board',
  standalone: true,
  templateUrl: './board.component.html',
  styleUrl: './board.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BoardComponent {
  readonly board = input.required<(Player | null)[][]>();
  readonly winningCells = input<number[]>([]);
  readonly gameOver = input(false);
  readonly cellSelected = output<number>();

  cellIndex(row: number, column: number): number {
    return row * 3 + column;
  }

  isWinningCell(row: number, column: number): boolean {
    return this.winningCells().includes(this.cellIndex(row, column));
  }

  selectCell(row: number, column: number): void {
    this.cellSelected.emit(this.cellIndex(row, column));
  }
}
