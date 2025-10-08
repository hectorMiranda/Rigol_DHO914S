import type { AcquisitionState, AcquisitionUpdate, RunState, TriggerSlope } from '../api/types';
import { formatSeconds } from '../utils/format';

const TIMEBASE_STEPS = [1e-6, 2e-6, 5e-6, 1e-5, 2e-5, 5e-5, 1e-4, 2e-4, 5e-4, 1e-3, 2e-3, 5e-3, 1e-2];
const SLOPES: TriggerSlope[] = ['positive', 'negative', 'either'];

interface Props {
  acquisition: AcquisitionState | null;
  onRunState: (state: RunState) => void;
  onUpdate: (update: AcquisitionUpdate) => void;
}

export function TriggerControls({ acquisition, onRunState, onUpdate }: Props) {
  const running = acquisition?.runState === 'running';

  return (
    <section className="panel">
      <h2>Acquisition</h2>

      <div style={{ display: 'flex', gap: 6, marginBottom: 12 }}>
        <button className={running ? 'is-active' : ''} onClick={() => onRunState('running')}>
          ▶ Run
        </button>
        <button className={acquisition?.runState === 'stopped' ? 'is-active' : ''} onClick={() => onRunState('stopped')}>
          ■ Stop
        </button>
        <button onClick={() => onRunState('single')}>⤓ Single</button>
      </div>

      <Field label="Timebase">
        <select
          value={nearest(TIMEBASE_STEPS, acquisition?.secondsPerDivision ?? 1e-3)}
          onChange={(e) => onUpdate({ secondsPerDivision: Number(e.target.value) })}
        >
          {TIMEBASE_STEPS.map((s) => (
            <option key={s} value={s}>
              {formatSeconds(s, 0)}/div
            </option>
          ))}
        </select>
      </Field>

      <Field label="Trigger src">
        <select
          value={acquisition?.triggerSource ?? 1}
          onChange={(e) => onUpdate({ triggerSource: Number(e.target.value) })}
        >
          {[1, 2, 3, 4].map((c) => (
            <option key={c} value={c}>
              CH{c}
            </option>
          ))}
        </select>
      </Field>

      <Field label="Slope">
        <select
          value={acquisition?.triggerSlope ?? 'positive'}
          onChange={(e) => onUpdate({ triggerSlope: e.target.value as TriggerSlope })}
        >
          {SLOPES.map((s) => (
            <option key={s} value={s}>
              {s}
            </option>
          ))}
        </select>
      </Field>

      <Field label="Level (V)">
        <input
          type="number"
          step="0.05"
          value={acquisition?.triggerLevel ?? 0}
          onChange={(e) => onUpdate({ triggerLevel: Number(e.target.value) })}
          style={{ width: 90 }}
        />
      </Field>
    </section>
  );
}

function Field({ label, children }: { label: string; children: React.ReactNode }) {
  return (
    <label style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', padding: '4px 0', fontSize: 13 }}>
      <span style={{ color: 'var(--text-dim)' }}>{label}</span>
      {children}
    </label>
  );
}

function nearest(steps: number[], value: number): number {
  return steps.reduce((best, s) => (Math.abs(s - value) < Math.abs(best - value) ? s : best), steps[0]);
}
