# TicTacToe

## Project overview

This repository contains a browser-based Tic Tac Toe application with an ASP.NET Core API and an Angular frontend. The API owns game state and scoring. Game and scoreboard data are held in memory by the running API process.

## Tech stack

Versions below are the target frameworks and dependency specifications present in the project files; package ranges are reproduced as declared.

| Area | Technology | Declared version |
|---|---|---|
| Backend | .NET target framework | `net10.0` |
| Backend API | Swashbuckle.AspNetCore | `10.3.1` |
| Backend tests | Microsoft.AspNetCore.Mvc.Testing | `10.0.0` |
| Backend tests | Microsoft.NET.Test.Sdk | `17.14.1` |
| Backend tests | xUnit | `2.9.3` |
| Backend tests | xunit.runner.visualstudio | `3.1.4` |
| Frontend | Angular framework packages (`@angular/core`, common, compiler, forms, platform-browser, router) | `^22.2.3` |
| Frontend | Angular CLI and build-angular | `^22.2.4` |
| Frontend | TypeScript | `~6.0.3` |
| Frontend | RxJS | `~7.8.0` |
| Frontend runtime | Node.js engine range | `^24.15.0 || ^26.0.0` |
| Frontend runtime | `.node-version` pin | `24.22.1` |

## Features implemented

- [x] 3-by-3 board with alternating turns and server-side turn validation.
- [x] Win and draw detection.
- [x] Winning-cell highlighting.
- [x] Reset game.
- [x] Move history.
- [x] Undo in both modes; in Computer mode, undo removes the computer reply and the preceding human move.
- [x] Separate Two Player and Computer scoreboards with individual reset actions.
- [x] Computer mode with an automatic O response.
- [x] Mode selection is locked after the first move while that game remains in progress. It is available before the first move and after completion.

## Repository structure

```text
.
├── .gitignore                                 Ignores .NET DLL and PDB files
├── backend/                                  .NET solution and source
│   ├── TicTacToe.sln                         Backend solution
│   ├── src/TicTacToe.Api/                    HTTP API, controllers, contracts, configuration
│   ├── src/TicTacToe.Application/             Game service and response DTOs
│   ├── src/TicTacToe.Domain/                  Game rules, game, players, scoreboards
│   ├── tests/TicTacToe.Tests/                 xUnit unit and API integration tests
│   └── README.md                              Backend architecture and test guide
├── frontend/                                 Angular application
│   ├── package.json                          Scripts and dependency declarations
│   ├── .node-version                         Node.js version pin
│   ├── angular.json                          Angular build, serve, and test configuration
│   ├── README.md                              Frontend architecture and test guide
│   └── src/app/                              Components, services, models, and specs
├── docs/API.md                               HTTP API contract and examples
├── README.md                                 Repository guide
└── frontend/README.md                        Frontend guide
```

## How to run the backend locally

Prerequisite: the .NET 10 SDK. From the `backend/` directory, run the API with the HTTP launch profile:

```powershell
dotnet run --project src/TicTacToe.Api/TicTacToe.Api.csproj --launch-profile http
```

The `http` profile in `src/TicTacToe.Api/Properties/launchSettings.json` listens at `http://localhost:5218`. The `https` profile uses `https://localhost:7184` and `http://localhost:5218`. Swagger is registered for the Development environment and is available at `http://localhost:5218/swagger` with the `http` profile.

## How to run the frontend locally

Prerequisites: Node.js matching `frontend/.node-version` (24.22.1) and npm. From the `frontend/` directory:

```powershell
npm install
npm start
```

Angular CLI serves the development app at `http://localhost:4200`. The frontend reads its API base URL from `src/environments/environment.ts` and appends `/api/games` or `/api/scoreboard`. There is no API proxy configured in `angular.json`. Every API request must include an `X-Session-Id` GUID; the frontend creates or reuses this ID in browser `sessionStorage`, so games and scoreboards are isolated by the stored session ID.

## API endpoint summary

| Method | Path | Purpose | Request body / parameters | Success | Error statuses |
|---|---|---|---|---|---|
| `POST` | `/api/games` | Create a game in the requested mode | `{"mode":"TwoPlayer"}` or `{"mode":"Computer"}` | `201 Created` | `400` invalid or missing request data |
| `GET` | `/api/games/{id}` | Read a game by GUID | None | `200 OK` | `404` game not found |
| `POST` | `/api/games/{id}/moves` | Make a move; Computer mode also makes its automatic reply when applicable | `{"player":"X","row":0,"column":0}` (row and column are zero-based) | `200 OK` | `400` invalid request or out-of-bounds; `404` game not found; `409` game-rule violation |
| `POST` | `/api/games/{id}/undo` | Undo moves according to the game's mode | No body required; frontend sends `{}` | `200 OK` | `404` game not found; `409` undo rule violation |
| `POST` | `/api/games/{id}/reset` | Clear the selected game's moves and start it again | No body required; frontend sends `{}` | `200 OK` | `404` game not found |
| `GET` | `/api/scoreboard` | Read both mode scoreboards | None | `200 OK` | `400` missing or invalid session ID |
| `POST` | `/api/scoreboard/reset?mode={mode}` | Reset only `TwoPlayer` or `Computer`; returns both scoreboards | Query parameter `mode` is required; no body required (frontend sends `{}`) | `200 OK` | `400` missing or invalid mode |

All endpoints require the `X-Session-Id` request header containing a non-empty GUID. A missing or invalid header returns `400 Bad Request`. Game IDs are scoped to the session that created them; a game requested from another session returns `404 Not Found`. The two mode scoreboards are separate within each session.

Game endpoints return this `GameStateResponse` shape (example values shown for a newly created Two Player game):

```json
{
  "gameId": "11111111-1111-1111-1111-111111111111",
  "mode": "TwoPlayer",
  "status": "InProgress",
  "currentPlayer": "X",
  "winner": null,
  "winningCells": [],
  "board": [
    [null, null, null],
    [null, null, null],
    [null, null, null]
  ],
  "moves": [],
  "canUndo": false,
  "scoreboard": {
    "twoPlayer": { "xWins": 0, "oWins": 0, "draws": 0 },
    "computer": { "xWins": 0, "oWins": 0, "draws": 0 }
  }
}
```

The `gameId` above is an example GUID; the server generates a GUID for each created game. See [docs/API.md](./docs/API.md) for request, response, and error examples.

## How to run tests

From `backend/`, run the xUnit project through the solution:

```powershell
dotnet test TicTacToe.sln
```

The backend tests cover domain game rules and replay-derived state, computer move selection, application service scoring and resets, controller attributes, and HTTP integration including ProblemDetails responses.

From `frontend/`, run the Angular/Karma suite once in headless Chrome:

```powershell
npm test -- --watch=false --browsers=ChromeHeadless
```

The frontend specs cover application startup, board rendering and selection, move-history rendering, scoreboard rendering/reset mode, game-mode selection and controls, API request construction, and store state/error handling.

## AI tools and prompt summary

| Stage | Prompt summary | What the AI generated |
|---|---|---|
| Backend design | Propose a simple .NET Web API structure, domain/service/controller responsibilities, xUnit project, and REST contract for two-player and computer Tic Tac Toe. | Layered backend design, class responsibilities, endpoint contract, payloads, and error statuses. |
| Backend scaffold | Create the .NET 10 solution with a controller-based API and xUnit project; remove WeatherForecast and configure CORS, string enums, and Swagger. | Domain, Application, API, and test projects with project references and API configuration. |
| Domain rules | Implement the game domain with move history as the source of truth; derive board and status by replay, and support undo, reset, computer strategy, and scoreboard. | Domain enums, move record, game, rule exceptions, deterministic computer player, scoreboard, and domain tests. |
| Service and API | Add a thread-safe in-memory game service, exactly-once score updates, controllers for game and scoreboard operations, and ProblemDetails error mapping. | Service, response/request models, controllers, exception filter, and service/API tests. |
| Backend test coverage | Cover valid and invalid moves, turns, wins, draws, undo/reset, scoring, computer priorities, and HTTP behavior with `WebApplicationFactory`. | Expanded xUnit rule/service tests and API integration tests. |
| Frontend scaffold and API | Create a strict standalone Angular workspace and define API response types plus an `HttpClient` service for the backend. | Angular workspace, environment configuration, TypeScript models, API client, and HTTP provider setup. |
| Frontend state and UI | Use signals for API-backed state and errors; create standalone board, history, scoreboard, and game components with accessible controls and responsive SCSS. | Signal store, components/templates/styles, API-driven actions, and frontend unit tests. |
| Session isolation | Make games and scoreboards specific to a browser session, with the frontend sending a persistent session ID on API calls. | Session-scoped backend storage, request validation, frontend session header, tests, and updated API documentation. |
| Per-mode scoreboards | Keep separate scoreboards for Two Player and Computer modes, add independent resets, update game/API response shapes, and test mode-isolated scoring. | Per-mode backend scoreboards, mode-required reset endpoint, dual scoreboard UI, and backend/frontend tests. |
| Locking game mode selection | Permit mode changes before the first move and after completion, lock selection after a move during an in-progress game, and explain the lock with a tooltip. | Move-count/status-based selector lock, tooltip text, and tests for unlocked and locked states. |
| Repository documentation | Inspect the actual backend/frontend implementation and document project setup, API contracts, design, and tests without guessing missing configuration. | Root, backend, and frontend READMEs plus `docs/API.md`, including the configured backend/frontend port mismatch. |

## Design decisions

- The backend owns game state, move validation, computer moves, and scorekeeping; the frontend submits actions and renders returned state.
- The move list is the source of truth. Board contents, current player, status, winner, winning cells, and undo eligibility are derived by replaying the moves.
- Computer move priority is: take an immediate O win, block an immediate X win, take the center, take the first available corner, then take the first available cell.
- Undo in Two Player mode removes one move. In Computer mode, undo removes the last human move and its computer reply.
- **Option A: Undo is disabled after a game is completed.** The domain reports `canUndo` as false for completed games and rejects undo attempts with `UndoNotAllowed`.
- A guard flag on each game prevents recording its finished result more than once. Resetting that game clears its moves and the guard.
- Scoreboards are separate for `TwoPlayer` and `Computer`; a game's result is recorded on the scoreboard for its mode. Resetting a game does not reset either scoreboard.
- The UI mode selector is locked when the current game is in progress and its move list is non-empty. It is also disabled while a request is loading.
- The API exception filter maps out-of-bounds game-rule errors to `400`, missing games to `404`, and other game-rule errors to `409`. Request validation and invalid scoreboard reset modes also return `400`.
- Games and session-specific scoreboards are held in memory in the singleton `GameService`; no database or persistence layer is configured.

## Clarifications and assumptions

- Enum values in JSON are strings (`TwoPlayer`, `Computer`, `X`, `O`, `InProgress`, `Won`, `Draw`) because the API configures `JsonStringEnumConverter`.
- Coordinates in move requests and responses are zero-based. The UI displays positions as one-based row and column.
- Swagger is enabled only when the API runs in the Development environment.
- The Node version pin is 24.22.1, while `package.json` allows Node `^24.15.0 || ^26.0.0`.

## Known limitations

- Games and scoreboards are in-memory and are lost when the backend process restarts.
- Session entries have no expiration or cleanup mechanism and remain in memory for the lifetime of the backend process.
- Session isolation uses a client-generated GUID, not authentication. A client that knows another session ID can claim it; do not treat this identifier as an authorization boundary.
- The computer player uses a fixed priority strategy; it does not search all future game trees.

## Future improvements

- Persist games and scores if they need to survive backend restarts.
- Replace client-generated session IDs with authenticated identity if session ownership must be verified.
- Expand computer move selection if stronger play is required.