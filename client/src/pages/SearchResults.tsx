import { useEffect } from "react";
import { useSearchParams } from "react-router-dom";
import { useProducts } from "../hooks/queries/useProducts";
import { usePagination } from "../hooks/utilities/usePagination";
import { ProductsGrid } from "../components/product/ProductsGrid";

export function SearchResults() {
  const [searchParams] = useSearchParams();
  const query = searchParams.get('q') || '';
  const pagination = usePagination();

  useEffect(() => {
    pagination.resetToFirstPage();
  }, [query]);

  const { data, isLoading, error } = useProducts({
    search: query,
    page: pagination.page,
    pageSize: pagination.pageSize,
  });

  return (
    <ProductsGrid
      title={`Search Results for "${query}"`}
      data={data}
      isLoading={isLoading}
      error={error}
      {...pagination}
    />
  )
}
