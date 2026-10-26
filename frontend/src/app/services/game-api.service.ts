import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  GameMode,
  GameState,
  Player,
  Scoreboards,
} from '../models/game.models';

@Injectable({
  providedIn: 'root',
})
export class GameApiService {
  private readonly http = inject(HttpClient);
  private readonly gamesUrl = `${environment.apiBaseUrl}/api/games`;
  private readonly scoreboardUrl = `${environment.apiBaseUrl}/api/scoreboard`;
  private readonly sessionId = this.getOrCreateSessionId();

  createGame(mode: GameMode): Observable<GameState> {
    return this.http.post<GameState>(
      this.gamesUrl,
      { mode },
      { headers: this.sessionHeaders },
    );
  }

  getGame(id: string): Observable<GameState> {
    return this.http.get<GameState>(
      `${this.gamesUrl}/${encodeURIComponent(id)}`,
      { headers: this.sessionHeaders },
    );
  }

  makeMove(id: string, player: Player, index: number): Observable<GameState> {
    return this.http.post<GameState>(
      `${this.gamesUrl}/${encodeURIComponent(id)}/moves`,
      {
        player,
        row: Math.floor(index / 3),
        column: index % 3,
      },
      { headers: this.sessionHeaders },
    );
  }

  undo(id: string): Observable<GameState> {
    return this.http.post<GameState>(
      `${this.gamesUrl}/${encodeURIComponent(id)}/undo`,
      {},
      { headers: this.sessionHeaders },
    );
  }

  reset(id: string): Observable<GameState> {
    return this.http.post<GameState>(
      `${this.gamesUrl}/${encodeURIComponent(id)}/reset`,
      {},
      { headers: this.sessionHeaders },
    );
  }

  getScoreboard(): Observable<Scoreboards> {
    return this.http.get<Scoreboards>(this.scoreboardUrl, {
      headers: this.sessionHeaders,
    });
  }

  resetScoreboard(mode: GameMode): Observable<Scoreboards> {
    return this.http.post<Scoreboards>(
      `${this.scoreboardUrl}/reset`,
      {},
      { headers: this.sessionHeaders, params: { mode } },
    );
  }

  private get sessionHeaders(): { 'X-Session-Id': string } {
    return { 'X-Session-Id': this.sessionId };
  }

  private getOrCreateSessionId(): string {
    const storageKey = 'tic-tac-toe-session-id';
    const existingSessionId = sessionStorage.getItem(storageKey);
    if (existingSessionId) {
      return existingSessionId;
    }

    const sessionId = crypto.randomUUID();
    sessionStorage.setItem(storageKey, sessionId);
    return sessionId;
  }
}
