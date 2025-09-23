import { useCallback, useEffect, useState } from 'react';
import { api } from '../api/client';
import type { ScopeStatus } from '../api/types';

/**
 * Loads the aggregate scope status (device + acquisition + channels) and polls
 * it on an interval so the UI reflects external changes. Returns the status, a
 * manual refresh, and an optimistic setter for local edits.
 */
export function useScopeStatus(pollMs = 4000) {
  const [status, setStatus] = useState<ScopeStatus | null>(null);
  const [error, setError] = useState<string | null>(null);

  const refresh = useCallback(async () => {
    try {
      setStatus(await api.getStatus());
      setError(null);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'failed to load status');
    }
  }, []);

  useEffect(() => {
    void refresh();
    const id = setInterval(() => void refresh(), pollMs);
    return () => clearInterval(id);
  }, [refresh, pollMs]);

  return { status, error, refresh, setStatus } as const;
}
