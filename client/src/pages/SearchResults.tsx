import { useEffect } from "react";
import { useSearchParams } from "react-router-dom";
import { useProducts } from "../hooks/useProducts";
import { usePagination } from "../hooks/usePagination";
import { ProductsShell } from "../components/product/ProductsShell";
import ProductsGrid from "../components/product/ProductsGrid";

export default function SearchResults() {
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
    <ProductsShell
      title={`Search Results for "${query}"`}
      data={data}
      isLoading={isLoading}
      error={error}
      {...pagination}>
      {(products) => <ProductsGrid products={products} />}
    </ProductsShell>
  );
}
