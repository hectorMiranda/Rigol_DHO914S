import { useEffect, useMemo, useState } from 'react';
import { api } from './api/client';
import type { AcquisitionUpdate, ChannelUpdate, RunState } from './api/types';
import { AppHeader } from './components/AppHeader';
import { StatusBar } from './components/StatusBar';
import { DeviceInfoPanel } from './components/DeviceInfoPanel';
import { ScopeDisplay, type Cursors } from './components/ScopeDisplay';
import { SpectrumView } from './components/SpectrumView';
import { XYView } from './components/XYView';
import { ViewTabs } from './components/ViewTabs';
import { ScopeLegend } from './components/ScopeLegend';
import { CursorControls } from './components/CursorControls';
import { ChannelControls } from './components/ChannelControls';
import { TriggerControls } from './components/TriggerControls';
import { MeasurementsPanel } from './components/MeasurementsPanel';
import { StreamSettings, type StreamConfig } from './components/StreamSettings';
import { MathPanel } from './components/MathPanel';
import { RecorderPanel } from './components/RecorderPanel';
import { SetupsPanel } from './components/SetupsPanel';
import { DecodePanel } from './components/DecodePanel';
import { ScreenshotViewer } from './components/ScreenshotViewer';
import { useMath, type MathConfig } from './hooks/useMath';
import { useRecorder } from './hooks/useRecorder';
import { useScopeStatus } from './hooks/useScopeStatus';
import { useWaveformStream } from './hooks/useWaveformStream';
import { useSpectrum } from './hooks/useSpectrum';
import { useLocalStorage } from './hooks/useLocalStorage';
import { useHotkeys } from './hooks/useHotkeys';

type ViewMode = 'scope' | 'spectrum' | 'xy';

export default function App() {
  const { status, error, setStatus, refresh } = useScopeStatus();
  const [stream, setStream] = useLocalStorage<StreamConfig>('rigol.stream', { intervalMs: 100, points: 600 });
  const [theme, setTheme] = useLocalStorage<'dark' | 'light'>('rigol.theme', 'dark');
  const [view, setView] = useState<ViewMode>('scope');
  const [xy, setXy] = useState({ x: 1, y: 2 });
  const [persist, setPersist] = useState(false);

  useEffect(() => {
    document.documentElement.setAttribute('data-theme', theme);
  }, [theme]);

  const autoset = async () => {
    try {
      setStatus(await api.autoset());
    } catch {
      /* ignore */
    }
  };

  const channels = status?.channels ?? [];
  const enabledChannels = useMemo(
    () => channels.filter((c) => c.enabled).map((c) => c.channel),
    [channels],
  );

  const running = status?.acquisition.runState !== 'stopped';
  const analyzeChannel = enabledChannels[0] ?? 1;
  const spectrum = useSpectrum(analyzeChannel, 'hann', view === 'spectrum' && running);

  const { frames, connected, frameRate } = useWaveformStream({
    channels: enabledChannels,
    intervalMs: stream.intervalMs,
    points: stream.points,
    enabled: running && enabledChannels.length > 0,
  });

  const [cursors, setCursors] = useState<Cursors>({ enabled: false, a: 0.35, b: 0.65 });
  const recorder = useRecorder(frames);
  const baseFrames = recorder.mode === 'playback' ? recorder.playbackFrames : frames;

  const [math, setMath] = useState<MathConfig>({ enabled: false, op: 'subtract', a: 1, b: 2 });
  const mathFrame = useMath(math, stream.points);
  const displayFrames = useMemo(
    () => (mathFrame ? [...baseFrames, mathFrame] : baseFrames),
    [baseFrames, mathFrame],
  );

  const patchChannel = async (channel: number, update: ChannelUpdate) => {
    setStatus((prev) =>
      prev ? { ...prev, channels: prev.channels.map((c) => (c.channel === channel ? { ...c, ...update } : c)) } : prev,
    );
    try {
      const updated = await api.updateChannel(channel, update);
      setStatus((prev) =>
        prev ? { ...prev, channels: prev.channels.map((c) => (c.channel === channel ? updated : c)) } : prev,
      );
    } catch {
      /* poll will reconcile */
    }
  };

  const patchAcquisition = async (update: AcquisitionUpdate) => {
    try {
      const acquisition = await api.updateAcquisition(update);
      setStatus((prev) => (prev ? { ...prev, acquisition } : prev));
    } catch {
      /* ignore */
    }
  };

  const setRunState = async (state: RunState) => {
    try {
      const acquisition = await api.setRunState(state);
      setStatus((prev) => (prev ? { ...prev, acquisition } : prev));
    } catch {
      /* ignore */
    }
  };

  // Keyboard shortcuts: space=run/stop, s=single, a=autoset, p=persist, f=cycle view.
  useHotkeys(
    useMemo(
      () => ({
        ' ': () => void setRunState(running ? 'stopped' : 'running'),
        s: () => void setRunState('single'),
        a: () => void autoset(),
        p: () => setPersist((v) => !v),
        f: () => setView((v) => (v === 'scope' ? 'spectrum' : v === 'spectrum' ? 'xy' : 'scope')),
      }),
      // eslint-disable-next-line react-hooks/exhaustive-deps
      [running],
    ),
  );

  return (
    <div className="app">
      <AppHeader
        device={status?.device ?? null}
        runState={status?.acquisition.runState ?? 'stopped'}
        connected={connected}
        theme={theme}
        onToggleTheme={() => setTheme((t) => (t === 'dark' ? 'light' : 'dark'))}
      />

      <div className="app__main">
        <div className="app__scope">
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
              <ViewTabs
                value={view}
                options={[
                  { key: 'scope', label: 'Scope' },
                  { key: 'spectrum', label: 'FFT' },
                  { key: 'xy', label: 'XY' },
                ]}
                onChange={setView}
              />
              <button onClick={autoset} title="Auto-set scales and timebase">Auto</button>
              {view === 'scope' && (
                <button className={persist ? 'is-active' : ''} onClick={() => setPersist((p) => !p)} title="Afterglow persistence">
                  Persist
                </button>
              )}
              {view === 'xy' && (
                <span style={{ display: 'flex', gap: 4, alignItems: 'center', fontSize: 12 }}>
                  <select value={xy.x} onChange={(e) => setXy({ ...xy, x: Number(e.target.value) })}>
                    {[1, 2, 3, 4].map((c) => <option key={c} value={c}>X:CH{c}</option>)}
                  </select>
                  <select value={xy.y} onChange={(e) => setXy({ ...xy, y: Number(e.target.value) })}>
                    {[1, 2, 3, 4].map((c) => <option key={c} value={c}>Y:CH{c}</option>)}
                  </select>
                </span>
              )}
            </div>
            <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
              <ScopeLegend channels={channels} />
              <a
                href={api.exportCsvUrl(analyzeChannel, stream.points)}
                style={{ fontSize: 12, color: 'var(--accent)' }}
                title={`Download CH${analyzeChannel} as CSV`}
              >
                ⤓ CSV
              </a>
            </div>
          </div>
          {view === 'scope' && (
            <ScopeDisplay frames={displayFrames} channels={channels} acquisition={status?.acquisition ?? null} cursors={cursors} persistence={persist} />
          )}
          {view === 'spectrum' && <SpectrumView spectrum={spectrum} channel={analyzeChannel} />}
          {view === 'xy' && <XYView frames={frames} channels={channels} xChannel={xy.x} yChannel={xy.y} />}
        </div>

        <aside className="app__side">
          <TriggerControls acquisition={status?.acquisition ?? null} onRunState={setRunState} onUpdate={patchAcquisition} />
          <ChannelControls channels={channels} onUpdate={patchChannel} />
          <MeasurementsPanel enabledChannels={enabledChannels} />
          <CursorControls cursors={cursors} onChange={setCursors} />
          <RecorderPanel recorder={recorder} />
          <MathPanel config={math} onChange={setMath} />
          <DecodePanel channel={analyzeChannel} />
          <SetupsPanel onRecalled={refresh} />
          <StreamSettings config={stream} connected={connected} onChange={setStream} />
          <DeviceInfoPanel device={status?.device ?? null} />
          <ScreenshotViewer />
        </aside>
      </div>

      <StatusBar acquisition={status?.acquisition ?? null} frameRate={frameRate} error={error} />
    </div>
  );
}
