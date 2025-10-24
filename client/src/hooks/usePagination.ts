import { useState } from "react";

interface UsePaginationOptions {
  initialPageSize?: number;
  pageSizeOptions?: number[];
}

export function usePagination(options: UsePaginationOptions = {}) {
  const {
    initialPageSize = 12,
    pageSizeOptions = [12, 24, 48]
  } = options;

  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(initialPageSize);

  const handlePageSizeChange = (newPageSize: number) => {
    setPageSize(newPageSize);
    setPage(1);
  }

  const resetToFirstPage = () => setPage(1);

  return {
    page, pageSize, onPageChange: setPage, onPageSizeChange: handlePageSizeChange, pageSizeOptions, resetToFirstPage
  };
}