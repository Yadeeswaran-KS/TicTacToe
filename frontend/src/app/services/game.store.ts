import { HttpErrorResponse } from '@angular/common/http';
import { computed, inject, Injectable, signal } from '@angular/core';
import { firstValueFrom, Observable } from 'rxjs';
import { GameApiService } from './game-api.service';
import { GameMode, GameState } from '../models/game.models';

@Injectable({
  providedIn: 'root',
})
export class GameStore {
  private readonly gameApi = inject(GameApiService);

  readonly gameState = signal<GameState | null>(null);
  readonly scoreboard = computed(() => this.gameState()?.scoreboard ?? null);
  readonly loading = signal(false);
  readonly errorMessage = signal<string | null>(null);

  startGame(mode: GameMode): Promise<void> {
    return this.runRequest(this.gameApi.createGame(mode), (state) => {
      this.gameState.set(state);
    });
  }

  playCell(index: number): Promise<void> {
    const state = this.gameState();
    if (!state || !state.currentPlayer) {
      return Promise.resolve();
    }

    return this.runRequest(
      this.gameApi.makeMove(state.gameId, state.currentPlayer, index),
      (nextState) => this.gameState.set(nextState),
    );
  }

  undo(): Promise<void> {
    const state = this.gameState();
    if (!state) {
      return Promise.resolve();
    }

    return this.runRequest(this.gameApi.undo(state.gameId), (nextState) => {
      this.gameState.set(nextState);
    });
  }

  resetGame(): Promise<void> {
    const state = this.gameState();
    if (!state) {
      return Promise.resolve();
    }

    return this.runRequest(this.gameApi.reset(state.gameId), (nextState) => {
      this.gameState.set(nextState);
    });
  }

  resetScoreboard(mode: GameMode): Promise<void> {
    return this.runRequest(this.gameApi.resetScoreboard(mode), (scoreboard) => {
      const state = this.gameState();
      if (state) {
        this.gameState.set({ ...state, scoreboard });
      }
    });
  }

  private async runRequest<T>(
    request: Observable<T>,
    onSuccess: (response: T) => void,
  ): Promise<void> {
    if (this.loading()) {
      return;
    }

    this.loading.set(true);
    this.errorMessage.set(null);

    try {
      const response = await firstValueFrom(request);
      onSuccess(response);
    } catch (error: unknown) {
      this.errorMessage.set(this.getErrorMessage(error));
    } finally {
      this.loading.set(false);
    }
  }

  private getErrorMessage(error: unknown): string {
    if (error instanceof HttpErrorResponse) {
      const body: unknown = error.error;
      if (
        typeof body === 'object' &&
        body !== null &&
        'detail' in body &&
        typeof body.detail === 'string' &&
        body.detail.length > 0
      ) {
        return body.detail;
      }
    }

    return 'The request failed. Please try again.';
  }
}
