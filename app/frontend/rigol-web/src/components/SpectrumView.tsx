import { useEffect, useRef } from 'react';
import type { Spectrum } from '../api/types';
import { CHANNEL_COLORS } from './ScopeDisplay';
import { formatHertz } from '../utils/format';

interface Props {
  spectrum: Spectrum | null;
  channel: number;
}

const DB_TOP = 0;
const DB_BOTTOM = -120;

/** Plots a one-sided magnitude spectrum (dBV vs frequency) on a canvas. */
export function SpectrumView({ spectrum, channel }: Props) {
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const wrapRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const canvas = canvasRef.current;
    const wrap = wrapRef.current;
    if (!canvas || !wrap) return;

    const dpr = window.devicePixelRatio || 1;
    const w = wrap.clientWidth;
    const h = wrap.clientHeight;
    canvas.width = w * dpr;
    canvas.height = h * dpr;
    canvas.style.width = `${w}px`;
    canvas.style.height = `${h}px`;

    const ctx = canvas.getContext('2d');
    if (!ctx) return;
    ctx.setTransform(dpr, 0, 0, dpr, 0, 0);

    ctx.fillStyle = '#06080f';
    ctx.fillRect(0, 0, w, h);

    // grid
    ctx.strokeStyle = '#171d2b';
    ctx.lineWidth = 1;
    for (let i = 1; i < 10; i++) {
      const x = (i * w) / 10;
      ctx.beginPath();
      ctx.moveTo(x, 0);
      ctx.lineTo(x, h);
      ctx.stroke();
    }
    for (let j = 1; j < 6; j++) {
      const y = (j * h) / 6;
      ctx.beginPath();
      ctx.moveTo(0, y);
      ctx.lineTo(w, y);
      ctx.stroke();
    }

    if (!spectrum || spectrum.magnitudesDb.length === 0) return;

    const bins = spectrum.magnitudesDb;
    const nyquist = spectrum.frequencyStep * bins.length;
    const toX = (k: number) => (k / (bins.length - 1)) * w;
    const toY = (db: number) =>
      ((DB_TOP - Math.max(DB_BOTTOM, Math.min(DB_TOP, db))) / (DB_TOP - DB_BOTTOM)) * h;

    ctx.beginPath();
    ctx.moveTo(toX(0), toY(bins[0]));
    for (let k = 1; k < bins.length; k++) ctx.lineTo(toX(k), toY(bins[k]));
    ctx.strokeStyle = CHANNEL_COLORS[channel] ?? '#38bdf8';
    ctx.lineWidth = 1.4;
    ctx.stroke();

    // peak marker
    let peak = 1;
    for (let k = 1; k < bins.length; k++) if (bins[k] > bins[peak]) peak = k;
    const px = toX(peak);
    const py = toY(bins[peak]);
    ctx.fillStyle = '#fb923c';
    ctx.beginPath();
    ctx.arc(px, py, 3, 0, Math.PI * 2);
    ctx.fill();
    ctx.font = '12px ui-sans-serif, system-ui';
    ctx.fillStyle = '#d6def0';
    ctx.fillText(`${formatHertz(peak * spectrum.frequencyStep)}  ${bins[peak].toFixed(1)} dB`, Math.min(px + 8, w - 130), Math.max(py, 14));

    // axis labels
    ctx.fillStyle = '#8a96b0';
    ctx.fillText('0 Hz', 4, h - 6);
    ctx.fillText(formatHertz(nyquist), w - 64, h - 6);
  }, [spectrum, channel]);

  return (
    <div ref={wrapRef} className="scope" style={{ position: 'relative', flex: 1, minHeight: 280 }}>
      <canvas ref={canvasRef} style={{ display: 'block', borderRadius: 8 }} />
    </div>
  );
}
