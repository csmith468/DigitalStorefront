import { useQuery } from "@tanstack/react-query";
import { getProductTypes, getPriceTypes, getCategories, getTags } from "../../services/metadata";

export const useCategories = () => {
  return useQuery({
    queryKey: ['metadata', 'categories'],
    queryFn: getCategories,
    staleTime: 1000 * 60 * 10 // 10 minutes
  });
};

export const useTags = () => {
  return useQuery({
    queryKey: ['metadata', 'tags'],
    queryFn: getTags,
    staleTime: 1000 * 60 * 5 // 5 minutes
  });
};

export const useProductTypes = () => {
  return useQuery({
    queryKey: ['metadata', 'productTypes'],
    queryFn: getProductTypes,
    staleTime: Infinity
  });
};

export const usePriceTypes = () => {
  return useQuery({
    queryKey: ['metadata', 'priceTypes'],
    queryFn: getPriceTypes,
    staleTime: Infinity
  });
};