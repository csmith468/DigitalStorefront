import type { Order } from "../types/order";
import type { PaginatedResponse, PaginationParams } from "../types/pagination";
import { fetchers } from "./fetchers";

export const getOrders = (params: PaginationParams, signal?: AbortSignal): Promise<PaginatedResponse<Order>> =>
  fetchers.get<PaginatedResponse<Order>>('/orders', {
    params: { page: params.page, pageSize: params.pageSize },
    signal
  });