import type {
  AcquisitionState,
  AcquisitionUpdate,
  ChannelConfig,
  ChannelUpdate,
  DeviceInfo,
  MeasurementSet,
  RunState,
  ScopeStatus,
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
};
