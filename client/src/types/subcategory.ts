export interface Subcategory {
  subcategoryId: number;
  name: string;
  slug: string;
}

export const VIEW_ALL_SUBCATEGORY_ID = 0;

export const VIEW_ALL_SUBCATEGORY: Subcategory = {
  subcategoryId: VIEW_ALL_SUBCATEGORY_ID,
  name: 'View All',
  slug: 'all',
};

export const isViewAllSubcategory = (subcategorySlug: string): boolean => {
  return subcategorySlug === VIEW_ALL_SUBCATEGORY.slug;
};