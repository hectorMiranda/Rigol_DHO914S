import { describe, expect, it } from 'vitest';
import { engineering, formatMeasurement, formatSeconds, formatVolts } from '../utils/format';

describe('engineering', () => {
  it('uses SI prefixes', () => {
    expect(engineering(1500, 'Hz', 2)).toBe('1.50 kHz');
    expect(engineering(0.0012, 'V', 2)).toBe('1.20 mV');
    expect(engineering(2e-6, 's', 1)).toBe('2.0 µs');
  });

  it('handles zero and non-finite', () => {
    expect(engineering(0, 'V')).toBe('0 V');
    expect(engineering(Number.NaN, 'V')).toBe('– V');
  });
});

describe('formatVolts / formatSeconds', () => {
  it('formats with the right unit', () => {
    expect(formatVolts(3.3)).toBe('3.30 V');
    expect(formatSeconds(5e-4, 0)).toBe('500 µs');
  });
});

describe('formatMeasurement', () => {
  it('renders a placeholder for null', () => {
    expect(formatMeasurement(null, 'V')).toBe('——');
  });

  it('routes units to the right formatter', () => {
    expect(formatMeasurement(1000, 'Hz')).toBe('1.000 kHz');
    expect(formatMeasurement(50, '%')).toBe('50.00 %');
  });
});
