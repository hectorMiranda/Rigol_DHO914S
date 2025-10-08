import { useState } from 'react';
import { api } from '../api/client';

/** Grabs the instrument's display PNG on demand and shows it inline. */
export function ScreenshotViewer() {
  const [src, setSrc] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  const capture = () => {
    setLoading(true);
    setSrc(api.screenshotUrl());
  };

  return (
    <section className="panel">
      <h2>Screenshot</h2>
      <button onClick={capture} disabled={loading}>
        {loading && !src ? 'Capturing…' : '📷 Capture display'}
      </button>

      {src && (
        <div style={{ marginTop: 10 }}>
          <img
            src={src}
            alt="Oscilloscope display"
            onLoad={() => setLoading(false)}
            onError={() => setLoading(false)}
            style={{ width: '100%', borderRadius: 6, border: '1px solid var(--border)' }}
          />
          <a href={src} download="dho914s-screen.png" style={{ display: 'inline-block', marginTop: 6, fontSize: 12, color: 'var(--accent)' }}>
            Download PNG
          </a>
        </div>
      )}
    </section>
  );
}
