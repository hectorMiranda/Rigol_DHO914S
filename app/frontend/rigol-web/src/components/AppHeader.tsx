import type { DeviceInfo, RunState } from '../api/types';

interface Props {
  device: DeviceInfo | null;
  runState: RunState;
  connected: boolean;
  theme: 'dark' | 'light';
  onToggleTheme: () => void;
}

export function AppHeader({ device, runState, connected, theme, onToggleTheme }: Props) {
  return (
    <header className="header">
      <span className="header__title">Rigol DHO914S</span>
      <span className="header__model">
        {device ? `${device.manufacturer} ${device.model}` : 'connecting…'}
      </span>

      <span className="header__spacer" />

      <span className={`dot ${connected ? 'dot--ok' : 'dot--err'}`} title={connected ? 'stream connected' : 'disconnected'} />
      <span className="badge">{runState}</span>
      {device && (
        <span className={`badge ${device.simulated ? 'badge--sim' : 'badge--live'}`}>
          {device.simulated ? 'SIMULATED' : 'LIVE'}
        </span>
      )}
      <button onClick={onToggleTheme} title="Toggle theme" style={{ padding: '2px 8px' }}>
        {theme === 'dark' ? '☀' : '☾'}
      </button>
    </header>
  );
}
