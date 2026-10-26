import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { GameMode } from '../../models/game.models';

@Component({
  selector: 'app-scoreboard',
  standalone: true,
  templateUrl: './scoreboard.component.html',
  styleUrl: './scoreboard.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ScoreboardComponent {
  readonly title = input.required<string>();
  readonly mode = input.required<GameMode>();
  readonly xLabel = input.required<string>();
  readonly oLabel = input.required<string>();
  readonly drawsLabel = input.required<string>();
  readonly xWins = input.required<number>();
  readonly oWins = input.required<number>();
  readonly draws = input.required<number>();
  readonly active = input(false);
  readonly disabled = input(false);
  readonly resetRequested = output<GameMode>();
}
