import type { Subcategory } from "./subcategory";

export interface Category {
  categoryId: number;
  name: string;
  slug: string;
  displayOrder: number;
  subcategories: Subcategory[];
}

export interface Tag {
  tagId: number;
  name: string;
}

export interface ProductType {
  productTypeId: number;
  typeName: string;
  typeCode: string;
  description: string | null;
}

export interface PriceType {
  priceTypeId: number;
  priceTypeName: string;
  priceTypeCode: string;
  icon: string;
}
