# Frontend

The Angular application is a standalone-component UI. `AppComponent` starts a Two Player game on load, and `GameComponent` displays the board, game controls, both scoreboards, and move history.

## Component tree

```text
AppComponent
└── GameComponent
    ├── BoardComponent
    ├── ScoreboardComponent (Two Player)
    ├── ScoreboardComponent (Computer)
    └── MoveHistoryComponent
```

`BoardComponent` renders the 3-by-3 board, disables occupied/completed cells, highlights winning cells, and emits selected cell indexes. `ScoreboardComponent` receives its title, labels, counts, active state, and mode; its reset action emits that mode. `MoveHistoryComponent` displays move number, player, and row/column.

## State and API services

- `GameStore` holds the current `GameState`, loading flag, and error message in Angular signals. It calls the API service for create, move, undo, game reset, and mode-specific scoreboard reset. Successful API responses replace or update displayed state.
- `GameApiService` uses Angular `HttpClient`. It reads `apiBaseUrl` from `src/environments/environment.ts`, creates an `X-Session-Id` GUID when the `tic-tac-toe-session-id` key is absent from browser `sessionStorage`, reuses the stored value otherwise, and sends it with every API request.
- The backend keeps each browser session's games and both mode scoreboards separate. The session ID provides separation, not authentication.
- `game.models.ts` defines the `GameMode`, `Player`, `GameStatus`, `Move`, `Scoreboard`, `Scoreboards`, and `GameState` TypeScript shapes.
- The frontend submits actions and renders returned game/scoreboard state; game rules and scoring are handled by the backend.
- Game mode selection is disabled while an API request is loading, or while an in-progress game has at least one move. The selector title tooltip explains whether mode selection is locked.

## API error display

`GameStore` checks an `HttpErrorResponse` body for a non-empty string `detail`. If found, it displays that detail. Otherwise it displays `The request failed. Please try again.` `GameComponent` renders the message in an element with `role="alert"`.

## Tests

- `app.component.spec.ts`: app startup requests a Two Player game and stores the response.
- `components/board/board.component.spec.ts`: nine accessible cells, disabling occupied/completed cells, winning highlight, and selected index.
- `components/game/game.component.spec.ts`: mode selection before first move, lock after a move, unlocking after completion, and undo disabled when unavailable.
- `components/move-history/move-history.component.spec.ts`: rendered moves use one-based positions.
- `components/scoreboard/scoreboard.component.spec.ts`: supplied title/labels/counts and reset emits the selected mode.
- `services/game-api.service.spec.ts`: HTTP paths, methods, request payloads, coordinate conversion, and scoreboard mode parameter.
- `services/game.store.spec.ts`: state updates, API error display, reset and undo responses, and selected scoreboard reset.

Run the tests from `frontend/` (Node.js 24.22.1 is pinned in `.node-version`; `package.json` accepts `^24.15.0 || ^26.0.0`):

```powershell
npm install
npm test -- --watch=false --browsers=ChromeHeadless
```

The app starts from `frontend/` with `npm start` and serves at `http://localhost:4200`. The checked-in API base URL is `http://localhost:5037`; the backend launch profiles use port 5218, so the configured URLs currently differ.
