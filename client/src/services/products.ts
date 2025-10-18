import apiClient from './api';
import type { Product } from '../types/product';

export const productsService = {
  async getAllProducts(): Promise<Product[]> {
    const response = await apiClient.get<Product[]>('/product/all');
    return response.data;
  },

  async getProduct(productId: number): Promise<Product> {
    const response = await apiClient.get<Product>(`/product/${productId}`);
    return response.data;
  },

  async getProductsByCategory(categorySlug: string): Promise<Product[]> {
    const response = await apiClient.get<Product[]>(`/product/category/${categorySlug}`);
    return response.data;
  },

  async getProductsBySubcategory(subcategorySlug: string): Promise<Product[]> {
    const response = await apiClient.get<Product[]>(`/product/subcategory/${subcategorySlug}`);
    return response.data;
  }
}