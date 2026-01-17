# Changelog

## 0.3.0 — 2026-01-17

Capture & protocol release.

- **Stream recorder** — capture the live stream to a ring buffer and scrub back
  through frames in a playback mode.
- **Save/recall setups** — name and store instrument state (channels +
  acquisition) and re-apply it. `GET/POST/DELETE /api/setups`,
  `POST /api/setups/{name}/recall`.
- **UART decode** — configurable 8-N-1 async-serial decoder with auto threshold,
  shown as hex with framing-error flags. `GET /api/decode/uart/{channel}`.
- 5 new backend tests (UART round-trip).

## 0.2.0 — 2025-11-22

Analysis release.

- **FFT spectrum view** — radix-2 FFT with selectable windows (Hann, Hamming,
  Blackman, flat-top), one-sided dBV magnitude, peak/fundamental marker, and a
  Scope/FFT tab switcher. `GET /api/fft/{channel}`.
- **Math channel** — `A op B` (add/subtract/multiply) overlaid as a MATH trace.
  `GET /api/math/{op}`.
- **Time cursors** — two draggable cursors with on-canvas Δt / 1÷Δt / ΔV readout.
- **CSV export** — `GET /api/export/{channel}.csv` + a toolbar download link.
- **Measurement trends** — per-measurement sparklines over the last 120 readings.
- 7 new backend tests (FFT/windows).

## 0.1.0 — 2025-10-07

First full-stack release of the DHO914S web stream, merged from `develop`.

### Backend — RigolStream.Api (.NET 9 isolated Azure Functions)
- `IOscilloscope` abstraction with a built-in **simulator** and a real **SCPI**
  driver over the LXI socket (port 5555), selectable via `Oscilloscope:Mode`.
- Endpoints: device, status, channels (read + PATCH), waveform, measurements,
  acquisition run-control + timebase/trigger, screenshot, health.
- **Server-Sent-Events** live waveform stream (`/api/stream`).
- Dependency-free PNG encoder so the simulator renders real screenshots.
- SCPI command set, waveform math and measurement catalogue ported from the
  repo's Python library so both stacks speak the same dialect.
- 15 xUnit tests.

### Frontend — rigol-web (React + Vite + TypeScript)
- Canvas oscilloscope display (graticule, multi-channel traces, trigger marker,
  legend) fed by the live stream via `EventSource`.
- Panels: instrument info, channel controls, acquisition/trigger, measurements,
  stream settings, screenshot capture.
- Typed API client and 9 vitest tests.

### Ops
- Multi-stage Dockerfiles + `docker-compose.yml` (API + nginx-served SPA).
- GitHub Actions CI building and testing both stacks.
