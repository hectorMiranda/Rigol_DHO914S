import { useEffect, useRef } from 'react';
import type { AcquisitionState, ChannelConfig, Waveform } from '../api/types';
import { formatSeconds, formatHertz, formatVolts } from '../utils/format';

export interface Cursors {
  enabled: boolean;
  a: number; // 0..1 across the time axis
  b: number;
}

export const CHANNEL_COLORS: Record<number, string> = {
  0: '#e5e7eb', // MATH
  1: '#ffdc00',
  2: '#00c8ff',
  3: '#ff40c8',
  4: '#00dc78',
};

const X_DIV = 10;
const Y_DIV = 8;

interface Props {
  frames: Waveform[];
  channels: ChannelConfig[];
  acquisition: AcquisitionState | null;
  cursors?: Cursors;
  persistence?: boolean;
}

const PERSIST_DEPTH = 16;

/**
 * Renders the scope graticule and each channel's trace onto a canvas. Voltages
 * map to vertical divisions using each channel's volts/div + offset, exactly
 * like a real instrument, so the same trace looks right at any vertical scale.
 */
export function ScopeDisplay({ frames, channels, acquisition, cursors, persistence }: Props) {
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const wrapRef = useRef<HTMLDivElement>(null);
  const historyRef = useRef<Waveform[][]>([]);

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

    drawGraticule(ctx, w, h);

    const byChannel = new Map(channels.map((c) => [c.channel, c]));
    const drawSet = (set: Waveform[], alpha: number) => {
      ctx.globalAlpha = alpha;
      for (const frame of set) {
        const cfg = byChannel.get(frame.channel);
        if (cfg && !cfg.enabled) continue;
        drawTrace(ctx, frame, cfg, w, h);
      }
      ctx.globalAlpha = 1;
    };

    if (persistence) {
      const hist = historyRef.current;
      hist.push(frames);
      if (hist.length > PERSIST_DEPTH) hist.shift();
      // Oldest frames faintest — an afterglow trail.
      hist.forEach((set, i) => drawSet(set, 0.12 + 0.88 * ((i + 1) / hist.length)));
    } else {
      historyRef.current = [];
      drawSet(frames, 1);
    }

    drawTriggerMarker(ctx, acquisition, channels, w, h);
    if (cursors?.enabled) drawCursors(ctx, cursors, frames, acquisition, w, h);
  }, [frames, channels, acquisition, cursors, persistence]);

  return (
    <div ref={wrapRef} className="scope" style={{ position: 'relative', flex: 1, minHeight: 280 }}>
      <canvas ref={canvasRef} style={{ display: 'block', borderRadius: 8 }} />
    </div>
  );
}

function drawGraticule(ctx: CanvasRenderingContext2D, w: number, h: number) {
  ctx.fillStyle = '#06080f';
  ctx.fillRect(0, 0, w, h);

  ctx.lineWidth = 1;
  for (let i = 0; i <= X_DIV; i++) {
    const x = Math.round((i * w) / X_DIV) + 0.5;
    ctx.strokeStyle = i === X_DIV / 2 ? '#2c3550' : '#171d2b';
    line(ctx, x, 0, x, h);
  }
  for (let j = 0; j <= Y_DIV; j++) {
    const y = Math.round((j * h) / Y_DIV) + 0.5;
    ctx.strokeStyle = j === Y_DIV / 2 ? '#2c3550' : '#171d2b';
    line(ctx, 0, y, w, y);
  }
}

function drawTrace(
  ctx: CanvasRenderingContext2D,
  frame: Waveform,
  cfg: ChannelConfig | undefined,
  w: number,
  h: number,
) {
  const v = frame.voltage;
  if (v.length === 0) return;

  const voltsPerDiv = cfg?.voltsPerDivision ?? 1;
  const offset = cfg?.offsetVolts ?? 0;
  const pxPerDivY = h / Y_DIV;
  const centerY = h / 2;

  const toY = (volts: number) => centerY - ((volts + offset) / voltsPerDiv) * pxPerDivY;
  const toX = (i: number) => (i / (v.length - 1)) * w;

  ctx.beginPath();
  ctx.moveTo(toX(0), toY(v[0]));
  for (let i = 1; i < v.length; i++) ctx.lineTo(toX(i), toY(v[i]));

  ctx.lineWidth = 1.5;
  ctx.strokeStyle = CHANNEL_COLORS[frame.channel] ?? '#ffffff';
  ctx.shadowColor = ctx.strokeStyle;
  ctx.shadowBlur = 6;
  ctx.stroke();
  ctx.shadowBlur = 0;
}

function drawTriggerMarker(
  ctx: CanvasRenderingContext2D,
  acquisition: AcquisitionState | null,
  channels: ChannelConfig[],
  w: number,
  h: number,
) {
  if (!acquisition) return;
  const cfg = channels.find((c) => c.channel === acquisition.triggerSource);
  const voltsPerDiv = cfg?.voltsPerDivision ?? 1;
  const offset = cfg?.offsetVolts ?? 0;
  const y = h / 2 - ((acquisition.triggerLevel + offset) / voltsPerDiv) * (h / Y_DIV);

  ctx.strokeStyle = '#fb923c';
  ctx.setLineDash([4, 3]);
  line(ctx, 0, y + 0.5, w, y + 0.5);
  ctx.setLineDash([]);
  ctx.fillStyle = '#fb923c';
  ctx.beginPath();
  ctx.moveTo(w - 8, y - 4);
  ctx.lineTo(w, y);
  ctx.lineTo(w - 8, y + 4);
  ctx.fill();
}

function drawCursors(
  ctx: CanvasRenderingContext2D,
  cursors: Cursors,
  frames: Waveform[],
  acquisition: AcquisitionState | null,
  w: number,
  h: number,
) {
  const xa = cursors.a * w;
  const xb = cursors.b * w;

  ctx.setLineDash([5, 4]);
  ctx.lineWidth = 1;
  for (const [x, label] of [[xa, 'A'], [xb, 'B']] as const) {
    ctx.strokeStyle = '#38bdf8';
    line(ctx, x + 0.5, 0, x + 0.5, h);
    ctx.fillStyle = '#38bdf8';
    ctx.font = '11px ui-sans-serif, system-ui';
    ctx.fillText(label, x + 3, 12);
  }
  ctx.setLineDash([]);

  const window = (acquisition?.secondsPerDivision ?? 0) * 10;
  const dt = (cursors.b - cursors.a) * window;

  let dv: number | null = null;
  const frame = frames.find((f) => f.channel !== 0) ?? frames[0];
  if (frame && frame.voltage.length > 1) {
    const idx = (frac: number) => Math.round(frac * (frame.voltage.length - 1));
    dv = frame.voltage[idx(cursors.b)] - frame.voltage[idx(cursors.a)];
  }

  const lines = [
    `Δt = ${formatSeconds(Math.abs(dt))}`,
    `1/Δt = ${dt !== 0 ? formatHertz(1 / Math.abs(dt)) : '—'}`,
    dv !== null ? `ΔV = ${formatVolts(dv)}` : '',
  ].filter(Boolean);

  ctx.fillStyle = 'rgba(10,14,22,0.85)';
  ctx.fillRect(w - 150, 6, 144, 14 * lines.length + 8);
  ctx.fillStyle = '#d6def0';
  lines.forEach((l, i) => ctx.fillText(l, w - 142, 22 + i * 14));
}

function line(ctx: CanvasRenderingContext2D, x0: number, y0: number, x1: number, y1: number) {
  ctx.beginPath();
  ctx.moveTo(x0, y0);
  ctx.lineTo(x1, y1);
  ctx.stroke();
}
