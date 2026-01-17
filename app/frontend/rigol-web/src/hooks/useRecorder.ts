import { useEffect, useRef, useState } from 'react';
import type { Waveform } from '../api/types';

export type RecorderMode = 'live' | 'playback';

const MAX_FRAMES = 600;

/**
 * Records snapshots of the live frame set into a ring buffer (capped) and lets
 * the user scrub back through them. While in playback mode the caller should
 * render the returned playbackFrames instead of the live ones.
 */
export function useRecorder(liveFrames: Waveform[]) {
  const [recording, setRecording] = useState(false);
  const [mode, setMode] = useState<RecorderMode>('live');
  const [count, setCount] = useState(0);
  const [playhead, setPlayhead] = useState(0);
  const bufferRef = useRef<Waveform[][]>([]);

  useEffect(() => {
    if (!recording || liveFrames.length === 0) return;
    const buf = bufferRef.current;
    buf.push(liveFrames);
    if (buf.length > MAX_FRAMES) buf.shift();
    setCount(buf.length);
  }, [recording, liveFrames]);

  const start = () => {
    bufferRef.current = [];
    setCount(0);
    setPlayhead(0);
    setMode('live');
    setRecording(true);
  };

  const stop = () => {
    setRecording(false);
    if (bufferRef.current.length > 0) {
      setMode('playback');
      setPlayhead(bufferRef.current.length - 1);
    }
  };

  const clear = () => {
    bufferRef.current = [];
    setCount(0);
    setPlayhead(0);
    setMode('live');
    setRecording(false);
  };

  const playbackFrames = bufferRef.current[playhead] ?? [];

  return { recording, mode, count, playhead, playbackFrames, start, stop, clear, setMode, setPlayhead } as const;
}
