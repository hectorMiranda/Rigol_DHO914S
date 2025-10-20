import { useEffect, useState } from 'react';
import { api } from '../api/client';
import type { Spectrum } from '../api/types';

/** Polls the FFT spectrum for one channel while enabled. */
export function useSpectrum(channel: number | null, window: string, enabled: boolean, intervalMs = 250) {
  const [spectrum, setSpectrum] = useState<Spectrum | null>(null);

  useEffect(() => {
    if (!enabled || channel === null) {
      return;
    }
    let active = true;
    const tick = async () => {
      try {
        const s = await api.getFft(channel, 2048, window);
        if (active) setSpectrum(s);
      } catch {
        /* keep last good */
      }
    };
    void tick();
    const id = setInterval(tick, intervalMs);
    return () => {
      active = false;
      clearInterval(id);
    };
  }, [channel, window, enabled, intervalMs]);

  return spectrum;
}
