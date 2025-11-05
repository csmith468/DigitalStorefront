import { fetchers } from '../fetchers';
import type { Product, ProductDetail, ProductFormRequest } from '../../types/product';
import type { PaginatedResponse, ProductFilterParams } from '../../types/pagination';

export const getProductById = (productId: number, signal?: AbortSignal): Promise<ProductDetail> =>
  fetchers.get<ProductDetail>(`/products/${productId}`, { signal });

export const getProductBySlug = (slug: string, signal?: AbortSignal): Promise<ProductDetail> =>
  fetchers.get<ProductDetail>(`/products/slug/${slug}`, { signal });

export const getProducts = (params: ProductFilterParams, signal?: AbortSignal): Promise<PaginatedResponse<Product>> =>
  fetchers.get<PaginatedResponse<Product>>('/products', { params, signal });

export const getProductsByCategory = (categorySlug: string, page: number, pageSize: number, signal?: AbortSignal): Promise<PaginatedResponse<Product>> =>
  fetchers.get<PaginatedResponse<Product>>(`/products/category/${categorySlug}`, {
    params: { page, pageSize },
    signal
  });

export const getProductsBySubcategory = (subcategorySlug: string, page: number, pageSize: number, signal?: AbortSignal): Promise<PaginatedResponse<Product>> =>
  fetchers.get<PaginatedResponse<Product>>(`/products/subcategory/${subcategorySlug}`, {
    params: { page, pageSize },
    signal
  });

export const createProduct = (product: ProductFormRequest): Promise<ProductDetail> =>
  fetchers.post<ProductDetail>('/products', product);

export const updateProduct = (productId: number, product: ProductFormRequest): Promise<ProductDetail> =>
  fetchers.put<ProductDetail>(`/products/${productId}`, product);

export const deleteProduct = (productId: number): Promise<void> =>
  fetchers.delete(`/products/${productId}`);