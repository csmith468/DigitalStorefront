import { Page, expect } from '@playwright/test';
import { TIMEOUTS } from '../config/timeouts';

export const STRIPE_TEST_CARDS = {
  SUCCESS: '4242424242424242',
  DECLINE: '4000000000000002',
  INSUFFICIENT_FUNDS: '4000000000009995',
  EXPIRED: '4000000000000069',
  INCORRECT_CVC: '4000000000000127',
} as const;

const TEST_CARD_EXPIRY = '12/30';
const TEST_CARD_CVC = '123';
const TEST_CARD_ZIP = '12345';

// Stripe embeds card inputs in an iframe, so we need to use frameLocator
export async function fillStripeCard(
  page: Page,
  cardNumber: string = STRIPE_TEST_CARDS.SUCCESS
) {
  const stripeFrame = page.frameLocator('iframe[title="Secure card payment input frame"]');

  const cardInput = stripeFrame.locator('[placeholder="Card number"]');
  await cardInput.waitFor({ state: 'visible', timeout: TIMEOUTS.PAGE_LOAD });
  await cardInput.fill(cardNumber);

  const expiryInput = stripeFrame.locator('[placeholder="MM / YY"]');
  await expiryInput.fill(TEST_CARD_EXPIRY);

  const cvcInput = stripeFrame.locator('[placeholder="CVC"]');
  await cvcInput.fill(TEST_CARD_CVC);

  const zipInput = stripeFrame.locator('[placeholder="ZIP"]');
  if (await zipInput.isVisible({ timeout: 1000 }).catch(() => false)) {
    await zipInput.fill(TEST_CARD_ZIP);
  }
}

export async function openPaymentModal(page: Page) {
  await page.click('button:has-text("Buy Now")');
  await expect(page.locator('text=Complete Purchase')).toBeVisible({
    timeout: TIMEOUTS.DOM_UPDATE
  });
}

export async function submitPayment(page: Page) {
  const payButton = page.locator('button:has-text("Pay $")');
  await payButton.click();
}

export async function completeCheckout(page: Page, cardNumber: string = STRIPE_TEST_CARDS.SUCCESS) {
  await openPaymentModal(page);
  await fillStripeCard(page, cardNumber);
  await submitPayment(page);
}

export async function expectPaymentSuccess(page: Page): Promise<string | null> {
  await page.waitForURL(/\/admin\?orderSuccess=\d+/, {
    timeout: TIMEOUTS.FORM_SUBMIT
  });

  const orderId = getOrderIdFromUrl(page.url());

  await expect(page.locator('text=Payment Successful!')).toBeVisible({
    timeout: TIMEOUTS.DOM_UPDATE
  });

  return orderId;
}

export async function expectPaymentError(page: Page, errorMessage?: string) {
  if (errorMessage) {
    await expect(page.locator(`text=${errorMessage}`)).toBeVisible({
      timeout: TIMEOUTS.FORM_SUBMIT
    });
  } else {
    await expect(page.locator('.text-red-600, .text-danger')).toBeVisible({
      timeout: TIMEOUTS.FORM_SUBMIT
    });
  }
}

export async function closeSuccessModal(page: Page) {
  await page.click('button:has-text("Close")');
  await expect(page.locator('text=Payment Successful!')).not.toBeVisible({
    timeout: TIMEOUTS.DOM_UPDATE
  });
}

export async function navigateToOrders(page: Page) {
  await page.goto('/admin?tab=orders');
  await expect(page.locator('text=Order ID')).toBeVisible({
    timeout: TIMEOUTS.PAGE_LOAD
  });
}

export async function expectOrderInList(page: Page, orderId: string | number) {
  await expect(page.locator(`text=#${orderId}`)).toBeVisible({
    timeout: TIMEOUTS.API_CALL
  });
}

export function getOrderIdFromUrl(url: string): string | null {
  const match = url.match(/orderSuccess=(\d+)/);
  return match ? match[1] : null;
}
