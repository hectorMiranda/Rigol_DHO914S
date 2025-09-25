import { useEffect, useRef, useState } from 'react';
import { api } from '../api/client';
import type { StreamFrame, Waveform } from '../api/types';

interface StreamOptions {
  channels: number[];
  intervalMs: number;
  points: number;
  enabled: boolean;
}

/**
 * Subscribes to the backend SSE stream with an EventSource and exposes the most
 * recent waveform per channel plus a measured frame rate and connection state.
 * Re-subscribes whenever the channel set / cadence changes.
 */
export function useWaveformStream({ channels, intervalMs, points, enabled }: StreamOptions) {
  const [frames, setFrames] = useState<Waveform[]>([]);
  const [connected, setConnected] = useState(false);
  const [frameRate, setFrameRate] = useState(0);
  const recentRef = useRef<number[]>([]);

  const key = channels.join(',');

  useEffect(() => {
    if (!enabled || channels.length === 0) {
      setConnected(false);
      return;
    }

    const source = new EventSource(api.streamUrl(channels, intervalMs, points));

    source.onopen = () => setConnected(true);
    source.onerror = () => setConnected(false); // EventSource auto-retries

    source.onmessage = (ev) => {
      try {
        const frame = JSON.parse(ev.data) as StreamFrame;
        setFrames(frame.frames);

        const now = performance.now();
        const recent = recentRef.current;
        recent.push(now);
        while (recent.length > 0 && now - recent[0] > 1000) recent.shift();
        setFrameRate(recent.length);
      } catch {
        /* ignore malformed frame */
      }
    };

    return () => source.close();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [key, intervalMs, points, enabled]);

  return { frames, connected, frameRate } as const;
}
