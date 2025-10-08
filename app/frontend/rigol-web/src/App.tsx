import { useMemo, useState } from 'react';
import { api } from './api/client';
import type { AcquisitionUpdate, ChannelUpdate, RunState } from './api/types';
import { AppHeader } from './components/AppHeader';
import { StatusBar } from './components/StatusBar';
import { DeviceInfoPanel } from './components/DeviceInfoPanel';
import { ScopeDisplay } from './components/ScopeDisplay';
import { ScopeLegend } from './components/ScopeLegend';
import { ChannelControls } from './components/ChannelControls';
import { TriggerControls } from './components/TriggerControls';
import { MeasurementsPanel } from './components/MeasurementsPanel';
import { StreamSettings, type StreamConfig } from './components/StreamSettings';
import { ScreenshotViewer } from './components/ScreenshotViewer';
import { useScopeStatus } from './hooks/useScopeStatus';
import { useWaveformStream } from './hooks/useWaveformStream';

export default function App() {
  const { status, error, setStatus } = useScopeStatus();
  const [stream, setStream] = useState<StreamConfig>({ intervalMs: 100, points: 600 });

  const channels = status?.channels ?? [];
  const enabledChannels = useMemo(
    () => channels.filter((c) => c.enabled).map((c) => c.channel),
    [channels],
  );

  const running = status?.acquisition.runState !== 'stopped';
  const { frames, connected, frameRate } = useWaveformStream({
    channels: enabledChannels,
    intervalMs: stream.intervalMs,
    points: stream.points,
    enabled: running && enabledChannels.length > 0,
  });

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

  return (
    <div className="app">
      <AppHeader
        device={status?.device ?? null}
        runState={status?.acquisition.runState ?? 'stopped'}
        connected={connected}
      />

      <div className="app__main">
        <div className="app__scope">
          <ScopeLegend channels={channels} />
          <ScopeDisplay frames={frames} channels={channels} acquisition={status?.acquisition ?? null} />
        </div>

        <aside className="app__side">
          <TriggerControls acquisition={status?.acquisition ?? null} onRunState={setRunState} onUpdate={patchAcquisition} />
          <ChannelControls channels={channels} onUpdate={patchChannel} />
          <MeasurementsPanel enabledChannels={enabledChannels} />
          <StreamSettings config={stream} connected={connected} onChange={setStream} />
          <DeviceInfoPanel device={status?.device ?? null} />
          <ScreenshotViewer />
        </aside>
      </div>

      <StatusBar acquisition={status?.acquisition ?? null} frameRate={frameRate} error={error} />
    </div>
  );
}
