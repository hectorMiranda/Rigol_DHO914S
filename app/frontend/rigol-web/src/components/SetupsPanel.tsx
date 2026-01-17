import { useCallback, useEffect, useState } from 'react';
import { api } from '../api/client';
import type { SetupSummary } from '../api/types';

interface Props {
  onRecalled: () => void;
}

/** Save the current instrument state under a name and recall it later. */
export function SetupsPanel({ onRecalled }: Props) {
  const [setups, setSetups] = useState<SetupSummary[]>([]);
  const [name, setName] = useState('');

  const reload = useCallback(async () => {
    try {
      setSetups(await api.listSetups());
    } catch {
      /* ignore */
    }
  }, []);

  useEffect(() => {
    void reload();
  }, [reload]);

  const save = async () => {
    const trimmed = name.trim();
    if (!trimmed) return;
    await api.saveSetup(trimmed);
    setName('');
    await reload();
  };

  const recall = async (n: string) => {
    await api.recallSetup(n);
    onRecalled();
  };

  const remove = async (n: string) => {
    await api.deleteSetup(n);
    await reload();
  };

  return (
    <section className="panel">
      <h2>Setups</h2>

      <div style={{ display: 'flex', gap: 6, marginBottom: 8 }}>
        <input
          value={name}
          placeholder="setup name"
          onChange={(e) => setName(e.target.value)}
          onKeyDown={(e) => e.key === 'Enter' && void save()}
          style={{ flex: 1 }}
        />
        <button onClick={save} disabled={!name.trim()}>Save</button>
      </div>

      {setups.length === 0 ? (
        <p style={{ color: 'var(--text-dim)', fontSize: 13 }}>No saved setups.</p>
      ) : (
        <ul style={{ listStyle: 'none', margin: 0, padding: 0, display: 'flex', flexDirection: 'column', gap: 4 }}>
          {setups.map((s) => (
            <li key={s.name} style={{ display: 'flex', alignItems: 'center', gap: 6, fontSize: 13 }}>
              <span style={{ flex: 1 }}>{s.name}</span>
              <button onClick={() => void recall(s.name)} title="Recall">↺</button>
              <button onClick={() => void remove(s.name)} title="Delete">✕</button>
            </li>
          ))}
        </ul>
      )}
    </section>
  );
}
