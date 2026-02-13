import { fileURLToPath, URL } from 'node:url';
import { defineConfig } from 'vite';
import vue from '@vitejs/plugin-vue';

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [vue()],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url))
    }
  },
  server: { 
    port: 5173,
    strictPort: true,
    proxy: {
      // ✅ Proxy pour l'API backend
      '/api': {
        target: 'https://localhost:7250',
        changeOrigin: true,
        secure: false,  // Accepter les certificats auto-signés du backend
        rewrite: (path) => path
      }
    }
  }
});
