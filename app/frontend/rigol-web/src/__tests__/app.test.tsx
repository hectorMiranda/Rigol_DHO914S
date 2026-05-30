import { afterEach, describe, expect, it, vi } from 'vitest';
import { render, screen, cleanup } from '@testing-library/react';
import App from '../App';

// Minimal EventSource stub (jsdom has none); the stream just stays "open".
class FakeEventSource {
  onmessage: ((e: MessageEvent) => void) | null = null;
  onopen: (() => void) | null = null;
  onerror: (() => void) | null = null;
  constructor(public url: string) {}
  close() {}
}

const status = {
  device: {
    manufacturer: 'RIGOL TECHNOLOGIES',
    model: 'DHO914S',
    serialNumber: 'SIM0000000001',
    firmwareVersion: '00.01.03 (sim)',
    simulated: true,
  },
  acquisition: {
    runState: 'running',
    secondsPerDivision: 0.001,
    timebaseOffset: 0,
    triggerSource: 1,
    triggerLevel: 0,
    triggerSlope: 'positive',
    triggerStatus: 'AUTO',
  },
  channels: [1, 2, 3, 4].map((c) => ({
    channel: c,
    enabled: c <= 2,
    voltsPerDivision: 0.5,
    offsetVolts: 0,
    coupling: 'dc',
    probeRatio: 10,
    label: `CH${c}`,
  })),
};

const measurements = {
  channel: 1,
  timestamp: 0,
  items: [{ name: 'Peak-to-peak', code: 'VPP', value: 1.6, unit: 'V' }],
};

function mockFetch() {
  vi.stubGlobal(
    'fetch',
    vi.fn((url: string) => {
      const body = url.includes('/api/status')
        ? status
        : url.includes('/api/measurements')
          ? measurements
          : url.includes('/api/setups')
            ? []
            : {};
      return Promise.resolve({ ok: true, json: async () => body } as Response);
    }),
  );
}

afterEach(() => {
  vi.unstubAllGlobals();
  cleanup();
});

describe('App', () => {
  it('renders the shell and loads device identity', async () => {
    vi.stubGlobal('EventSource', FakeEventSource as unknown as typeof EventSource);
    mockFetch();

    render(<App />);

    // Static shell renders immediately.
    expect(screen.getByText('Rigol DHO914S')).toBeTruthy();
    expect(screen.getByText('Channels')).toBeTruthy();
    expect(screen.getByText('Measurements')).toBeTruthy();

    // Device identity + simulated badge appear once /api/status resolves.
    expect(await screen.findByText('SIMULATED')).toBeTruthy();
    expect((await screen.findAllByText('DHO914S')).length).toBeGreaterThan(0);
  });
});
