import { test, expect } from '@playwright/test';
import { TIMEOUTS } from './config/timeouts';
import { openPaymentModal } from './helpers/checkout';

test.describe('Browse Catalog Flow', () => {
  test('should browse products by category and view product detail', async ({ page }) => {
    await page.setViewportSize({ width: 1280, height: 720 });

    await test.step('Navigate to homepage', async () => {
      await page.goto('/');
      await expect(page.locator('h1')).toBeVisible();
    });

    await test.step('Expand first category in sidebar', async () => {
      const firstCategoryButton = page.locator('aside button.category-item__button').first();
      await firstCategoryButton.waitFor({ state: 'visible' });
      await firstCategoryButton.click();

      await expect(page.locator('aside a.subcategory-link').first()).toBeVisible();
    });

    await test.step('Click first subcategory and view products', async () => {
      const firstSubcategory = page.locator('aside a.subcategory-link').first();
      await firstSubcategory.click();

      expect(page.url()).toContain('/products/');

      await expect(page.locator('li.bg-white').first()).toBeVisible({ timeout: TIMEOUTS.PAGE_LOAD });
    });

    await test.step('Click on first product to view details', async () => {
      const firstProductLink = page.locator('a[href^="/product/"]').first();
      await firstProductLink.click();

      expect(page.url()).toContain('/product/');
      await expect(page.getByText('Loading')).not.toBeVisible({ timeout: TIMEOUTS.PAGE_LOAD });

      await expect(page.locator('h1')).toBeVisible({ timeout: TIMEOUTS.PAGE_LOAD });
      await expect(page.locator('button:has-text("Buy Now")')).toBeVisible({ timeout: TIMEOUTS.PAGE_LOAD });
    });

    await test.step('Click Buy Now opens payment modal', async () => {
      await openPaymentModal(page);

      await expect(page.locator('text=Test Mode')).toBeVisible();
      await expect(page.locator('text=4242 4242 4242 4242')).toBeVisible();

      await page.click('button:has-text("Cancel")');
      await expect(page.locator('text=Complete Purchase')).not.toBeVisible();
    });
  });
});