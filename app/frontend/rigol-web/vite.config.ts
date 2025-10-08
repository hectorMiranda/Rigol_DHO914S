import { defineConfig } from 'vitest/config';
import react from '@vitejs/plugin-react';

// During dev, proxy the API + SSE stream to the Functions host so the SPA can
// use same-origin relative URLs (/api/...) and avoid CORS entirely.
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      '/api': {
        target: 'http://localhost:7071',
        changeOrigin: true,
      },
    },
  },
  test: {
    environment: 'jsdom',
    globals: true,
  },
});
