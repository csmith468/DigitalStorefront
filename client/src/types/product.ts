export interface Product {
  productId: number;
  name: string;
  slug: string;
  productTypeId: number;
  isTradeable: boolean;
  isNew: boolean;
  isPromotional: boolean;
  isExclusive: boolean;
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
}