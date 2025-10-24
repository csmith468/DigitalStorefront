import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { createProduct, getProducts, getProductById, getProductsByCategory, getProductsBySubcategory, updateProduct } from '../services/products';
import { uploadProductImage, deleteProductImage, setImageAsPrimary } from '../services/products/images';
import type { ProductFormRequest, AddProductImageRequest } from "../types/product";
import type { ProductFilterParams } from "../types/pagination";


export const useProduct = (productId: number) => {
  return useQuery({
    queryKey: ['product', productId],
    queryFn: () => getProductById(productId),
    enabled: !!productId
  });
};

export const useProducts = (filters: ProductFilterParams) => {
  return useQuery({
    queryKey: ['products', filters],
    queryFn: () => getProducts(filters)
  });
};

export const useProductsByCategory = (categorySlug: string, page: number, pageSize: number) => {
  return useQuery({
    queryKey: ['categoryProducts', categorySlug, page, pageSize],
    queryFn: () => getProductsByCategory(categorySlug, page, pageSize),
    enabled: !!categorySlug
  });
};

export const useProductsBySubcategory = (subcategorySlug: string, page: number, pageSize: number) => {
  return useQuery({
    queryKey: ['subcategoryProducts', subcategorySlug, page, pageSize],
    queryFn: () => getProductsBySubcategory(subcategorySlug, page, pageSize),
    enabled: !!subcategorySlug
  });
};

// mutationFn(input: inputType) => functionToCallApi(input),
// onSuccess: (data, variables (input), context) => {
//    queryClient.setQueryData(['queryToSetOutputTo', idToSetOutputTo], output)
//    queryClient.invalidateQueries({ queryKey: ['queryKeysToRefetch'] })

export const useCreateProduct = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (product: ProductFormRequest) => createProduct(product),
    onSuccess: (data) => {
      queryClient.setQueryData(['product', data.productId], data);
      queryClient.invalidateQueries({ queryKey: ['products'] });
    },
  });
};

export const useUpdateProduct = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ productId, product }: { productId: number; product: ProductFormRequest }) => updateProduct(productId, product),
    onSuccess: (data) => {
      queryClient.setQueryData(['product', data.productId], data);
      queryClient.invalidateQueries({ queryKey: ['products'] });
    },
  });
};

export const useUploadProductImage = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ productId, imageData }: { productId: number, imageData: AddProductImageRequest }) => 
      uploadProductImage(productId, imageData),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['product', variables.productId] });
    },
  });
};

export const useDeleteProductImage = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ productId, productImageId }: { productId: number, productImageId: number }) => 
      deleteProductImage(productId, productImageId),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['product', variables.productId] });
    },
  });
};

export const useSetImageAsPrimary = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ productId, productImageId }: { productId: number, productImageId: number }) => 
      setImageAsPrimary(productId, productImageId),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['product', variables.productId] });
    },
  });
};
