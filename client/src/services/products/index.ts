import apiClient from '../api';
import type { Product, ProductDetail, ProductFormRequest } from '../../types/product';
import type { PaginatedResponse, ProductFilterParams } from '../../types/pagination';

export const getProductById = async (productId: number): Promise<ProductDetail> => {
  const response = await apiClient.get<ProductDetail>(`/product/${productId}`);
  return response.data;
};

export const getProducts = async (params: ProductFilterParams): Promise<PaginatedResponse<Product>> => {
  const response = await apiClient.get<PaginatedResponse<Product>>('/product', { params });
  return response.data;
};

export const getProductsByCategory = async (categorySlug: string, page: number, pageSize: number): Promise<PaginatedResponse<Product>> => {
  const response = await apiClient.get<PaginatedResponse<Product>>(`/product/category/${categorySlug}`, {
    params: { page, pageSize }
  });
  return response.data;
};

export const getProductsBySubcategory = async (subcategorySlug: string, page: number, pageSize: number): Promise<PaginatedResponse<Product>> => {
  const response = await apiClient.get<PaginatedResponse<Product>>(`/product/subcategory/${subcategorySlug}`, {
    params: { page, pageSize }
  });
  return response.data;
};

export const createProduct = async (product: ProductFormRequest): Promise<ProductDetail> => {
    const response = await apiClient.post<ProductDetail>('/product', product);
    return response.data;
  };

export const updateProduct = async (productId: number, product: ProductFormRequest): Promise<ProductDetail> => {
  const response = await apiClient.put<ProductDetail>(`/product/${productId}`, product);
  return response.data;
};

export const deleteProduct = async (productId: number): Promise<void> => {
  await apiClient.delete(`/product/${productId}`);
};