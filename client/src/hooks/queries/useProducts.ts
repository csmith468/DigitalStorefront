import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
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


// Getters
export const useProduct = (productId: number) => {
  return useQuery({
    queryKey: ["product", productId],
    queryFn: () => getProductById(productId),
    enabled: !!productId,
  });
};

export const useProductBySlug = (slug: string) => {
  return useQuery({
    queryKey: ["product", slug],
    queryFn: () => getProductBySlug(slug),
    enabled: !!slug,
  });

}

export const useProducts = (filters: ProductFilterParams) => {
  return useQuery({
    queryKey: ['products', filters],
    queryFn: () => getProducts(filters)
  });
};

export const useProductsByCategory = (
  categorySlug: string,
  page: number,
  pageSize: number
) => {
  return useQuery({
    queryKey: ['categoryProducts', categorySlug, page, pageSize],
    queryFn: () => getProductsByCategory(categorySlug, page, pageSize),
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
    queryFn: () => getProductsBySubcategory(subcategorySlug, page, pageSize),
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
    successMessage: 'Product created!',
    errorMessage: 'Failed to create product. Please try again.',
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
    successMessage: 'Product updated!',
    errorMessage: 'Failed to update product. Please try again.',
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
    successMessage: 'Product deleted!',
    errorMessage: 'Failed to delete product. Please try again.',
  });
}