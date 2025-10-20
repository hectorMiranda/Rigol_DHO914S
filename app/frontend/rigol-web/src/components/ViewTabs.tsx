interface Option<T extends string> {
  key: T;
  label: string;
}

interface Props<T extends string> {
  value: T;
  options: Option<T>[];
  onChange: (value: T) => void;
}

/** A small segmented tab bar for switching the main display mode. */
export function ViewTabs<T extends string>({ value, options, onChange }: Props<T>) {
  return (
    <div style={{ display: 'flex', gap: 4 }}>
      {options.map((o) => (
        <button key={o.key} className={value === o.key ? 'is-active' : ''} onClick={() => onChange(o.key)}>
          {o.label}
        </button>
      ))}
    </div>
  );
}
