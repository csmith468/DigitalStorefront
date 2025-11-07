import type { Category, PriceType, ProductType, Tag } from "../../types/metadata";
import type { Subcategory } from "../../types/subcategory";

export const mockSubcategories: Subcategory[] = [
  { subcategoryId: 1, name: 'Dogs', slug: 'dogs' },
  { subcategoryId: 2, name: 'Cats', slug: 'cats' },
];

export const mockCategories: Category[] = [
  {
    categoryId: 1,
    name: 'Pets',
    slug: 'pets',
    subcategories: mockSubcategories,
    displayOrder: 0
  },
];

export const mockProductTypes: ProductType[] = [
  { productTypeId: 1, typeName: 'Pet', typeCode: 'pet', description: null },
  { productTypeId: 2, typeName: 'Furniture', typeCode: 'furniture', description: null },
];

export const mockPriceTypes: PriceType[] = [
  { priceTypeId: 1, priceTypeName: 'Coins', priceTypeCode: 'coins', icon: '★' },
  { priceTypeId: 2, priceTypeName: 'Dollars', priceTypeCode: 'dollars', icon: '$' },
];

export const mockTags: Tag[] = [
  { tagId: 1, name: 'red' },
  { tagId: 2, name: 'blue' },
];

