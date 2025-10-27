import apiClient from "../api";
import type { AddProductImageRequest } from "../../types/product";
import type { ProductImage } from "../../types/product";

export const uploadProductImage = async (productId: number, imageData: AddProductImageRequest): Promise<ProductImage> => {
  const formData = new FormData();
  formData.append('file', imageData.file);
  if (imageData.altText)
    formData.append('altText', imageData.altText);
  formData.append('setAsPrimary', String(imageData.setAsPrimary));

  const response = await apiClient.post<ProductImage>(`/products/${productId}/images`, formData,
    { headers: { 'Content-Type': 'multipart/form-data' } }
  );
  return response.data;
};

export const deleteProductImage = async (productId: number, productImageId: number): Promise<void> => {
  await apiClient.delete(`/products/${productId}/images/${productImageId}`);
};

export const setImageAsPrimary = async (productId: number, productImageId: number): Promise<void> => {
  await apiClient.put(`/products/${productId}/images/${productImageId}/set-primary`);
};

export const reorderProductImages = async (productId: number, orderedImageIds: number[]): Promise<void> => {
  await apiClient.put(`/products/${productId}/images/reorder`, orderedImageIds);
};