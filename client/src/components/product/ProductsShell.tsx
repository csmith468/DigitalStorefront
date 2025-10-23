import type { ReactNode } from "react";
import type { PaginatedResponse } from "../../types/pagination";
import type { Product } from "../../types/product";
import { LoadingScreen } from "../primitives/LoadingScreen";
import { PaginationWrapper } from "../primitives/PaginationWrapper";

interface ProductsShellProps {
  title: string;
  data: PaginatedResponse<Product> | undefined;
  isLoading: boolean;
  error: Error | null;
  page: number;
  pageSize: number;
  onPageChange: (page: number) => void;
  onPageSizeChange: (pageSize: number) => void;
  emptyMessage?: ReactNode;
  pageSizeOptions?: number[];
  children: (products: Product[]) => ReactNode;
}

  export function ProductsShell({
    title,
    data,
    isLoading,
    error,
    page,
    pageSize,
    onPageChange,
    onPageSizeChange,
    emptyMessage,
    pageSizeOptions = [12, 24, 48],
    children
  }: ProductsShellProps) {

  if (isLoading) {
    return ( <LoadingScreen message="Loading Products..." /> );
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
      <div className="mb-8">
        <h1 className="text-4xl font-bold mb-2 text-text-primary">{title}</h1>
        <div className="w-24 h-1 rounded-full" style={{ background: `linear-gradient(90deg, var(--color-primary) 0%, var(--color-accent) 100%)` }}></div>
      </div>

      {products.length === 0 ? (
        <div className="text-center py-16">
          {emptyMessage || <p className="text-text-secondary">No products found.</p>}
        </div>
      ) : (
        <PaginationWrapper 
          currentPage={page} 
          totalPages={data?.totalPages || 0} 
          pageSize={pageSize} 
          totalCount={data?.totalCount || 0}
          onPageChange={onPageChange} 
          onPageSizeChange={onPageSizeChange} 
          pageSizeOptions={pageSizeOptions} 
          isLoading={isLoading}
        >
          {children(products)}
        </PaginationWrapper>
      )}
    </div>
  );
}