interface Props {
  values: number[];
  width?: number;
  height?: number;
  color?: string;
}

/** A tiny inline trend line (SVG) auto-scaled to the value range. */
export function Sparkline({ values, width = 280, height = 40, color = '#38bdf8' }: Props) {
  const finite = values.filter((v) => Number.isFinite(v));
  if (finite.length < 2) {
    return <svg width={width} height={height} role="img" aria-label="trend" />;
  }

  const min = Math.min(...finite);
  const max = Math.max(...finite);
  const span = max - min || 1;

  const points = values
    .map((v, i) => {
      const x = (i / (values.length - 1)) * width;
      const y = height - ((v - min) / span) * (height - 4) - 2;
      return `${x.toFixed(1)},${y.toFixed(1)}`;
    })
    .join(' ');

  return (
    <svg width={width} height={height} role="img" aria-label="trend">
      <polyline points={points} fill="none" stroke={color} strokeWidth={1.5} />
    </svg>
  );
}
