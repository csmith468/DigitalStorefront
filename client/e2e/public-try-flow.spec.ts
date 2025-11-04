import { test, expect } from '@playwright/test';
import { clickTryItOut, fillProductForm, generateTestProduct } from './helpers/products';

test.describe('Public Try-It-Out Flow', () => {
  test('should allow unauthenticated users to try product form without saving', async ({ page }) => {
    await test.step('Navigate to Try It Out page', async () => {
      await page.goto('/');
      await clickTryItOut(page);

      expect(page.url()).toContain('/admin/products/try');
      await expect(page.locator('h1')).toContainText('Product Form');
    });

    await test.step('Fill out product form', async () => {
      const productData = generateTestProduct();
      await fillProductForm(page, productData);

      // Verify form was filled correctly
      await expect(page.locator('input[id="name"]')).toHaveValue(productData.name);
      await expect(page.locator('input[id="slug"]')).toHaveValue(productData.slug);
      await expect(page.locator('input[id="price"]')).toHaveValue(productData.price.toString());
    });

    await test.step('Verify no Save button in try mode', async () => {
      const saveButton = page.locator('button:has-text("Save")');
      await expect(saveButton).not.toBeVisible();
    });

    await test.step('Cancel and return to product list', async () => {
      const cancelButton = page.locator('button:has-text("Cancel")');
      await expect(cancelButton).toBeVisible();
      await cancelButton.click();

      await page.waitForURL('/admin/products');
      await expect(page.locator('h1:has-text("Product Management")')).toBeVisible();
    });
  });
});