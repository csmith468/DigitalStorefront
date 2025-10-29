import apiClient from "./api";
import type { Category, PriceType, ProductType, Tag } from "../types/metadata";

export const getCategories = async (): Promise<Category[]> => {
  const response = await apiClient.get<Category[]>('/metadata/categories');
  return response.data;
};

export const getTags = async (): Promise<Tag[]> => {
  const response = await apiClient.get<Tag[]>('/metadata/tags');
  return response.data;
};

export const getProductTypes = async (): Promise<ProductType[]> => {
  const response = await apiClient.get<ProductType[]>('/metadata/product-types');
  return response.data;
}

export const getPriceTypes = async (): Promise<PriceType[]> => {
  const response = await apiClient.get<PriceType[]>('/metadata/price-types');
  return response.data;
}