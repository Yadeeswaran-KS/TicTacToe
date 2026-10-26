import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../environments/environment';
import { GameState, Scoreboards } from '../models/game.models';
import { GameApiService } from './game-api.service';

describe('GameApiService', () => {
  const apiBaseUrl = environment.apiBaseUrl;
  let service: GameApiService;
  let httpTestingController: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [GameApiService, provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(GameApiService);
    httpTestingController = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTestingController.verify();
  });

  it('creates a game with POST /api/games', () => {
    let response: GameState | undefined;

    service.createGame('Computer').subscribe((state) => {
      response = state;
    });

    const request = httpTestingController.expectOne(`${apiBaseUrl}/api/games`);
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({ mode: 'Computer' });
    expectSessionHeader(request.request.headers.get('X-Session-Id'));
    request.flush(createGameState());
    expect(response).toEqual(createGameState());
  });

  it('gets a game with GET /api/games/{id}', () => {
    service.getGame('game-id').subscribe();

    const request = httpTestingController.expectOne(`${apiBaseUrl}/api/games/game-id`);
    expect(request.request.method).toBe('GET');
    expectSessionHeader(request.request.headers.get('X-Session-Id'));
    request.flush(createGameState());
  });

  it('reuses the same session ID for subsequent API requests', () => {
    service.createGame('TwoPlayer').subscribe();
    const createRequest = httpTestingController.expectOne(`${apiBaseUrl}/api/games`);
    const sessionId = createRequest.request.headers.get('X-Session-Id');
    expectSessionHeader(sessionId);
    createRequest.flush(createGameState());

    service.getScoreboard().subscribe();
    const scoreboardRequest = httpTestingController.expectOne(`${apiBaseUrl}/api/scoreboard`);
    expect(scoreboardRequest.request.headers.get('X-Session-Id')).toBe(sessionId);
    scoreboardRequest.flush(createScoreboards());
  });

  it('makes a move with POST /api/games/{id}/moves and converts index to row and column', () => {
    service.makeMove('game-id', 'O', 5).subscribe();

    const request = httpTestingController.expectOne(
      `${apiBaseUrl}/api/games/game-id/moves`,
    );
    expect(request.request.method).toBe('POST');
    expectSessionHeader(request.request.headers.get('X-Session-Id'));
    expect(request.request.body).toEqual({
      player: 'O',
      row: 1,
      column: 2,
    });
    request.flush(createGameState());
  });

  it('undoes with POST /api/games/{id}/undo', () => {
    service.undo('game-id').subscribe();

    const request = httpTestingController.expectOne(
      `${apiBaseUrl}/api/games/game-id/undo`,
    );
    expect(request.request.method).toBe('POST');
    expectSessionHeader(request.request.headers.get('X-Session-Id'));
    expect(request.request.body).toEqual({});
    request.flush(createGameState());
  });

  it('resets a game with POST /api/games/{id}/reset', () => {
    service.reset('game-id').subscribe();

    const request = httpTestingController.expectOne(
      `${apiBaseUrl}/api/games/game-id/reset`,
    );
    expect(request.request.method).toBe('POST');
    expectSessionHeader(request.request.headers.get('X-Session-Id'));
    expect(request.request.body).toEqual({});
    request.flush(createGameState());
  });

  it('gets the scoreboard with GET /api/scoreboard', () => {
    let response: Scoreboards | undefined;

    service.getScoreboard().subscribe((scoreboard) => {
      response = scoreboard;
    });

    const request = httpTestingController.expectOne(`${apiBaseUrl}/api/scoreboard`);
    expect(request.request.method).toBe('GET');
    expectSessionHeader(request.request.headers.get('X-Session-Id'));
    request.flush(createScoreboards());
    expect(response).toEqual(createScoreboards());
  });

  it('resets only the selected scoreboard with POST /api/scoreboard/reset?mode=Computer', () => {
    let response: Scoreboards | undefined;

    service.resetScoreboard('Computer').subscribe((scoreboard) => {
      response = scoreboard;
    });

    const request = httpTestingController.expectOne(
      `${apiBaseUrl}/api/scoreboard/reset?mode=Computer`,
    );
    expect(request.request.method).toBe('POST');
    expectSessionHeader(request.request.headers.get('X-Session-Id'));
    expect(request.request.body).toEqual({});
    request.flush(createScoreboards());
    expect(response).toEqual(createScoreboards());
  });
});

function expectSessionHeader(sessionId: string | null): void {
  expect(sessionId).toMatch(/^[0-9a-f-]{36}$/i);
}

function createScoreboards(): Scoreboards {
  return {
    twoPlayer: { xWins: 2, oWins: 1, draws: 3 },
    computer: { xWins: 4, oWins: 5, draws: 6 },
  };
}

function createGameState(): GameState {
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
  };
}
