import { useEffect, useState } from 'react';
import { api } from '../api/client';
import type { MaskResult } from '../api/types';

export interface MaskConfig {
  enabled: boolean;
  lower: number;
  upper: number;
}

interface Props {
  channel: number;
  config: MaskConfig;
  onChange: (config: MaskConfig) => void;
}

/** Pass/fail mask band for the analysed channel; polls the test while enabled. */
export function MaskPanel({ channel, config, onChange }: Props) {
  const [result, setResult] = useState<MaskResult | null>(null);

  useEffect(() => {
    if (!config.enabled) {
      setResult(null);
      return;
    }
    let active = true;
    const tick = async () => {
      try {
        const r = await api.maskTest({ channel, lowerVolts: config.lower, upperVolts: config.upper });
        if (active) setResult(r);
      } catch {
        /* ignore */
      }
    };
    void tick();
    const id = setInterval(tick, 500);
    return () => {
      active = false;
      clearInterval(id);
    };
  }, [channel, config.enabled, config.lower, config.upper]);

  return (
    <section className="panel">
      <h2>Mask test · CH{channel}</h2>

      <div style={{ display: 'flex', gap: 6, alignItems: 'center', marginBottom: 8 }}>
        <button className={config.enabled ? 'is-active' : ''} onClick={() => onChange({ ...config, enabled: !config.enabled })}>
          {config.enabled ? 'On' : 'Off'}
        </button>
        {result && (
          <span style={{ fontWeight: 600, color: result.pass ? 'var(--ok)' : 'var(--err)' }}>
            {result.pass ? 'PASS' : 'FAIL'}
            {!result.pass && ` · ${result.violations}/${result.total}`}
          </span>
        )}
      </div>

      <div style={{ display: 'flex', gap: 8, opacity: config.enabled ? 1 : 0.5 }}>
        <label style={{ fontSize: 13, color: 'var(--text-dim)' }}>
          Low{' '}
          <input type="number" step="0.1" value={config.lower} onChange={(e) => onChange({ ...config, lower: Number(e.target.value) })} style={{ width: 70 }} />
        </label>
        <label style={{ fontSize: 13, color: 'var(--text-dim)' }}>
          High{' '}
          <input type="number" step="0.1" value={config.upper} onChange={(e) => onChange({ ...config, upper: Number(e.target.value) })} style={{ width: 70 }} />
        </label>
      </div>
    </section>
  );
}
