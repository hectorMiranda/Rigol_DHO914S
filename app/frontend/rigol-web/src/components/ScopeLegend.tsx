import type { ChannelConfig } from '../api/types';
import { CHANNEL_COLORS } from './ScopeDisplay';
import { formatVolts } from '../utils/format';

interface Props {
  channels: ChannelConfig[];
}

/** Compact legend strip shown above the scope: colour, label and V/div per active channel. */
export function ScopeLegend({ channels }: Props) {
  const active = channels.filter((c) => c.enabled);
  if (active.length === 0) return null;

  return (
    <div style={{ display: 'flex', gap: 14, flexWrap: 'wrap', padding: '2px 4px' }}>
      {active.map((ch) => (
        <span key={ch.channel} style={{ display: 'inline-flex', alignItems: 'center', gap: 6, fontSize: 12 }}>
          <span style={{ width: 14, height: 3, borderRadius: 2, background: CHANNEL_COLORS[ch.channel] }} />
          <b style={{ color: CHANNEL_COLORS[ch.channel] }}>{ch.label ?? `CH${ch.channel}`}</b>
          <span style={{ color: 'var(--text-dim)' }}>{formatVolts(ch.voltsPerDivision, 0)}/div</span>
          <span style={{ color: 'var(--text-dim)' }}>{ch.coupling.toUpperCase()}</span>
        </span>
      ))}
    </div>
  );
}
