import { defineConfig, devices } from '@playwright/test'

const apiUrl = 'http://127.0.0.1:5280'
const webUrl = 'http://127.0.0.1:5173'
const isCi = !!process.env.CI

export default defineConfig({
  testDir: './e2e',
  fullyParallel: false,
  forbidOnly: isCi,
  retries: isCi ? 2 : 0,
  workers: 1,
  reporter: isCi ? 'github' : 'list',
  use: {
    baseURL: webUrl,
    trace: 'on-first-retry',
  },
  projects: [{ name: 'chromium', use: { ...devices['Desktop Chrome'] } }],
  webServer: [
    {
      command: isCi
        ? 'dotnet run --project ../WorldForge.Server.csproj --urls http://127.0.0.1:5280 -c Release --no-build'
        : 'dotnet run --project ../WorldForge.Server.csproj --urls http://127.0.0.1:5280',
      url: `${apiUrl}/api/health`,
      reuseExistingServer: !isCi,
      timeout: 120_000,
      env: { ASPNETCORE_ENVIRONMENT: 'Development' },
    },
    {
      command: 'npm run dev -- --port 5173 --strictPort',
      url: webUrl,
      reuseExistingServer: !isCi,
      timeout: 60_000,
    },
  ],
})
