import { useQuery } from "@tanstack/react-query";
import { getProductTypes, getPriceTypes } from "../services/common";

export const useProductTypes = () => {
  return useQuery({
    queryKey: ['productTypes'],
    queryFn: getProductTypes,
    staleTime: Infinity
  });
};
export const usePriceTypes = () => {
  return useQuery({
    queryKey: ['priceTypes'],
    queryFn: getPriceTypes,
    staleTime: Infinity
  });
};