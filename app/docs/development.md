# Development

## Prerequisites

- .NET SDK 9.0
- Node.js 22+
- (optional) Azure Functions Core Tools v4 — `func`
- (optional) Docker, for the compose workflow

## Run locally

```bash
# Backend — http://localhost:7071
cd app/backend
cp src/RigolStream.Api/local.settings.sample.json src/RigolStream.Api/local.settings.json
func start                       # or: dotnet run --project src/RigolStream.Api

# Frontend — http://localhost:5173 (proxies /api to :7071)
cd app/frontend/rigol-web
npm install
npm run dev
```

With the default `Oscilloscope:Mode = Simulated`, no hardware is needed.

## Point at a real DHO914S

In `local.settings.json`:

```json
"Oscilloscope:Mode": "Scpi",
"Oscilloscope:Resource": "TCPIP::192.168.1.50::INSTR",
"Oscilloscope:TimeoutMs": "10000"
```

`Resource` accepts `TCPIP::host::INSTR`, `host`, or `host:port` (default 5555).

## Docker

```bash
cd app
docker compose up --build
# open http://localhost:8080
```

## Tests

```bash
# backend
cd app/backend && dotnet test

# frontend
cd app/frontend/rigol-web && npm test
```
