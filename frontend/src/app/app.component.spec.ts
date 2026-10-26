import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '../environments/environment';
import { AppComponent } from './app.component';
import { GameStore } from './services/game.store';

describe('AppComponent', () => {
  let httpTestingController: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AppComponent],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
    httpTestingController = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTestingController.verify();
  });

  it('starts a TwoPlayer game on load', async () => {
    const fixture = TestBed.createComponent(AppComponent);
    fixture.detectChanges();

    const request = httpTestingController.expectOne(`${environment.apiBaseUrl}/api/games`);
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({ mode: 'TwoPlayer' });
    request.flush(createGameState());
    await fixture.whenStable();
    expect(TestBed.inject(GameStore).gameState()).toEqual(createGameState());
  });
});

function createGameState() {
  return {
    gameId: 'game-id',
    mode: 'TwoPlayer' as const,
    status: 'InProgress' as const,
    currentPlayer: 'X' as const,
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
