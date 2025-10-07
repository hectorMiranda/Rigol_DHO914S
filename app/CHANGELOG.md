# Changelog

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
