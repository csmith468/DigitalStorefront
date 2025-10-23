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
  priceTypeId: number;
  price: number;
  premiumPrice: number;
  primaryImage: ProductImage | null;
  priceIcon: string;
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
  images: ProductImage[];
  subcategories: Subcategory[];
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
}

export interface AddProductImageRequest {
  file: File;
  altText: string | null;
  setAsPrimary: boolean;
}