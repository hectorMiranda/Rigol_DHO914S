import type { DeviceInfo } from '../api/types';

interface Props {
  device: DeviceInfo | null;
}

export function DeviceInfoPanel({ device }: Props) {
  return (
    <section className="panel">
      <h2>Instrument</h2>
      {device ? (
        <dl className="kv">
          <Row label="Model" value={device.model} />
          <Row label="Vendor" value={device.manufacturer} />
          <Row label="Serial" value={device.serialNumber} />
          <Row label="Firmware" value={device.firmwareVersion} />
          <Row label="Source" value={device.simulated ? 'Simulator' : 'Hardware'} />
        </dl>
      ) : (
        <p style={{ color: 'var(--text-dim)' }}>Connecting…</p>
      )}
    </section>
  );
}

function Row({ label, value }: { label: string; value: string }) {
  return (
    <div style={{ display: 'flex', justifyContent: 'space-between', padding: '3px 0', fontSize: 13 }}>
      <span style={{ color: 'var(--text-dim)' }}>{label}</span>
      <span style={{ fontVariantNumeric: 'tabular-nums' }}>{value || '—'}</span>
    </div>
  );
}
