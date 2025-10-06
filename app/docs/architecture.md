# Architecture

```
┌────────────────────────┐         ┌──────────────────────────────┐        ┌──────────────┐
│ rigol-web (React/Vite) │  HTTP   │ RigolStream.Api (Functions)   │  SCPI  │  DHO914S     │
│                        │ ──────▶ │                               │ ─────▶ │  hardware    │
│  ScopeDisplay (canvas) │  SSE    │  Functions ── IOscilloscope ──┤        └──────────────┘
│  panels + hooks        │ ◀────── │            ├ ScpiOscilloscope ┘  (port 5555, raw TCP)
└────────────────────────┘  JSON   │            └ SimulatedOscilloscope ◀── SignalGenerator
                                    └──────────────────────────────┘
```

## Backend (`app/backend`)

A .NET 9 **isolated** Azure Functions app using the ASP.NET Core HTTP
integration (so handlers take `HttpRequest`/return `IActionResult`).

- **`Functions/`** — thin HTTP adapters. They validate input, call the scope,
  and let `ApiResults.Execute` translate `OscilloscopeException` into the right
  status code. No business logic lives here.
- **`Devices/IOscilloscope`** — the one contract the HTTP layer depends on. Two
  implementations:
  - **`SimulatedOscilloscope`** — in-memory, synthesizes traces with
    `SignalGenerator`, derives measurements from the samples (`Statistics`) and
    renders the screenshot PNG with a hand-rolled encoder (`PngCanvas`). It is a
    singleton so channel/timebase edits persist across requests.
  - **`ScpiOscilloscope`** — drives real hardware over `IScpiTransport`
    (`TcpScpiTransport` = the LXI socket on port 5555), using the same SCPI
    dialect as the repo's Python library.
- **`Scpi/`** — command strings, the measurement catalogue, and `WaveformMath`
  (byte→volt conversion, IEEE 488.2 block stripping), all ported from Python.
- **`Models/`** — immutable DTOs serialized as camelCase JSON with string enums.

`OscilloscopeFactory` reads `Oscilloscope:Mode` (`Simulated` | `Scpi`) to choose
the implementation at startup.

## Live streaming

The `GET /api/stream` function holds the response open and writes a `data:`
frame per channel on a fixed cadence as **Server-Sent Events**. The browser
consumes it with a native `EventSource` (`useWaveformStream`), so there is no
WebSocket machinery and reconnection is automatic. `SseWriter` sets
`text/event-stream`, disables buffering, and flushes after every frame.

## Frontend (`app/frontend/rigol-web`)

- **`api/`** — typed DTOs + a small `fetch` wrapper (`ApiError` on non-2xx).
- **`hooks/`** — `useScopeStatus` (poll device/acquisition/channels),
  `useWaveformStream` (SSE), `useMeasurements` (poll per channel).
- **`components/ScopeDisplay`** — renders the 10×8 graticule and each trace onto
  a `<canvas>`, mapping volts to divisions via per-channel scale/offset, plus a
  dashed trigger-level marker.
- **`App.tsx`** — composes the display and side panels and applies optimistic
  updates that the status poll reconciles.

## Why a simulator

There isn't always a scope on the bench, and CI never has one. The simulator
makes the entire stack runnable and testable anywhere while keeping the real
SCPI path one config switch away.
