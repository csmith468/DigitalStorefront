import { test, expect } from '@playwright/test';
import { SuccessMessages } from '../src/constants/messages';
import { TIMEOUTS } from './config/timeouts';

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
      await expect(page.locator('button:has-text("Add to Cart")')).toBeVisible({ timeout: TIMEOUTS.PAGE_LOAD });
    });

    await test.step('Add product to cart', async () => {
      await page.click('button:has-text("Add to Cart")');

      // ✅ CHANGED: Verify toast notification (check actual toast message)
      await expect(page.locator(`text=${SuccessMessages.Product.addedToCart}`)).toBeVisible({ timeout: TIMEOUTS.DOM_UPDATE });
    });
  });
});