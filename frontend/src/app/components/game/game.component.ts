import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { GameMode } from '../../models/game.models';
import { GameStore } from '../../services/game.store';
import { BoardComponent } from '../board/board.component';
import { MoveHistoryComponent } from '../move-history/move-history.component';
import { ScoreboardComponent } from '../scoreboard/scoreboard.component';

@Component({
  selector: 'app-game',
  standalone: true,
  imports: [BoardComponent, MoveHistoryComponent, ScoreboardComponent],
  templateUrl: './game.component.html',
  styleUrl: './game.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class GameComponent {
  readonly store = inject(GameStore);
  readonly selectedMode = signal<GameMode>('TwoPlayer');
  readonly modeSelectionLocked = computed(() => {
    const game = this.store.gameState();
    return game?.status === 'InProgress' && game.moves.length > 0;
  });
  readonly modeSelectorTooltip = computed(() =>
    this.modeSelectionLocked()
      ? 'Game mode is locked after the first move. Reset the game to choose another mode.'
      : 'Choose a game mode. You can change modes until the first move.',
  );
  readonly statusLine = computed(() => {
    const state = this.store.gameState();
    if (!state) {
      return 'Starting game…';
    }
    if (state.status === 'Won' && state.winner) {
      return `${state.winner} wins!`;
    }
    if (state.status === 'Draw') {
      return "It's a draw";
    }
    return `${state.currentPlayer}'s turn`;
  });

  onModeChange(event: Event): void {
    const mode = (event.target as HTMLSelectElement).value as GameMode;
    this.selectedMode.set(mode);
    void this.store.startGame(mode);
  }
}
