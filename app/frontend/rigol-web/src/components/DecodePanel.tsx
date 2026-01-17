import { useState } from 'react';
import { api } from '../api/client';
import type { UartDecodeResult } from '../api/types';

const BAUDS = [1200, 2400, 9600, 19200, 38400, 57600, 115200];

interface Props {
  channel: number;
}

/** On-demand UART decode of the analysed channel. */
export function DecodePanel({ channel }: Props) {
  const [baud, setBaud] = useState(9600);
  const [result, setResult] = useState<UartDecodeResult | null>(null);
  const [busy, setBusy] = useState(false);

  const decode = async () => {
    setBusy(true);
    try {
      setResult(await api.decodeUart(channel, baud));
    } catch {
      setResult(null);
    } finally {
      setBusy(false);
    }
  };

  return (
    <section className="panel">
      <h2>UART decode · CH{channel}</h2>

      <div style={{ display: 'flex', gap: 6, marginBottom: 8 }}>
        <select value={baud} onChange={(e) => setBaud(Number(e.target.value))}>
          {BAUDS.map((b) => (
            <option key={b} value={b}>{b} baud</option>
          ))}
        </select>
        <button onClick={decode} disabled={busy}>{busy ? '…' : 'Decode'}</button>
      </div>

      {result && (
        <div style={{ fontSize: 12, fontFamily: 'ui-monospace, monospace', color: 'var(--text-dim)' }}>
          {result.frames.length === 0 ? (
            <span>No frames (try another baud or channel).</span>
          ) : (
            <div style={{ display: 'flex', flexWrap: 'wrap', gap: 4 }}>
              {result.frames.slice(0, 64).map((f, i) => (
                <span
                  key={i}
                  title={f.framingError ? 'framing error' : `${f.time.toExponential(2)} s`}
                  style={{ color: f.framingError ? 'var(--err)' : 'var(--text)' }}
                >
                  {f.value.toString(16).padStart(2, '0').toUpperCase()}
                </span>
              ))}
            </div>
          )}
        </div>
      )}
    </section>
  );
}
