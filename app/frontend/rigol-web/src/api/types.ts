// Mirror of the backend DTOs (camelCase JSON, enums as camelCase strings).

export type Coupling = 'dc' | 'ac' | 'gnd';
export type RunState = 'stopped' | 'running' | 'single';
export type TriggerSlope = 'positive' | 'negative' | 'either';

export interface DeviceInfo {
  manufacturer: string;
  model: string;
  serialNumber: string;
  firmwareVersion: string;
  simulated: boolean;
  rawIdentity?: string;
}

export interface ChannelConfig {
  channel: number;
  enabled: boolean;
  voltsPerDivision: number;
  offsetVolts: number;
  coupling: Coupling;
  probeRatio: number;
  label?: string;
}

export interface ChannelUpdate {
  enabled?: boolean;
  voltsPerDivision?: number;
  offsetVolts?: number;
  coupling?: Coupling;
  probeRatio?: number;
  label?: string;
}

export interface Waveform {
  channel: number;
  voltage: number[];
  timeOrigin: number;
  timeIncrement: number;
  timestamp: number;
  sampleCount: number;
  duration: number;
}

export interface Measurement {
  name: string;
  code: string;
  value: number | null;
  unit: string;
}

export interface MeasurementSet {
  channel: number;
  items: Measurement[];
  timestamp: number;
}

export interface AcquisitionState {
  runState: RunState;
  secondsPerDivision: number;
  timebaseOffset: number;
  triggerSource: number;
  triggerLevel: number;
  triggerSlope: TriggerSlope;
  triggerStatus: string;
}

export interface AcquisitionUpdate {
  secondsPerDivision?: number;
  timebaseOffset?: number;
  triggerSource?: number;
  triggerLevel?: number;
  triggerSlope?: TriggerSlope;
}

export interface ScopeStatus {
  device: DeviceInfo;
  acquisition: AcquisitionState;
  channels: ChannelConfig[];
  lastError?: string;
}

/** One pushed frame on the SSE stream. */
export interface StreamFrame {
  t: number;
  frames: Waveform[];
}

export interface Spectrum {
  channel: number;
  window: string;
  sampleRate: number;
  frequencyStep: number;
  magnitudesDb: number[];
  timestamp: number;
  bins: number;
}

export type MathOp = 'add' | 'subtract' | 'multiply';

export interface SetupSummary {
  name: string;
  savedAt: number;
  channelCount: number;
}

export interface UartFrame {
  time: number;
  value: number;
  framingError: boolean;
}

export interface UartDecodeResult {
  channel: number;
  baud: number;
  threshold: number;
  frames: UartFrame[];
  timestamp: number;
}

export interface MaskRequest {
  channel: number;
  lowerVolts: number;
  upperVolts: number;
  points?: number;
}

export interface MaskResult {
  channel: number;
  lowerVolts: number;
  upperVolts: number;
  total: number;
  violations: number;
  pass: boolean;
  violationRatio: number;
  timestamp: number;
}
