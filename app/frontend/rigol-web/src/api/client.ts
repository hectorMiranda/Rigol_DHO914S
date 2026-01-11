import type {
  AcquisitionState,
  AcquisitionUpdate,
  ChannelConfig,
  ChannelUpdate,
  DeviceInfo,
  MathOp,
  MeasurementSet,
  RunState,
  ScopeStatus,
  SetupSummary,
  Spectrum,
  UartDecodeResult,
  Waveform,
} from './types';

/** Base URL for the API. Empty string = same origin (dev proxy / co-hosted). */
export const API_BASE = (import.meta.env.VITE_API_BASE as string | undefined) ?? '';

export class ApiError extends Error {
  constructor(
    public readonly status: number,
    public readonly detail: string,
  ) {
    super(`API ${status}: ${detail}`);
    this.name = 'ApiError';
  }
}

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const res = await fetch(`${API_BASE}/api/${path}`, {
    headers: { 'Content-Type': 'application/json' },
    ...init,
  });
  if (!res.ok) {
    let detail = res.statusText;
    try {
      const body = await res.json();
      detail = body?.detail ?? detail;
    } catch {
      /* non-JSON error body */
    }
    throw new ApiError(res.status, detail);
  }
  return (await res.json()) as T;
}

async function requestVoid(path: string, init?: RequestInit): Promise<void> {
  const res = await fetch(`${API_BASE}/api/${path}`, init);
  if (!res.ok) throw new ApiError(res.status, res.statusText);
}

export const api = {
  getDevice: () => request<DeviceInfo>('device'),
  getStatus: () => request<ScopeStatus>('status'),
  getChannels: () => request<ChannelConfig[]>('channels'),

  updateChannel: (channel: number, update: ChannelUpdate) =>
    request<ChannelConfig>(`channels/${channel}`, {
      method: 'PATCH',
      body: JSON.stringify(update),
    }),

  getWaveform: (channel: number, points?: number) =>
    request<Waveform>(`waveform/${channel}${points ? `?points=${points}` : ''}`),

  getMeasurements: (channel: number) => request<MeasurementSet>(`measurements/${channel}`),

  getFft: (channel: number, points = 2048, window = 'hann') =>
    request<Spectrum>(`fft/${channel}?points=${points}&window=${window}`),

  getMath: (op: MathOp, a: number, b: number, points = 600) =>
    request<Waveform>(`math/${op}?a=${a}&b=${b}&points=${points}`),

  decodeUart: (channel: number, baud: number, points = 4096) =>
    request<UartDecodeResult>(`decode/uart/${channel}?baud=${baud}&points=${points}`),

  listSetups: () => request<SetupSummary[]>('setups'),
  saveSetup: (name: string) =>
    request<unknown>(`setups/${encodeURIComponent(name)}`, { method: 'POST' }),
  recallSetup: (name: string) =>
    request<AcquisitionState>(`setups/${encodeURIComponent(name)}/recall`, { method: 'POST' }),
  deleteSetup: (name: string) =>
    requestVoid(`setups/${encodeURIComponent(name)}`, { method: 'DELETE' }),

  getAcquisition: () => request<AcquisitionState>('acquisition'),

  setRunState: (state: RunState) => {
    const action = state === 'running' ? 'run' : state === 'stopped' ? 'stop' : 'single';
    return request<AcquisitionState>(`acquisition/${action}`, { method: 'POST' });
  },

  updateAcquisition: (update: AcquisitionUpdate) =>
    request<AcquisitionState>('acquisition', {
      method: 'PATCH',
      body: JSON.stringify(update),
    }),

  /** URL for the live SSE stream (consumed via EventSource). */
  streamUrl: (channels: number[], intervalMs: number, points: number) =>
    `${API_BASE}/api/stream?channels=${channels.join(',')}&interval=${intervalMs}&points=${points}`,

  /** URL for the display screenshot PNG (cache-busted). */
  screenshotUrl: () => `${API_BASE}/api/screenshot?t=${Date.now()}`,

  /** URL for a CSV export of one channel's current trace. */
  exportCsvUrl: (channel: number, points?: number) =>
    `${API_BASE}/api/export/${channel}.csv${points ? `?points=${points}` : ''}`,
};
