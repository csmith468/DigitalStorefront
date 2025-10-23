import { useQuery } from "@tanstack/react-query";
import { getCategories } from "../services/categories";

export const useCategories = () => {
  return useQuery({
    queryKey: ['categories'],
    queryFn: getCategories,
    staleTime: 1000 * 60 * 10 // 10 minutes
  });
};