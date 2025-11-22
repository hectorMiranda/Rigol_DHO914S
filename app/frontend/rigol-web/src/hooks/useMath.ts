import { useEffect, useState } from 'react';
import { api } from '../api/client';
import type { MathOp, Waveform } from '../api/types';

export interface MathConfig {
  enabled: boolean;
  op: MathOp;
  a: number;
  b: number;
}

/** Polls the math-channel result while enabled; returns a MATH waveform (channel 0). */
export function useMath(config: MathConfig, points: number, intervalMs = 150) {
  const [waveform, setWaveform] = useState<Waveform | null>(null);

  useEffect(() => {
    if (!config.enabled) {
      setWaveform(null);
      return;
    }
    let active = true;
    const tick = async () => {
      try {
        const w = await api.getMath(config.op, config.a, config.b, points);
        if (active) setWaveform(w);
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
  }, [config.enabled, config.op, config.a, config.b, points, intervalMs]);

  return waveform;
}
