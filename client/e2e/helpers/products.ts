import { Page } from '@playwright/test';
import { mockProductFormRequest } from '../fixtures/test-data';
import { SuccessMessages } from '../../src/constants/messages';
import { TIMEOUTS } from '../config/timeouts';

export function generateTestProduct() {
  const timestamp = Date.now();
  return {
    ...mockProductFormRequest,
    name: `Test Product ${timestamp}`,
    slug: `test-product-${timestamp}`,
  };
}

export async function fillProductForm(page: Page, productData: typeof mockProductFormRequest) {
  // Wait for everything to load
  await page.waitForSelector('input[id="name"]', { state: 'visible', timeout: TIMEOUTS.PAGE_LOAD });
  await page.waitForSelector('select[id="productTypeId"] option:not([value="0"])', { state: 'attached' });
  await page.waitForSelector('select[id="priceTypeId"] option:not([value="0"])', { state: 'attached' });
  await page.waitForSelector('input[id^="subcategory_"]', { state: 'attached' });

  // Fill in form once data has loaded
  await page.fill('input[id="name"]', productData.name);
  await page.fill('input[id="slug"]', productData.slug);

  if (productData.description) 
    await page.fill('textarea[id="description"]', productData.description);

  const productTypeSelect = page.locator('select[id="productTypeId"]');
  await productTypeSelect.scrollIntoViewIfNeeded();
  await productTypeSelect.selectOption(productData.productTypeId.toString());

  const priceTypeSelect = page.locator('select[id="priceTypeId"]');
  await priceTypeSelect.scrollIntoViewIfNeeded();

  // Wait for any non-placeholder option, then select the first real option
  await page.waitForSelector('select[id="priceTypeId"] option:nth-child(2)', { state: 'attached' });
  await priceTypeSelect.selectOption({ index: 1 });

  await page.fill('input[id="price"]', productData.price.toString());
  await page.fill('input[id="premiumPrice"]', productData.premiumPrice.toString());

  if (productData.subcategoryIds && productData.subcategoryIds.length > 0) {
    for (const id of productData.subcategoryIds) {
      await page.check(`input[id="subcategory_${id}"]`);
    }
  } else {
    await page.check('input[id^="subcategory_"]');
  }

  if (productData.tags && productData.tags.length > 0) {
    const tagInput = page.locator('input[placeholder*="tag"]').first();
    for (const tag of productData.tags) {
      await tagInput.fill(tag);
      await tagInput.press('Enter');
      
      await page.waitForSelector(`text=${tag}`, { state: 'visible', timeout: TIMEOUTS.DOM_UPDATE });
    }
  }

  if (productData.isNew) await page.check('input[id="isNew"]');
  if (productData.isTradeable) await page.check('input[id="isTradeable"]');
  if (productData.isExclusive) await page.check('input[id="isExclusive"]');
  if (productData.isPromotional) await page.check('input[id="isPromotional"]');
}

export async function navigateToAdminProducts(page: Page) {
  await page.goto('/admin?tab=products');
  await page.waitForSelector('h2:has-text("Product Management")');
}

export async function clickTryItOut(page: Page) {
  await navigateToAdminProducts(page);
  await page.click('button:has-text("Try Now")');
  await page.waitForURL('/admin/products/try');
}

export async function createProduct(page: Page) {
  await navigateToAdminProducts(page);
  await page.click('button:has-text("Create New Product")');
  await page.waitForURL('/admin/products/create');

  const productData = generateTestProduct();
  await fillProductForm(page, productData);

  await page.click('button:has-text("Create Product")');
  await page.waitForSelector(`text=${SuccessMessages.Product.created}`, { timeout: TIMEOUTS.FORM_SUBMIT });

  await page.waitForURL(/\/admin\/products\/\d+\/edit/);

  return productData;
}