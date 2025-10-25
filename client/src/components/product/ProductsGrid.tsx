import type { PaginatedResponse } from "../../types/pagination";
import type { Product } from "../../types/product";
import { LoadingScreen } from "../primitives/LoadingScreen";
import { PageHeader } from "../primitives/PageHeader";
import { PaginationWrapper } from "../primitives/PaginationWrapper";
import ProductCard from "./ProductCard";

interface ProductsGridProps {
  title: string;
  data: PaginatedResponse<Product> | undefined;
  isLoading: boolean;
  error: Error | null;
  page: number;
  pageSize: number;
  onPageChange: (page: number) => void;
  onPageSizeChange: (pageSize: number) => void;
  pageSizeOptions?: number[];
}

export function ProductsGrid({
  title,
  data,
  isLoading,
  error,
  page,
  pageSize,
  onPageChange,
  onPageSizeChange,
  pageSizeOptions = [12, 24, 48],
}: ProductsGridProps) {
  if (isLoading) {
    return <LoadingScreen message="Loading Products..." />;
  }

  if (error) {
    return (
      <div className="container mx-auto px-4 py-8">
        <div className="flex justify-center items-center min-h-[400px]">
          <div className="text-danger text-center">
            <p className="text-lg font-semibold mb-2">Error</p>
            <p>Failed to load products. Please try again.</p>
          </div>
        </div>
      </div>
    );
  }

  const products = data?.items || [];

  return (
    <div className="container mx-auto px-4 py-8">
      <PageHeader 
        title={title}
        returnLink='/'
        returnText='Back to Home'
      />

      <PaginationWrapper
        page={page}
        totalPages={data?.totalPages || 0}
        pageSize={pageSize}
        totalCount={data?.totalCount || 0}
        onPageChange={onPageChange}
        onPageSizeChange={onPageSizeChange}
        pageSizeOptions={pageSizeOptions}
        isLoading={isLoading}>
        {products.length === 0 ? (
          <div className="flex flex-col items-center justify-center py-16 px-4">
            <h3 className="text-xl font-semibold text-text-primary mb-2">
              No Products Found
            </h3>
            <p className="text-text-secondary">
              Please check back later or adjust your search criteria.
            </p>
          </div>
        ) : (
          <ul className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-2">
            {products.map((product) => (
              <ProductCard key={product.productId} product={product} />
            ))}
          </ul>
        )}
      </PaginationWrapper>
    </div>
  );
}
