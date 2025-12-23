import { test as base, Page } from '@playwright/test';
import { TIMEOUTS } from './config/timeouts';

export const test = base.extend({
  page: async ({page}, use) => {
    page.on('load', async () => {
      await collapseFeatureChecklist(page);
    });
    await use(page);
  },
});

export async function collapseFeatureChecklist(page: Page) {
  const minimizeButton = page.locator('button[aria-label="Minimize checklist"]');
  if (await minimizeButton.isVisible({ timeout: TIMEOUTS.DOM_UPDATE }).catch(() => false))
    await minimizeButton.click();
}

export { expect, type Page } from '@playwright/test';