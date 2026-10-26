import { ChangeDetectionStrategy, Component } from '@angular/core';
import { GameComponent } from './components/game/game.component';
import { GameStore } from './services/game.store';

@Component({
  selector: 'app-root',
  imports: [GameComponent],
  templateUrl: './app.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './app.component.scss'
})
export class AppComponent {
  readonly title = 'TicTacToeFrontend';

  constructor(gameStore: GameStore) {
    void gameStore.startGame('TwoPlayer');
  }
}
