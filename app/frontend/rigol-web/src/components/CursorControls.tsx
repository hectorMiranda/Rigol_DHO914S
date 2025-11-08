import type { Cursors } from './ScopeDisplay';

interface Props {
  cursors: Cursors;
  onChange: (cursors: Cursors) => void;
}

/** Toggle and position the two time cursors; the readout is drawn on the scope. */
export function CursorControls({ cursors, onChange }: Props) {
  return (
    <section className="panel">
      <h2>Cursors</h2>

      <button
        className={cursors.enabled ? 'is-active' : ''}
        style={{ marginBottom: 8 }}
        onClick={() => onChange({ ...cursors, enabled: !cursors.enabled })}
      >
        {cursors.enabled ? 'Cursors on' : 'Cursors off'}
      </button>

      <div style={{ opacity: cursors.enabled ? 1 : 0.5, display: 'flex', flexDirection: 'column', gap: 6 }}>
        <Slider label="A" value={cursors.a} onChange={(a) => onChange({ ...cursors, a })} />
        <Slider label="B" value={cursors.b} onChange={(b) => onChange({ ...cursors, b })} />
      </div>
    </section>
  );
}

function Slider({ label, value, onChange }: { label: string; value: number; onChange: (v: number) => void }) {
  return (
    <label style={{ display: 'flex', alignItems: 'center', gap: 8, fontSize: 13 }}>
      <span style={{ color: 'var(--text-dim)', width: 14 }}>{label}</span>
      <input
        type="range"
        min={0}
        max={1}
        step={0.001}
        value={value}
        onChange={(e) => onChange(Number(e.target.value))}
        style={{ flex: 1 }}
      />
    </label>
  );
}
