import type { MathOp } from '../api/types';
import type { MathConfig } from '../hooks/useMath';

const OPS: { key: MathOp; symbol: string }[] = [
  { key: 'add', symbol: '+' },
  { key: 'subtract', symbol: '−' },
  { key: 'multiply', symbol: '×' },
];

interface Props {
  config: MathConfig;
  onChange: (config: MathConfig) => void;
}

/** Configures the MATH channel (A op B) overlaid in white on the scope. */
export function MathPanel({ config, onChange }: Props) {
  return (
    <section className="panel">
      <h2>Math</h2>

      <button
        className={config.enabled ? 'is-active' : ''}
        style={{ marginBottom: 8 }}
        onClick={() => onChange({ ...config, enabled: !config.enabled })}
      >
        {config.enabled ? 'MATH on' : 'MATH off'}
      </button>

      <div style={{ display: 'flex', gap: 6, alignItems: 'center', opacity: config.enabled ? 1 : 0.5 }}>
        <select value={config.a} onChange={(e) => onChange({ ...config, a: Number(e.target.value) })}>
          {[1, 2, 3, 4].map((c) => (
            <option key={c} value={c}>CH{c}</option>
          ))}
        </select>
        <select value={config.op} onChange={(e) => onChange({ ...config, op: e.target.value as MathOp })}>
          {OPS.map((o) => (
            <option key={o.key} value={o.key}>{o.symbol}</option>
          ))}
        </select>
        <select value={config.b} onChange={(e) => onChange({ ...config, b: Number(e.target.value) })}>
          {[1, 2, 3, 4].map((c) => (
            <option key={c} value={c}>CH{c}</option>
          ))}
        </select>
      </div>
    </section>
  );
}
