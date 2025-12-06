import type { Tag } from "../../types/metadata";
import type { PaginatedResponse } from "../../types/pagination";
import type { Product, ProductDetail, ProductFormRequest } from "../../types/product";
import type { Subcategory } from "../../types/subcategory";
import { mockSubcategories, mockTags } from "./metadata-fixtures";

export const mockProduct: Product = {
  productId: 1,
  name: 'Test Product',
  slug: 'test-product',
  productTypeId: 1,
  isTradeable: false,
  isNew: true,
  isPromotional: false,
  isExclusive: false,
  isDemoProduct: false,
  sku: 'ABC123',
  price: 5000,
  premiumPrice: 4000,
  priceIcon: '★',
  primaryImage: null,
  updatedAt: null
};

export const mockProductDetail: ProductDetail = {
  ...mockProduct,
  description: 'Test description',
  priceTypeId: 1,
  images: [],
  subcategories: mockSubcategories,
  tags: [mockTags[0]]
};

export const mockProductFormRequest: ProductFormRequest = {
  name: 'Test Product',
  slug: 'test-product',
  productTypeId: 1,
  description: 'Test description',
  isTradeable: false,
  isNew: true,
  isPromotional: false,
  isExclusive: false,
  price: 5000,
  premiumPrice: 4000,
  priceTypeId: 1,
  subcategoryIds: [1],
  tags: ['dog'],
  updatedAt: null
};

export const mockPaginatedResponse: PaginatedResponse<Product> = {
  items: [mockProduct],
  totalCount: 1,
  page: 1,
  pageSize: 12,
  totalPages: 1,
  hasPrevious: false,
  hasNext: false
};

export const createMockProductDetail = (overrides: Partial<ProductDetail> = {}): ProductDetail => ({
  ...mockProductDetail,
  ...overrides,
});

export class ProductDetailBuilder {
  private product: ProductDetail = { ...mockProductDetail };

  withId(id: number) {
    this.product.productId = id;
    return this;
  }

  asDemo() {
    this.product.isDemoProduct = true;
    this.product.name = 'Demo Pet';
    this.product.slug = 'demo-pet';
    return this;
  }

  withName(name: string, slug?: string) {
    this.product.name = name;
    this.product.slug = slug ?? name.toLowerCase().replace(/\s+/g, '-');
    return this;
  }

  withTags(tags: Array<Tag>) {
    this.product.tags = tags;
    return this;
  }

  withSubcategories(subcategories: Array<Subcategory>) {
    this.product.subcategories = subcategories;
    return this;
  }

  build(): ProductDetail {
    return { ...this.product };
  }
}