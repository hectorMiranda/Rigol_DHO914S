import { useEffect } from 'react';

type Handlers = Record<string, (e: KeyboardEvent) => void>;

/**
 * Binds single-key shortcuts. Keys are matched case-insensitively; ' ' is space.
 * Ignored while typing in an input/select/textarea so panel controls still work.
 */
export function useHotkeys(handlers: Handlers) {
  useEffect(() => {
    const onKey = (e: KeyboardEvent) => {
      const target = e.target as HTMLElement | null;
      const tag = target?.tagName;
      if (tag === 'INPUT' || tag === 'SELECT' || tag === 'TEXTAREA') return;
      if (e.metaKey || e.ctrlKey || e.altKey) return;

      const handler = handlers[e.key.toLowerCase()];
      if (handler) {
        e.preventDefault();
        handler(e);
      }
    };
    window.addEventListener('keydown', onKey);
    return () => window.removeEventListener('keydown', onKey);
  }, [handlers]);
}
