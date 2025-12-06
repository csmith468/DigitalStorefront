import { keepPreviousData, useQuery } from "@tanstack/react-query";
import { createProduct,
  getProducts,
  getProductById,
  getProductsByCategory,
  getProductsBySubcategory,
  updateProduct,
  deleteProduct,
  getProductBySlug,
} from "../../services/products";
import type { ProductDetail, ProductFormRequest } from "../../types/product";
import type { ProductFilterParams } from "../../types/pagination";
import { useMutationWithToast } from "../utilities/useMutationWithToast";
import { ErrorMessages, SuccessMessages } from "../../constants/messages";


// Getters
export const useProduct = (productId: number) => {
  return useQuery({
    queryKey: ["product", productId],
    queryFn: ({ signal }) => getProductById(productId, signal),
    enabled: !!productId,
  });
};

export const useProductBySlug = (slug: string) => {
  return useQuery({
    queryKey: ["product", slug],
    queryFn: ({ signal }) => getProductBySlug(slug, signal),
    enabled: !!slug,
  });
}

export const useProducts = (filters: ProductFilterParams) => {
  return useQuery({
    queryKey: ['products', filters],
    queryFn: ({ signal }) => getProducts(filters, signal),
    placeholderData: keepPreviousData,
  });
};

export const useProductsByCategory = (
  categorySlug: string,
  page: number,
  pageSize: number
) => {
  return useQuery({
    queryKey: ['categoryProducts', categorySlug, page, pageSize],
    queryFn: ({ signal }) => getProductsByCategory(categorySlug, page, pageSize, signal),
    enabled: !!categorySlug
  });
};

export const useProductsBySubcategory = (
  subcategorySlug: string,
  page: number,
  pageSize: number
) => {
  return useQuery({
    queryKey: ['subcategoryProducts', subcategorySlug, page, pageSize],
    queryFn: ({ signal }) => getProductsBySubcategory(subcategorySlug, page, pageSize, signal),
    enabled: !!subcategorySlug
  });
};

// Mutations (with toast - similar setup, just pass messages)
export const useCreateProduct = () => {
  return useMutationWithToast({
    mutationFn: (product: ProductFormRequest) => createProduct(product),
    onSuccess: (data, _, queryClient) => {
      queryClient.setQueryData(['product', data.productId], data);
      queryClient.invalidateQueries({ queryKey: ['products'] });
      queryClient.invalidateQueries({ queryKey: ['metadata', 'tags'] });
    },
    successMessage: SuccessMessages.Product.created,
    errorMessage: ErrorMessages.Product.createFailed,
  })
}

export const useUpdateProduct = () => {
  return useMutationWithToast({
    mutationFn: ({ productId, product }: {
      productId: number;
      product: ProductFormRequest;
    }) => updateProduct(productId, product),
    onSuccess: (data: ProductDetail, _, queryClient) => {
      queryClient.setQueryData(['product', data.productId], data);
      queryClient.invalidateQueries({ queryKey: ['products'] });
      queryClient.invalidateQueries({ queryKey: ['metadata', 'tags'] });
    },
    successMessage: SuccessMessages.Product.updated,
    errorMessage: ErrorMessages.Product.updateFailed,
  });
};

export const useDeleteProduct = () => {
  return useMutationWithToast({
    mutationFn: ({ productId }: {
      productId: number;
    }) => deleteProduct(productId),
    onSuccess: (_d, _v, queryClient) => {
      queryClient.invalidateQueries({ queryKey: ['products']});
    },
    successMessage: SuccessMessages.Product.deleted,
    errorMessage: ErrorMessages.Product.deleteFailed,
  });
}