import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  build: {
    rollupOptions: {
      output: {
        manualChunks: {
          'react-vendor': ['react', 'react-dom', 'react-router-dom'],
          'query-vendor': ['@tanstack/react-query'],
          'form-vendor': ['react-hook-form'],
        }
      }
    },
    chunkSizeWarningLimit: 1000
  },
  server: { // temp
      proxy: {
        '/images': {
          target: 'http://localhost:5000',
          changeOrigin: true
        }
      }
    }
})
