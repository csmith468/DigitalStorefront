import { useMutation, useQueryClient } from "@tanstack/react-query";
import { deleteProductImage, reorderProductImages, setImageAsPrimary, uploadProductImage } from "../../services/products/images";
import type { AddProductImageRequest } from "../../types/product";
import toast from "react-hot-toast";
import { useMutationWithToast } from "../utilities/useMutationWithToast";

export const useUploadProductImage = () => {
  return useMutationWithToast({
    mutationFn: ({ productId, imageData }: {
      productId: number;
      imageData: AddProductImageRequest;
    }) => uploadProductImage(productId, imageData),
    onSuccess: (_, variables, queryClient) => {
      queryClient.invalidateQueries({
        queryKey: ['product', variables.productId],
      });
      queryClient.invalidateQueries({ queryKey: ['products'] });
    },
    successMessage: 'Image uploaded!',
    errorMessage: 'Failed to upload image. Please try again.',
  });
};

export const useDeleteProductImage = () => {
  return useMutationWithToast({
    mutationFn: ({ productId, productImageId }: {
      productId: number;
      productImageId: number;
    }) => deleteProductImage(productId, productImageId),
    onSuccess: (_, variables, queryClient) => {
      queryClient.invalidateQueries({
        queryKey: ['product', variables.productId],
      });
      queryClient.invalidateQueries({ queryKey: ['products'] });
    },
    successMessage: 'Image deleted!',
    errorMessage: 'Failed to deleted image. Please try again.',
  });
};

export const useSetImageAsPrimary = () => {
  return useMutationWithToast({
    mutationFn: ({ productId, productImageId }: {
      productId: number;
      productImageId: number;
    }) => setImageAsPrimary(productId, productImageId),
    onSuccess: (_, variables, queryClient) => {
      queryClient.invalidateQueries({
        queryKey: ['product', variables.productId],
      });
      queryClient.invalidateQueries({ queryKey: ['products'] });
    },
    successMessage: 'Image set as primary!',
    errorMessage: 'Failed to set image as primary. Please try again.',
  });
};

export const useReorderProductImages = () => {
  return useMutationWithToast({
    mutationFn: ({ productId, orderedImageIds }: {
      productId: number;
      orderedImageIds: number[];
    }) => reorderProductImages(productId, orderedImageIds),
    onSuccess: (_, variables, queryClient) => {
      queryClient.invalidateQueries({
        queryKey: ['product"', variables.productId],
      });
      queryClient.invalidateQueries({ queryKey: ['products'] });
    },
    successMessage: 'Images re-ordered!',
    errorMessage: 'Failed to re-order images. Please try again.',
  });
};