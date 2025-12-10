import { useQuery } from "@tanstack/react-query";
import { getOrders } from "../../services/orderService";
import type { PaginationParams } from "../../types/pagination";

export const useOrders = (params: PaginationParams) => {
  return useQuery({
    queryKey: ['orders', params],
    queryFn: ({ signal }) => getOrders(params, signal)
  });
};
