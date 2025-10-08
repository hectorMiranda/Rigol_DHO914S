import type { AcquisitionState } from '../api/types';
import { formatSeconds } from '../utils/format';

interface Props {
  acquisition: AcquisitionState | null;
  frameRate: number;
  error: string | null;
}

export function StatusBar({ acquisition, frameRate, error }: Props) {
  return (
    <footer className="statusbar">
      <span>
        Trigger: <b>{acquisition?.triggerStatus ?? '—'}</b>
      </span>
      <span>
        Timebase: <b>{acquisition ? `${formatSeconds(acquisition.secondsPerDivision)}/div` : '—'}</b>
      </span>
      <span>
        Source: <b>CH{acquisition?.triggerSource ?? '—'}</b>
      </span>
      <span>
        Frames: <b>{frameRate.toFixed(0)}/s</b>
      </span>
      {error && (
        <span style={{ color: 'var(--err)' }}>
          <span className="dot dot--err" /> {error}
        </span>
      )}
    </footer>
  );
}
