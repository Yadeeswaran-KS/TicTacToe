import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../environments/environment';
import { GameState } from '../models/game.models';
import { GameStore } from './game.store';

describe('GameStore', () => {
  const apiBaseUrl = environment.apiBaseUrl;
  let store: GameStore;
  let httpTestingController: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [GameStore, provideHttpClient(), provideHttpClientTesting()],
    });
    store = TestBed.inject(GameStore);
    httpTestingController = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTestingController.verify();
  });

  it('replaces state with the response when starting a game', async () => {
    const action = store.startGame('Computer');
    expect(store.loading()).toBeTrue();

    httpTestingController.expectOne((request) =>
      request.url === `${apiBaseUrl}/api/games` &&
      request.method === 'POST' &&
      request.body.mode === 'Computer',
    ).flush(createGameState({ mode: 'Computer' }));

    await action;

    expect(store.gameState()?.mode).toBe('Computer');
    expect(store.loading()).toBeFalse();
    expect(store.errorMessage()).toBeNull();
  });

  it('uses the current player from state when making a move', async () => {
    store.gameState.set(createGameState({ currentPlayer: 'O' }));

    const action = store.playCell(5);
    const request = httpTestingController.expectOne(
      `${apiBaseUrl}/api/games/game-id/moves`,
    );
    expect(request.request.body).toEqual({
      player: 'O',
      row: 1,
      column: 2,
    });
    request.flush(createGameState({ currentPlayer: 'X' }));

    await action;

    expect(store.gameState()?.currentPlayer).toBe('X');
  });

  it('replaces state with undo and game-reset responses', async () => {
    store.gameState.set(createGameState());

    const undoAction = store.undo();
    httpTestingController.expectOne({
      method: 'POST',
      url: `${apiBaseUrl}/api/games/game-id/undo`,
    }).flush(createGameState({ currentPlayer: 'O' }));
    await undoAction;
    expect(store.gameState()?.currentPlayer).toBe('O');

    const resetAction = store.resetGame();
    const resetState = createGameState();
    httpTestingController.expectOne({
      method: 'POST',
      url: `${apiBaseUrl}/api/games/game-id/reset`,
    }).flush(resetState);
    await resetAction;
    expect(store.gameState()).toEqual(resetState);
  });

  it('leaves game state unchanged and displays ProblemDetails detail on error', async () => {
    const originalState = createGameState();
    store.gameState.set(originalState);

    const action = store.playCell(0);
    httpTestingController.expectOne(
      `${apiBaseUrl}/api/games/game-id/moves`,
    ).flush(
      { detail: 'That cell is already occupied.' },
      { status: 409, statusText: 'Conflict' },
    );

    await action;

    expect(store.gameState()).toBe(originalState);
    expect(store.errorMessage()).toBe('That cell is already occupied.');
    expect(store.loading()).toBeFalse();
  });

  it('updates both scoreboards returned by the selected scoreboard reset endpoint', async () => {
    const originalState = createGameState();
    store.gameState.set(originalState);

    const resetScoreboards = {
      twoPlayer: { xWins: 1, oWins: 0, draws: 0 },
      computer: { xWins: 0, oWins: 0, draws: 0 },
    };
    const action = store.resetScoreboard('Computer');
    httpTestingController.expectOne({
      method: 'POST',
      url: `${apiBaseUrl}/api/scoreboard/reset?mode=Computer`,
    }).flush(resetScoreboards);

    await action;

    expect(store.gameState()).toEqual({
      ...originalState,
      scoreboard: resetScoreboards,
    });
    expect(store.gameState()?.gameId).toBe(originalState.gameId);
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
