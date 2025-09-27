import { useEffect, useState } from 'react';
import { api } from '../api/client';
import type { MeasurementSet } from '../api/types';

/** Polls the auto-measurement set for one channel while enabled. */
export function useMeasurements(channel: number | null, intervalMs = 1000) {
  const [data, setData] = useState<MeasurementSet | null>(null);

  useEffect(() => {
    if (channel === null) {
      setData(null);
      return;
    }
    let active = true;
    const tick = async () => {
      try {
        const set = await api.getMeasurements(channel);
        if (active) setData(set);
      } catch {
        /* transient; keep last good values */
      }
    };
    void tick();
    const id = setInterval(tick, intervalMs);
    return () => {
      active = false;
      clearInterval(id);
    };
  }, [channel, intervalMs]);

  return data;
}
