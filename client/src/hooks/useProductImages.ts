import { useMutation, useQueryClient } from "@tanstack/react-query";
import { deleteProductImage, reorderProductImages, setImageAsPrimary, uploadProductImage } from "../services/products/images";
import type { AddProductImageRequest } from "../types/product";

// NOTE: Error handling happens here because FormShell is not used for image uploader

export const useUploadProductImage = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ productId, imageData }: {
      productId: number;
      imageData: AddProductImageRequest;
    }) => uploadProductImage(productId, imageData),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({
        queryKey: ['product', variables.productId],
      });
      queryClient.invalidateQueries({ queryKey: ['products'] });
    },
    onError: (error) => {
      console.error('Failed to upload image: ', error);
      alert('Failed to upload image. Please try again.');
    }
  });
};

export const useDeleteProductImage = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ productId, productImageId }: {
      productId: number;
      productImageId: number;
    }) => deleteProductImage(productId, productImageId),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({
        queryKey: ['product', variables.productId],
      });
      queryClient.invalidateQueries({ queryKey: ['products'] });
    },
    onError: (error) => {
      console.error('Failed to delete image: ', error);
      alert('Failed to delete image. Please try again.');
    }
  });
};

export const useSetImageAsPrimary = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ productId, productImageId }: {
      productId: number;
      productImageId: number;
    }) => setImageAsPrimary(productId, productImageId),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({
        queryKey: ['product', variables.productId],
      });
      queryClient.invalidateQueries({ queryKey: ['products'] });
    },
    onError: (error) => {
      console.error('Failed to update image: ', error);
      alert('Failed to update image. Please try again.');
    }
  });
};

export const useReorderProductImages = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ productId, orderedImageIds }: {
      productId: number;
      orderedImageIds: number[];
    }) => reorderProductImages(productId, orderedImageIds),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({
        queryKey: ['product"', variables.productId],
      });
      queryClient.invalidateQueries({ queryKey: ['products'] });
    },
    onError: (error) => {
      console.error('Failed to reorder images:', error);
      alert('Failed to reorder images. Please try again.');
    }
  });
};