import apiClient from '../api';
import type { Product, ProductDetail, ProductFormRequest } from '../../types/product';

export const getAllProducts = async (): Promise<Product[]> => {
  const response = await apiClient.get<Product[]>('/product/all');
  return response.data;
};

export const getProductById = async (productId: number): Promise<Product> => {
  const response = await apiClient.get<Product>(`/product/${productId}`);
  return response.data;
};

export const getProductsByCategory = async (categorySlug: string): Promise<Product[]> => {
  const response = await apiClient.get<Product[]>(`/product/category/${categorySlug}`);
  return response.data;
};

export const getProductsBySubcategory = async (subcategorySlug: string): Promise<Product[]> => {
  const response = await apiClient.get<Product[]>(`/product/subcategory/${subcategorySlug}`);
  return response.data;
};

export const createProduct = async (product: ProductFormRequest): Promise<ProductDetail> => {
  const response = await apiClient.post<ProductDetail>('/product/create', product);
  return response.data;
};

export const updateProduct = async (productId: number, product: ProductFormRequest): Promise<ProductDetail> => {
  const response = await apiClient.put<ProductDetail>(`product/update/${productId}`, product);
  return response.data;
};
