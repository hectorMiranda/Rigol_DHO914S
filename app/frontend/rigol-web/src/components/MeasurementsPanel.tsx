import { useEffect, useState } from 'react';
import { useMeasurements } from '../hooks/useMeasurements';
import { formatMeasurement } from '../utils/format';
import { CHANNEL_COLORS } from './ScopeDisplay';

interface Props {
  enabledChannels: number[];
}

export function MeasurementsPanel({ enabledChannels }: Props) {
  const [selected, setSelected] = useState<number | null>(enabledChannels[0] ?? null);

  // Keep the selection valid as channels toggle on/off.
  useEffect(() => {
    if (selected === null || !enabledChannels.includes(selected)) {
      setSelected(enabledChannels[0] ?? null);
    }
  }, [enabledChannels, selected]);

  const data = useMeasurements(selected);

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
        <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '6px 12px' }}>
          {data.items.map((m) => (
            <div key={m.code} style={{ display: 'flex', justifyContent: 'space-between', fontSize: 13 }}>
              <span style={{ color: 'var(--text-dim)' }}>{m.name}</span>
              <span style={{ fontVariantNumeric: 'tabular-nums' }}>{formatMeasurement(m.value, m.unit)}</span>
            </div>
          ))}
        </div>
      ) : (
        <p style={{ color: 'var(--text-dim)' }}>No channel selected.</p>
      )}
    </section>
  );
}
