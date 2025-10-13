export interface Category {
  categoryId: number;
  name: string;
  slug: string;
  displayOrder: number;
  subcategories: Subcategories[];
}

export interface Subcategories {
  subcategoryId: number;
  name: string;
  slug: string;
  displayOrder: number;
  imageUrl: string;
}