import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { createProduct, getAllProducts, getProductById, getProductsByCategory, getProductsBySubcategory, updateProduct } from '../services/products';
import { uploadProductImage, deleteProductImage, setImageAsPrimary } from '../services/products/images';
import type { ProductFormRequest, AddProductImageRequest } from "../types/product";


export const useProducts = () => {
  return useQuery({
    queryKey: ['products'],
    queryFn: getAllProducts
  });
};

export const useProduct = (productId: number) => {
  return useQuery({
    queryKey: ['product', productId],
    queryFn: () => getProductById(productId),
    enabled: !!productId
  });
};

export const useProductsByCategory = (categorySlug: string) => {
  return useQuery({
    queryKey: ['categoryProducts', categorySlug],
    queryFn: () => getProductsByCategory(categorySlug),
    enabled: !!categorySlug
  });
};

export const useProductsBySubcategory = (subcategorySlug: string) => {
  return useQuery({
    queryKey: ['subcategoryProducts', subcategorySlug],
    queryFn: () => getProductsBySubcategory(subcategorySlug),
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
