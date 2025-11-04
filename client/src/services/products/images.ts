import { fetchers } from "../fetchers";
import type { AddProductImageRequest } from "../../types/product";
import type { ProductImage } from "../../types/product";

export const uploadProductImage = async (productId: number, imageData: AddProductImageRequest): Promise<ProductImage> => {
  const formData = new FormData();
  formData.append('file', imageData.file);
  if (imageData.altText)
    formData.append('altText', imageData.altText);
  formData.append('setAsPrimary', String(imageData.setAsPrimary));

  return fetchers.post<ProductImage>(`/products/${productId}/images`, formData,
    { headers: { 'Content-Type': 'multipart/form-data' } }
  );
};

export const deleteProductImage = (productId: number, productImageId: number) => 
  fetchers.delete(`/products/${productId}/images/${productImageId}`);

export const setImageAsPrimary = (productId: number, productImageId: number) => 
  fetchers.put(`/products/${productId}/images/${productImageId}/set-primary`);

export const reorderProductImages = (productId: number, orderedImageIds: number[]) => 
  fetchers.put(`/products/${productId}/images/reorder`, orderedImageIds);