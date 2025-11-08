import { Page } from '@playwright/test';
import { mockRegisterRequest } from '../fixtures/test-data';
import { TIMEOUTS } from '../config/timeouts';

export function generateTestUser() {
  const timestamp = Date.now();
  return {
    ...mockRegisterRequest,
    username: `testuser_${timestamp}`,
    email: `test_${timestamp}@example.com`,
  };
};

export async function register(page: Page, userDetails: typeof mockRegisterRequest) {
  await page.click('button:has-text("Login")');
  await page.waitForSelector('text=Sign In', { state: 'visible' });

  const registerSwitchButton = page.locator('button:has-text("Register Here")');
  await registerSwitchButton.waitFor({ state: 'visible', timeout: 5000 });
  await registerSwitchButton.click();
  await page.waitForSelector('text=Create Account', { state: 'visible' });

  await page.waitForSelector('input[id="confirmPassword"]', { state: 'visible', timeout: TIMEOUTS.DOM_UPDATE });

  await page.fill('input[id="username"]', userDetails.username);
  await page.fill('input[id="password"]', userDetails.password);
  await page.fill('input[id="confirmPassword"]', userDetails.confirmPassword);

  if (userDetails.firstName) 
    await page.fill('input[id="firstName"]', userDetails.firstName);
  if (userDetails.lastName) 
    await page.fill('input[id="lastName"]', userDetails.lastName);
  if (userDetails.email) 
    await page.fill('input[id="email"]', userDetails.email);

  await page.click('button[type="submit"]:has-text("Register")');

  await page.waitForSelector('text=Create Account', { state: 'hidden', timeout: TIMEOUTS.FORM_SUBMIT });
}

export async function expectUserLoggedIn(page: Page) {
  await page.waitForSelector('button:has-text("Login")', { state: 'hidden' });
  await page.waitForSelector('[data-testid="user-dropdown"]', { state: 'visible' });
}