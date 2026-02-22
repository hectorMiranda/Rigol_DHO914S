import { useEffect, useRef } from 'react';
import type { ChannelConfig, Waveform } from '../api/types';

interface Props {
  frames: Waveform[];
  channels: ChannelConfig[];
  xChannel: number;
  yChannel: number;
}

const Y_DIV = 8;

/** XY (Lissajous) plot: one channel's voltage drives X, another drives Y. */
export function XYView({ frames, channels, xChannel, yChannel }: Props) {
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const wrapRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const canvas = canvasRef.current;
    const wrap = wrapRef.current;
    if (!canvas || !wrap) return;

    const dpr = window.devicePixelRatio || 1;
    const size = Math.min(wrap.clientWidth, wrap.clientHeight);
    canvas.width = size * dpr;
    canvas.height = size * dpr;
    canvas.style.width = `${size}px`;
    canvas.style.height = `${size}px`;

    const ctx = canvas.getContext('2d');
    if (!ctx) return;
    ctx.setTransform(dpr, 0, 0, dpr, 0, 0);

    ctx.fillStyle = '#06080f';
    ctx.fillRect(0, 0, size, size);
    ctx.strokeStyle = '#171d2b';
    for (let i = 1; i < Y_DIV; i++) {
      const p = (i * size) / Y_DIV;
      ctx.beginPath(); ctx.moveTo(p, 0); ctx.lineTo(p, size); ctx.stroke();
      ctx.beginPath(); ctx.moveTo(0, p); ctx.lineTo(size, p); ctx.stroke();
    }

    const fx = frames.find((f) => f.channel === xChannel);
    const fy = frames.find((f) => f.channel === yChannel);
    if (!fx || !fy) return;

    const cfgX = channels.find((c) => c.channel === xChannel);
    const cfgY = channels.find((c) => c.channel === yChannel);
    const perDiv = size / Y_DIV;
    const half = size / 2;

    const mapX = (v: number) => half + ((v + (cfgX?.offsetVolts ?? 0)) / (cfgX?.voltsPerDivision ?? 1)) * perDiv;
    const mapY = (v: number) => half - ((v + (cfgY?.offsetVolts ?? 0)) / (cfgY?.voltsPerDivision ?? 1)) * perDiv;

    const n = Math.min(fx.voltage.length, fy.voltage.length);
    ctx.beginPath();
    ctx.moveTo(mapX(fx.voltage[0]), mapY(fy.voltage[0]));
    for (let i = 1; i < n; i++) ctx.lineTo(mapX(fx.voltage[i]), mapY(fy.voltage[i]));
    ctx.strokeStyle = '#38bdf8';
    ctx.lineWidth = 1.2;
    ctx.shadowColor = '#38bdf8';
    ctx.shadowBlur = 6;
    ctx.stroke();
    ctx.shadowBlur = 0;
  }, [frames, channels, xChannel, yChannel]);

  return (
    <div ref={wrapRef} className="scope" style={{ position: 'relative', flex: 1, minHeight: 280, display: 'grid', placeItems: 'center' }}>
      <canvas ref={canvasRef} style={{ display: 'block', borderRadius: 8 }} />
    </div>
  );
}
