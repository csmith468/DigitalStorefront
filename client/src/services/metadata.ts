import { fetchers } from "./fetchers";
import type { Category, PriceType, ProductType, Tag } from "../types/metadata";

export const getCategories = (signal?: AbortSignal): Promise<Category[]> => 
  fetchers.get<Category[]>('/metadata/categories', { signal});

export const getTags = (signal?: AbortSignal): Promise<Tag[]> => 
  fetchers.get<Tag[]>('/metadata/tags', { signal});

export const getProductTypes = (signal?: AbortSignal): Promise<ProductType[]> => 
  fetchers.get<ProductType[]>('/metadata/product-types', { signal});

export const getPriceTypes = (signal?: AbortSignal): Promise<PriceType[]> => 
  fetchers.get<PriceType[]>('/metadata/price-types', { signal});