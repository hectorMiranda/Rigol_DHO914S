import type { ChannelConfig, ChannelUpdate, Coupling } from '../api/types';
import { CHANNEL_COLORS } from './ScopeDisplay';
import { formatVolts } from '../utils/format';

const VDIV_STEPS = [0.01, 0.02, 0.05, 0.1, 0.2, 0.5, 1, 2, 5];
const COUPLINGS: Coupling[] = ['dc', 'ac', 'gnd'];

interface Props {
  channels: ChannelConfig[];
  onUpdate: (channel: number, update: ChannelUpdate) => void;
}

export function ChannelControls({ channels, onUpdate }: Props) {
  return (
    <section className="panel">
      <h2>Channels</h2>
      <div style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
        {channels.map((ch) => (
          <ChannelRow key={ch.channel} ch={ch} onUpdate={onUpdate} />
        ))}
      </div>
    </section>
  );
}

function ChannelRow({ ch, onUpdate }: { ch: ChannelConfig; onUpdate: Props['onUpdate'] }) {
  const color = CHANNEL_COLORS[ch.channel];
  return (
    <div style={{ display: 'grid', gridTemplateColumns: 'auto 1fr', gap: 8, alignItems: 'center' }}>
      <button
        className={ch.enabled ? 'is-active' : ''}
        style={{ borderColor: ch.enabled ? color : undefined, color: ch.enabled ? color : undefined, minWidth: 48 }}
        onClick={() => onUpdate(ch.channel, { enabled: !ch.enabled })}
        title={ch.enabled ? 'Disable channel' : 'Enable channel'}
      >
        CH{ch.channel}
      </button>

      <div style={{ display: 'flex', gap: 6, opacity: ch.enabled ? 1 : 0.5 }}>
        <select
          value={ch.voltsPerDivision}
          onChange={(e) => onUpdate(ch.channel, { voltsPerDivision: Number(e.target.value) })}
          title="Volts / division"
        >
          {VDIV_STEPS.map((v) => (
            <option key={v} value={v}>
              {formatVolts(v, 0)}/div
            </option>
          ))}
        </select>

        <select
          value={ch.coupling}
          onChange={(e) => onUpdate(ch.channel, { coupling: e.target.value as Coupling })}
          title="Coupling"
        >
          {COUPLINGS.map((c) => (
            <option key={c} value={c}>
              {c.toUpperCase()}
            </option>
          ))}
        </select>
      </div>
    </div>
  );
}
