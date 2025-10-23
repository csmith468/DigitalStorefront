export interface PaginationParams {
  page: number;
  pageSize: number;
}

export interface ProductFilterParams extends PaginationParams {
  search?: string;
  productTypeId?: number;
  categorySlug?: string;
  subcategorySlug?: string;
}

export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasPrevious: boolean;
  hasNext: boolean;
}