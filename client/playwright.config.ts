import { defineConfig, devices } from '@playwright/test';

export default defineConfig({

  // Look for test files in the "tests" directory, relative to this configuration file.
  testDir: './e2e',

  // Run all tests in parallel.
  fullyParallel: true,

  // Fail the build on CI if you accidentally left test.only in the source code.
  forbidOnly: !!process.env.CI,

  // Retry on CI only.
  retries: process.env.CI ? 2 : 0,

  // Opt out of parallel tests on CI.
  workers: process.env.CI ? 1 : undefined,

  // Reporter to use
  reporter: 'html',

  // File types to match to
  testMatch: '**/*.spec.ts',

  use: {
    // Base URL to use in actions like `await page.goto('/')`.
    baseURL: 'http://localhost:5173',

    // Collect trace when retrying the failed test.
    trace: 'on-first-retry',

    screenshot: 'only-on-failure',

    // Partition to avoid rate limits
    extraHTTPHeaders: {
      'Test-Partition-Key': `e2e-${Date.now()}-${Math.random().toString(36).slice(2)}`,
    },
  },
  // Configure projects for major browsers.
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
      testIgnore: ['**/src/**', '**/node_modules/**'],
    },
  ],
  // Run your local dev server before starting the tests.
  webServer: {
    command: 'VITE_PLAYWRIGHT=true npm run dev',
    url: 'http://localhost:5173',
    reuseExistingServer: !process.env.CI,
  },
});