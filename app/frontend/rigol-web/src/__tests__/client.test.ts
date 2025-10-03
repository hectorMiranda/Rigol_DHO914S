import { afterEach, describe, expect, it, vi } from 'vitest';
import { ApiError, api } from '../api/client';

afterEach(() => vi.unstubAllGlobals());

function stubFetch(response: Partial<Response> & { json?: () => Promise<unknown> }) {
  vi.stubGlobal('fetch', vi.fn().mockResolvedValue(response));
}

describe('api client', () => {
  it('returns parsed JSON on success', async () => {
    stubFetch({ ok: true, json: async () => ({ channel: 1, voltage: [], sampleCount: 0 }) });
    const wf = await api.getWaveform(1);
    expect(wf.channel).toBe(1);
  });

  it('throws ApiError with detail on non-2xx', async () => {
    stubFetch({ ok: false, status: 400, statusText: 'Bad Request', json: async () => ({ detail: 'Channel 9 out of range' }) });
    await expect(api.getWaveform(9)).rejects.toMatchObject({
      name: 'ApiError',
      status: 400,
      detail: 'Channel 9 out of range',
    });
  });

  it('builds stream and screenshot URLs', () => {
    expect(api.streamUrl([1, 2], 100, 600)).toBe('/api/stream?channels=1,2&interval=100&points=600');
    expect(api.screenshotUrl()).toMatch(/^\/api\/screenshot\?t=\d+$/);
  });

  it('exposes ApiError as an Error subclass', () => {
    const e = new ApiError(503, 'down');
    expect(e).toBeInstanceOf(Error);
    expect(e.message).toContain('503');
  });
});
