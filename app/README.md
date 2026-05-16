# Rigol DHO914S — Web Stream

A full-stack companion to the Python `rigol_dho914s` library that turns the
oscilloscope into a browser-based instrument. It streams live waveforms,
exposes channel/trigger/measurement control over HTTP, and renders the scope
graticule in the browser on an HTML canvas.

```
┌──────────────────┐      HTTP / SSE      ┌───────────────────────┐     SCPI / VISA
│  rigol-web        │ ───────────────────▶│  RigolStream.Api       │ ──────────────▶  DHO914S
│  (React + Vite)   │ ◀─────────────────── │  (.NET Azure Functions)│ ◀──────────────  (or simulator)
└──────────────────┘   waveforms, JSON    └───────────────────────┘
```

## Why

The Python tooling in this repo is great for scripted capture, but there was no
*live* view you could open on any device on the bench network. This app fills
that gap:

- **`backend/`** — a .NET 9 isolated **Azure Functions** API. It speaks to the
  scope through an `IOscilloscope` abstraction, so it runs against a real
  DHO914S over SCPI **or** a built-in signal simulator when no hardware is
  attached. Endpoints cover device info, per-channel config, single-shot
  waveform capture, automatic measurements, screenshots, run control, and a
  Server-Sent-Events live stream.
- **`frontend/`** — a React + TypeScript single-page app (Vite). It renders a
  proper oscilloscope display (graticule, multi-channel traces, legend) fed by
  the live stream, with side panels for channels, trigger, and measurements.

## Quick start

```bash
# 1. backend  (http://localhost:7071)
cd backend
func start            # or: dotnet run --project src/RigolStream.Api

# 2. frontend (http://localhost:5173)
cd frontend/rigol-web
npm install
npm run dev
```

With no scope connected the API falls back to the simulator, so the whole stack
is runnable on a laptop. Point it at real hardware by setting
`Oscilloscope:Mode = Scpi` and `Oscilloscope:Resource` in
`local.settings.json` (see `local.settings.sample.json`).

## Features

- **Live scope display** — multi-channel canvas traces, graticule, trigger
  marker, afterglow persistence, and time cursors with Δt/ΔV readout.
- **FFT spectrum** and **XY (Lissajous)** views, switchable from the toolbar.
- **MATH channel** (A ± / × B) overlaid on the scope.
- **Measurements** with live trend sparklines; **auto-set**; **CSV export**.
- **Recorder** — capture the stream and scrub back through frames.
- **Save/recall setups**, **UART decode**, and **pass/fail mask testing**.
- Light/dark theme, keyboard shortcuts, persisted settings.

See [`docs/architecture.md`](docs/architecture.md), [`docs/api.md`](docs/api.md)
and [`docs/deploy.md`](docs/deploy.md) for details.

## Layout

```
app/
  backend/    RigolStream.sln — .NET 9 Azure Functions API + xUnit tests
  frontend/   rigol-web — Vite + React + TypeScript SPA
  docs/       architecture & HTTP API reference
  docker-compose.yml
```
