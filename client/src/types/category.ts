import type { Subcategory } from "./subcategory";

export interface Category {
  categoryId: number;
  name: string;
  slug: string;
  displayOrder: number;
  subcategories: Subcategory[];
}