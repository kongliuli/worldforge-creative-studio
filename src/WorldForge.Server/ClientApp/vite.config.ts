import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import { VitePWA } from 'vite-plugin-pwa'

export default defineConfig({
  plugins: [
    vue(),
    VitePWA({
      registerType: 'autoUpdate',
      devOptions: { enabled: false },
      manifest: {
        name: 'WorldForge',
        short_name: 'WorldForge',
        description: '离线优先的创作工作室',
        display: 'standalone',
        start_url: '/',
        theme_color: '#1e1b4b',
        background_color: '#0f1117',
        icons: [
          {
            src: 'favicon.svg',
            sizes: 'any',
            type: 'image/svg+xml',
            purpose: 'any',
          },
        ],
      },
      workbox: {
        globPatterns: ['**/*.{js,css,html,ico,svg,woff2}'],
        runtimeCaching: [
          {
            urlPattern: /\/api\/health$/,
            handler: 'NetworkFirst',
            options: {
              cacheName: 'worldforge-health',
              networkTimeoutSeconds: 3,
            },
          },
          {
            urlPattern: /\/api\/projects$/,
            handler: 'NetworkFirst',
            options: {
              cacheName: 'worldforge-projects',
              networkTimeoutSeconds: 3,
              expiration: { maxEntries: 1, maxAgeSeconds: 60 * 60 * 24 * 7 },
            },
          },
        ],
      },
    }),
  ],
  server: {
    port: 5173,
    proxy: {
      '/api': {
        target: 'http://localhost:5280',
        changeOrigin: true,
      },
    },
  },
})
