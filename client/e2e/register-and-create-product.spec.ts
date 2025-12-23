import { test, expect } from './fixtures';
import { register, generateTestUser, expectUserLoggedIn } from './helpers/auth';
import { createProduct, navigateToAdminProducts } from './helpers/products';
import { TIMEOUTS } from './config/timeouts';

test.describe('Register and Create Product Flow', () => {
  test('should register new user and create a product', async ({ page }) => {
    let productName: string;

    await test.step('Register new user', async () => {
      await page.goto('/');

      const user = generateTestUser();
      await register(page, user);

      await expectUserLoggedIn(page);
    });

    await test.step('Verify user has correct roles', async () => {
      await page.click('[data-testid="user-dropdown"]');
      await expect(page.locator('text=ProductWriter')).toBeVisible();
      await expect(page.locator('text=ImageManager')).toBeVisible();

      await page.keyboard.press('Escape');
    });

    await test.step('Navigate to admin console', async () => {
      await navigateToAdminProducts(page);
      await expect(page.locator('h2:has-text("Product Management")')).toBeVisible();
    });

    await test.step('Create new product', async () => {
      const product = await createProduct(page);
      productName = product.name;
      
      expect(page.url()).toMatch(/\/admin\/products\/\d+\/edit/); // Verify redirect to edit page
    });

    await test.step('Verify product appears in list', async () => {
      await navigateToAdminProducts(page);
      await page.fill('input[placeholder*="Search"]', productName);
      // Search is debounced so just wait for results to appear
      await expect(page.locator(`td:has-text("${productName}")`)).toBeVisible({ timeout: TIMEOUTS.API_CALL });
    });
  });
});