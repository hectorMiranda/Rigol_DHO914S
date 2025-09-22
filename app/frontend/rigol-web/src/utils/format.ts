// Engineering-notation formatters with SI prefixes — the way a scope labels things.

const PREFIXES: Array<[number, string]> = [
  [1e9, 'G'],
  [1e6, 'M'],
  [1e3, 'k'],
  [1, ''],
  [1e-3, 'm'],
  [1e-6, 'µ'],
  [1e-9, 'n'],
  [1e-12, 'p'],
];

/** Format a value with an SI prefix and unit, e.g. 1.2e-3 V -> "1.20 mV". */
export function engineering(value: number, unit: string, digits = 2): string {
  if (value === 0) return `0 ${unit}`;
  if (!Number.isFinite(value)) return `– ${unit}`;

  const abs = Math.abs(value);
  for (const [scale, prefix] of PREFIXES) {
    if (abs >= scale) {
      return `${(value / scale).toFixed(digits)} ${prefix}${unit}`;
    }
  }
  const [scale, prefix] = PREFIXES[PREFIXES.length - 1];
  return `${(value / scale).toFixed(digits)} ${prefix}${unit}`;
}

export const formatVolts = (v: number, digits = 2) => engineering(v, 'V', digits);
export const formatSeconds = (s: number, digits = 2) => engineering(s, 's', digits);
export const formatHertz = (hz: number, digits = 3) => engineering(hz, 'Hz', digits);

/** A measurement value that may be null (no signal). */
export function formatMeasurement(value: number | null, unit: string): string {
  if (value === null || value === undefined) return '——';
  if (unit === 'Hz') return formatHertz(value);
  if (unit === 's') return formatSeconds(value);
  if (unit === 'V') return formatVolts(value);
  return `${value.toFixed(2)} ${unit}`;
}
