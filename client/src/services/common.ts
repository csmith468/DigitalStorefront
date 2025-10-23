import apiClient from "./api";
import type { PriceType, ProductType } from "../types/common";

export const getProductTypes = async (): Promise<ProductType[]> => {
  const response = await apiClient.get<ProductType[]>('/common/product-types');
  return response.data;
}

export const getPriceTypes = async (): Promise<PriceType[]> => {
  const response = await apiClient.get<PriceType[]>('/common/price-types');
  return response.data;
}