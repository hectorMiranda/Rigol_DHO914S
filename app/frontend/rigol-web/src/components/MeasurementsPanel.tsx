import { useEffect, useState } from 'react';
import { useMeasurements } from '../hooks/useMeasurements';
import { formatMeasurement } from '../utils/format';
import { CHANNEL_COLORS } from './ScopeDisplay';
import { Sparkline } from './Sparkline';

interface Props {
  enabledChannels: number[];
}

const HISTORY = 120;

export function MeasurementsPanel({ enabledChannels }: Props) {
  const [selected, setSelected] = useState<number | null>(enabledChannels[0] ?? null);
  const [tracked, setTracked] = useState<string>('FREQuency');
  const [history, setHistory] = useState<Record<string, number[]>>({});

  // Keep the selection valid as channels toggle on/off.
  useEffect(() => {
    if (selected === null || !enabledChannels.includes(selected)) {
      setSelected(enabledChannels[0] ?? null);
    }
  }, [enabledChannels, selected]);

  // Reset trend history when the channel changes.
  useEffect(() => {
    setHistory({});
  }, [selected]);

  const data = useMeasurements(selected);

  // Append each fresh reading to the per-code history buffers.
  useEffect(() => {
    if (!data) return;
    setHistory((prev) => {
      const next: Record<string, number[]> = { ...prev };
      for (const m of data.items) {
        if (m.value === null) continue;
        const buf = (next[m.code] ?? []).concat(m.value);
        next[m.code] = buf.slice(-HISTORY);
      }
      return next;
    });
  }, [data]);

  const trackedUnit = data?.items.find((m) => m.code === tracked)?.unit ?? '';
  const trackedSeries = history[tracked] ?? [];
  const color = selected ? CHANNEL_COLORS[selected] : '#38bdf8';

  return (
    <section className="panel">
      <h2>Measurements</h2>

      <div style={{ display: 'flex', gap: 6, marginBottom: 10 }}>
        {enabledChannels.map((ch) => (
          <button
            key={ch}
            className={selected === ch ? 'is-active' : ''}
            style={{ borderColor: selected === ch ? CHANNEL_COLORS[ch] : undefined, color: selected === ch ? CHANNEL_COLORS[ch] : undefined }}
            onClick={() => setSelected(ch)}
          >
            CH{ch}
          </button>
        ))}
      </div>

      {data ? (
        <>
          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '6px 12px' }}>
            {data.items.map((m) => (
              <button
                key={m.code}
                onClick={() => setTracked(m.code)}
                title="Track this measurement"
                style={{
                  display: 'flex',
                  justifyContent: 'space-between',
                  fontSize: 13,
                  padding: '2px 4px',
                  background: 'transparent',
                  border: 'none',
                  borderBottom: tracked === m.code ? `1px solid ${color}` : '1px solid transparent',
                  cursor: 'pointer',
                }}
              >
                <span style={{ color: 'var(--text-dim)' }}>{m.name}</span>
                <span style={{ fontVariantNumeric: 'tabular-nums' }}>{formatMeasurement(m.value, m.unit)}</span>
              </button>
            ))}
          </div>

          <div style={{ marginTop: 10 }}>
            <div style={{ fontSize: 11, color: 'var(--text-dim)', marginBottom: 2 }}>
              {data.items.find((m) => m.code === tracked)?.name ?? tracked} — last {trackedSeries.length} ({trackedUnit})
            </div>
            <Sparkline values={trackedSeries} color={color} />
          </div>
        </>
      ) : (
        <p style={{ color: 'var(--text-dim)' }}>No channel selected.</p>
      )}
    </section>
  );
}
