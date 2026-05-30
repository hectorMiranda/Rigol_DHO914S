# HTTP API

Base URL: `/api` (route prefix set in `host.json`). All JSON is camelCase with
enums as camelCase strings. Errors return `{ status, kind, detail }` with an
appropriate status code.

| Method | Route | Description |
| ------ | ----- | ----------- |
| GET | `/health` | Liveness probe. |
| GET | `/device` | Instrument identity (`*IDN?`). |
| GET | `/status` | Device + acquisition + channels in one payload. |
| GET | `/channels` | All four channel configs. |
| GET | `/channels/{n}` | One channel config (n = 1‑4). |
| PATCH/PUT | `/channels/{n}` | Apply a partial `ChannelUpdate`. |
| GET | `/waveform/{n}?points=N` | Single-shot trace capture. |
| GET | `/measurements/{n}` | Default auto-measurement set. |
| GET | `/acquisition` | Timebase + trigger + run state. |
| POST | `/acquisition/{run\|stop\|single}` | Change run state. |
| PATCH/PUT | `/acquisition` | Update timebase/trigger (`AcquisitionUpdate`). |
| GET | `/screenshot` | Display as `image/png`. |
| GET | `/stream?channels=1,2&interval=100&points=600` | SSE live waveform stream. |
| GET | `/fft/{n}?points=2048&window=hann` | One-sided dBV magnitude spectrum. |
| GET | `/math/{add\|subtract\|multiply}?a=1&b=2` | Sample-wise MATH channel. |
| GET | `/export/{n}.csv?points=N` | Download trace as CSV. |
| POST | `/autoset` | Auto vertical/timebase scaling from measurements. |
| GET/POST/DELETE | `/setups[/{name}]` | List / save / delete named setups. |
| POST | `/setups/{name}/recall` | Re-apply a saved setup. |
| GET | `/decode/uart/{n}?baud=9600` | UART (8-N-1) protocol decode. |
| POST | `/mask/test` | Pass/fail mask test against a voltage band. |

## Examples

```bash
# Identity
curl localhost:7071/api/device

# Set CH2 to 0.2 V/div, AC coupled
curl -X PATCH localhost:7071/api/channels/2 \
  -H 'content-type: application/json' \
  -d '{"voltsPerDivision":0.2,"coupling":"ac"}'

# Capture 1200 points from CH1
curl 'localhost:7071/api/waveform/1?points=1200'

# Stop acquisition
curl -X POST localhost:7071/api/acquisition/stop

# Live stream (Server-Sent Events)
curl -N 'localhost:7071/api/stream?channels=1,2&interval=100&points=600'
```

## Stream frames

Each SSE `data:` line is a JSON object:

```json
{
  "t": 1733520000123,
  "frames": [
    { "channel": 1, "voltage": [/* volts */], "timeOrigin": -0.005,
      "timeIncrement": 8.3e-6, "timestamp": 1733520000120,
      "sampleCount": 600, "duration": 0.005 }
  ]
}
```

## Error shape

```json
{ "status": 400, "kind": "BadRequest", "detail": "Channel '9' must be 1-4" }
```

| Kind | Status | When |
| ---- | ------ | ---- |
| `BadRequest` / `Command` | 400 | bad argument / rejected command |
| `Connection` | 503 | cannot reach the instrument |
| `Timeout` | 504 | instrument did not respond in time |
| `Data` | 502 | malformed instrument response |
