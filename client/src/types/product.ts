import type { Tag } from "./metadata";
import type { Subcategory } from "./subcategory";

export interface Product {
  productId: number;
  name: string;
  slug: string;
  productTypeId: number;
  isTradeable: boolean;
  isNew: boolean;
  isPromotional: boolean;
  isExclusive: boolean;
  isDemoProduct: boolean;
  sku: string;
  price: number;
  premiumPrice: number;
  priceIcon: string;
  primaryImage: ProductImage | null;
}

export interface ProductImage {
  productImageId: number;
  productId: number;
  imageUrl: string;
  altText?: string;
  isPrimary: boolean;
  displayOrder: number;
}

export interface ProductDetail extends Product {
  description?: string;
  priceTypeId: number;
  images: ProductImage[];
  subcategories: Subcategory[];
  tags: Tag[];
}

export interface ProductFormRequest {
  name: string;
  slug: string;
  productTypeId: number;
  description: string | null;
  isTradeable: boolean;
  isNew: boolean;
  isPromotional: boolean;
  isExclusive: boolean;
  price: number;
  premiumPrice: number;
  priceTypeId: number;
  subcategoryIds: number[];
  tags: string[];
}

export interface AddProductImageRequest {
  file: File;
  altText: string | null;
  setAsPrimary: boolean;
}