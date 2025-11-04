import { ErrorMessages, SuccessMessages } from "../../constants/messages";
import { deleteProductImage, reorderProductImages, setImageAsPrimary, uploadProductImage } from "../../services/products/images";
import type { AddProductImageRequest } from "../../types/product";
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
    successMessage: SuccessMessages.Image.uploaded,
    errorMessage: ErrorMessages.Image.uploadFailed,
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
    successMessage: SuccessMessages.Image.deleted,
    errorMessage: ErrorMessages.Image.deleteFailed,
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
    successMessage: SuccessMessages.Image.setPrimary,
    errorMessage: ErrorMessages.Image.setPrimaryFailed,
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
        queryKey: ['product', variables.productId],
      });
      queryClient.invalidateQueries({ queryKey: ['products'] });
    },
    successMessage: SuccessMessages.Image.reordered,
    errorMessage: ErrorMessages.Image.reorderFailed,
  });
};