import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { GameState } from '../../models/game.models';
import { GameStore } from '../../services/game.store';
import { GameComponent } from './game.component';

describe('GameComponent', () => {
  it('allows changing modes while an in-progress game has no moves', async () => {
    await TestBed.configureTestingModule({
      imports: [GameComponent],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    const fixture = TestBed.createComponent(GameComponent);
    TestBed.inject(GameStore).gameState.set(createGameState());
    fixture.detectChanges();

    const modePicker = fixture.nativeElement.querySelector('select') as HTMLSelectElement;

    expect(modePicker.disabled).toBeFalse();
  });

  it('locks mode selection after a move while the game is in progress', async () => {
    await TestBed.configureTestingModule({
      imports: [GameComponent],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    const fixture = TestBed.createComponent(GameComponent);
    TestBed.inject(GameStore).gameState.set(createGameState({
      moves: [{ moveNumber: 1, player: 'X', row: 0, column: 0 }],
    }));
    fixture.detectChanges();

    const modePicker = fixture.nativeElement.querySelector('select') as HTMLSelectElement;
    const modePickerLabel = fixture.nativeElement.querySelector('.mode-picker') as HTMLLabelElement;

    expect(modePicker.disabled).toBeTrue();
    expect(modePicker.title).toContain('locked after the first move');
    expect(modePickerLabel.title).toBe(modePicker.title);
  });

  it('allows changing modes after a game is finished', async () => {
    await TestBed.configureTestingModule({
      imports: [GameComponent],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    const fixture = TestBed.createComponent(GameComponent);
    TestBed.inject(GameStore).gameState.set(createGameState({
      status: 'Won',
      moves: [{ moveNumber: 1, player: 'X', row: 0, column: 0 }],
    }));
    fixture.detectChanges();

    const modePicker = fixture.nativeElement.querySelector('select') as HTMLSelectElement;

    expect(modePicker.disabled).toBeFalse();
  });

  it('disables Undo Last Move when canUndo is false', async () => {
    await TestBed.configureTestingModule({
      imports: [GameComponent],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    const fixture = TestBed.createComponent(GameComponent);
    TestBed.inject(GameStore).gameState.set(createGameState());
    fixture.detectChanges();

    const undoButton = Array.from(
      fixture.nativeElement.querySelectorAll('button') as NodeListOf<HTMLButtonElement>,
    ).find((button) => button.textContent?.includes('Undo Last Move'));

    expect(undoButton).toBeDefined();
    expect(undoButton?.disabled).toBeTrue();
  });
});

function createGameState(overrides: Partial<GameState> = {}): GameState {
  return {
    gameId: 'game-id',
    mode: 'TwoPlayer',
    status: 'InProgress',
    currentPlayer: 'X',
    winner: null,
    winningCells: [],
    board: [
      [null, null, null],
      [null, null, null],
      [null, null, null],
    ],
    moves: [],
    canUndo: false,
    scoreboard: {
      twoPlayer: { xWins: 0, oWins: 0, draws: 0 },
      computer: { xWins: 0, oWins: 0, draws: 0 },
    },
    ...overrides,
  };
}
