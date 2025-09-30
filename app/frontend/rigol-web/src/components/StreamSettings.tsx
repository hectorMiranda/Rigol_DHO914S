export interface StreamConfig {
  intervalMs: number;
  points: number;
}

interface Props {
  config: StreamConfig;
  connected: boolean;
  onChange: (config: StreamConfig) => void;
}

const INTERVALS = [50, 100, 200, 500, 1000];
const POINTS = [300, 600, 1200, 2400];

export function StreamSettings({ config, connected, onChange }: Props) {
  return (
    <section className="panel">
      <h2>Stream</h2>

      <label style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', padding: '4px 0', fontSize: 13 }}>
        <span style={{ color: 'var(--text-dim)' }}>Refresh</span>
        <select value={config.intervalMs} onChange={(e) => onChange({ ...config, intervalMs: Number(e.target.value) })}>
          {INTERVALS.map((ms) => (
            <option key={ms} value={ms}>
              {ms} ms ({Math.round(1000 / ms)} fps)
            </option>
          ))}
        </select>
      </label>

      <label style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', padding: '4px 0', fontSize: 13 }}>
        <span style={{ color: 'var(--text-dim)' }}>Points</span>
        <select value={config.points} onChange={(e) => onChange({ ...config, points: Number(e.target.value) })}>
          {POINTS.map((p) => (
            <option key={p} value={p}>
              {p}
            </option>
          ))}
        </select>
      </label>

      <div style={{ marginTop: 8, fontSize: 12, color: connected ? 'var(--ok)' : 'var(--text-dim)' }}>
        <span className={`dot ${connected ? 'dot--ok' : 'dot--warn'}`} /> {connected ? 'streaming' : 'idle'}
      </div>
    </section>
  );
}
