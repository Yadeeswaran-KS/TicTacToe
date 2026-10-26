export type Player = 'X' | 'O';

export type GameMode = 'TwoPlayer' | 'Computer';

export type GameStatus = 'InProgress' | 'Won' | 'Draw';

export interface Move {
  moveNumber: number;
  player: Player;
  row: number;
  column: number;
}

export interface Scoreboard {
  xWins: number;
  oWins: number;
  draws: number;
}

export interface Scoreboards {
  twoPlayer: Scoreboard;
  computer: Scoreboard;
}

export interface GameState {
  gameId: string;
  mode: GameMode;
  status: GameStatus;
  currentPlayer: Player | null;
  winner: Player | null;
  winningCells: number[];
  board: (Player | null)[][];
  moves: Move[];
  canUndo: boolean;
  scoreboard: Scoreboards;
}
