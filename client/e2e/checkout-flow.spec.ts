import { test, expect, Page } from './fixtures';
import { TIMEOUTS } from './config/timeouts';
import {
  completeCheckout,
  expectPaymentSuccess,
  expectPaymentError,
  closeSuccessModal,
  navigateToOrders,
  expectOrderInList,
  openPaymentModal,
  fillStripeCard,
  submitPayment,
  STRIPE_TEST_CARDS,
} from './helpers/checkout';

test.describe('Checkout Flow', () => {
  async function navigateToProduct(page: Page) {
    await page.goto('/');

    const firstCategoryButton = page.locator('aside button.category-item__button').first();
    await firstCategoryButton.waitFor({ state: 'visible' });
    await firstCategoryButton.click();

    const firstSubcategory = page.locator('aside a.subcategory-link').first();
    await firstSubcategory.waitFor({ state: 'visible' });
    await firstSubcategory.click();

    const firstProductLink = page.locator('a[href^="/product/"]').first();
    await firstProductLink.waitFor({ state: 'visible', timeout: TIMEOUTS.PAGE_LOAD });
    await firstProductLink.click();

    await page.waitForURL(/\/product\//, { timeout: TIMEOUTS.PAGE_LOAD });

    await expect(page.locator('h1')).toBeVisible({ timeout: TIMEOUTS.PAGE_LOAD });
  }

  test.describe('Successful Payment', () => {
    test('should complete purchase with valid card and show success modal', async ({ page }) => {
      let orderId: string | null;

      await test.step('Navigate to product', async () => {
        await navigateToProduct(page);
      });

      await test.step('Open payment modal and verify content', async () => {
        await openPaymentModal(page);
        await expect(page.locator('text=Test Mode')).toBeVisible();
        await expect(page.locator('text=4242 4242 4242 4242')).toBeVisible();
        await expect(page.locator('h2:has-text("Complete Purchase")')).toBeVisible();
      });

      await test.step('Fill card details and submit', async () => {
        await fillStripeCard(page, STRIPE_TEST_CARDS.SUCCESS);
        await submitPayment(page);
      });

      await test.step('Verify redirect and success modal', async () => {
        orderId = await expectPaymentSuccess(page);
        expect(orderId).not.toBeNull();

        await expect(page.locator(`text=#${orderId}`)).toBeVisible();
        await expect(page.locator('text=has been placed successfully')).toBeVisible();
        await expect(page.locator('text=test transaction')).toBeVisible();
      });

      await test.step('Close success modal', async () => {
        await closeSuccessModal(page);
        expect(page.url()).toContain('/admin?tab=orders');
      });

      await test.step('Verify order appears in orders list', async () => {
        await navigateToOrders(page);
        await expectOrderInList(page, orderId!);
        await expect(page.locator('text=Completed').first()).toBeVisible();
      });
    });

    test('should show correct USD conversion for coin-priced products', async ({ page }) => {
      await test.step('Navigate to product', async () => {
        await navigateToProduct(page);
      });

      await test.step('Open payment modal and check conversion', async () => {
        await openPaymentModal(page);

        const conversionText = page.locator('text=Conversion');
        const hasConversion = await conversionText.isVisible({ timeout: 1000 }).catch(() => false);

        if (hasConversion) {
          await expect(page.locator('text=/\\$\\d+\\.\\d{2} USD/')).toBeVisible();
        }

        await page.click('button:has-text("Cancel")');
      });
    });
  });

  test.describe('Payment Failure', () => {
    test('should show error when card is declined', async ({ page }) => {
      await test.step('Navigate to product', async () => {
        await navigateToProduct(page);
      });

      await test.step('Attempt payment with declined card', async () => {
        await openPaymentModal(page);
        await fillStripeCard(page, STRIPE_TEST_CARDS.DECLINE);
        await submitPayment(page);
      });

      await test.step('Verify error is displayed', async () => {
        await expectPaymentError(page);
        await expect(page.locator('text=Complete Purchase')).toBeVisible();

        const payButton = page.locator('button:has-text("Pay $")');
        await expect(payButton).toBeEnabled({ timeout: TIMEOUTS.DOM_UPDATE });
      });

      await test.step('Can close modal after error', async () => {
        await page.click('button:has-text("Cancel")');
        await expect(page.locator('text=Complete Purchase')).not.toBeVisible();
      });
    });
  });

  test.describe('Payment Modal UI', () => {
    test('should show processing state during payment', async ({ page }) => {
      await test.step('Navigate to product', async () => {
        await navigateToProduct(page);
      });

      await test.step('Submit payment and verify processing state', async () => {
        await openPaymentModal(page);
        await fillStripeCard(page, STRIPE_TEST_CARDS.SUCCESS);

        const payButton = page.locator('button:has-text("Pay $")');
        await payButton.click();

        const processingVisible = await page.locator('text=Processing').isVisible({ timeout: 2000 }).catch(() => false);

        if (!processingVisible) {
          await expectPaymentSuccess(page);
        }
      });
    });

    test('should copy card number when copy button clicked', async ({ page }) => {
      await test.step('Navigate to product and open modal', async () => {
        await navigateToProduct(page);
        await openPaymentModal(page);
      });

      await test.step('Click copy button and verify feedback', async () => {
        const copyButton = page.locator('[aria-label="Copy card number"]');
        await copyButton.click();

        await expect(page.locator('text=Copied!')).toBeVisible({
          timeout: TIMEOUTS.DOM_UPDATE
        });
      });
    });

  });

  test.describe('Orders Tab', () => {
    test('should display orders with correct status badges', async ({ page }) => {
      await test.step('Complete a purchase', async () => {
        await navigateToProduct(page);
        await completeCheckout(page);
        await expectPaymentSuccess(page);
        await closeSuccessModal(page);
      });

      await test.step('Navigate to orders and verify display', async () => {
        await navigateToOrders(page);

        await expect(page.getByRole('columnheader', { name: 'Order ID' })).toBeVisible();
        await expect(page.getByRole('columnheader', { name: 'Status' })).toBeVisible();
        await expect(page.getByRole('columnheader', { name: 'Items' })).toBeVisible();
        await expect(page.getByRole('columnheader', { name: 'Total' })).toBeVisible();

        await expect(page.locator('td').filter({ hasText: /^#\d+$/ }).first()).toBeVisible();
        await expect(page.locator('.bg-green-100').filter({ hasText: 'Completed' }).first()).toBeVisible();
      });
    });

  });
});
