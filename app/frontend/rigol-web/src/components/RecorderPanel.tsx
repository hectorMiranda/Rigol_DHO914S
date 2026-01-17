import type { useRecorder } from '../hooks/useRecorder';

type Recorder = ReturnType<typeof useRecorder>;

/** Capture the live stream to a ring buffer and scrub back through it. */
export function RecorderPanel({ recorder }: { recorder: Recorder }) {
  const { recording, mode, count, playhead, start, stop, clear, setMode, setPlayhead } = recorder;

  return (
    <section className="panel">
      <h2>Recorder</h2>

      <div style={{ display: 'flex', gap: 6, marginBottom: 8 }}>
        {recording ? (
          <button className="is-active" style={{ color: 'var(--err)', borderColor: 'var(--err)' }} onClick={stop}>
            ■ Stop ({count})
          </button>
        ) : (
          <button onClick={start}>● Record</button>
        )}
        <button onClick={clear} disabled={count === 0 && !recording}>Clear</button>
      </div>

      {!recording && count > 0 && (
        <>
          <div style={{ display: 'flex', gap: 6, marginBottom: 6 }}>
            <button className={mode === 'live' ? 'is-active' : ''} onClick={() => setMode('live')}>Live</button>
            <button className={mode === 'playback' ? 'is-active' : ''} onClick={() => setMode('playback')}>Playback</button>
          </div>
          <input
            type="range"
            min={0}
            max={count - 1}
            value={playhead}
            disabled={mode !== 'playback'}
            onChange={(e) => setPlayhead(Number(e.target.value))}
            style={{ width: '100%' }}
          />
          <div style={{ fontSize: 12, color: 'var(--text-dim)' }}>
            frame {playhead + 1} / {count}
          </div>
        </>
      )}
    </section>
  );
}
