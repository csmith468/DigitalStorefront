export interface Product {
  productId: number;
  name: string;
  slug: string;
  description: string;
  productTypeId: number;
  isTradeable: boolean;
  isNew: boolean;
  isPromotional: boolean;
  isExclusive: boolean;
  sku: string;
  priceTypeId: number;
  price: number;
  premiumPrice: number;
  imageUrl: string;
}