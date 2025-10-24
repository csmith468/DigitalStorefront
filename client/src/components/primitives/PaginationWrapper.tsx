import type { ReactNode } from "react";
import { FormSelect } from "./FormSelect";
import { ChevronLeftIcon, ChevronRightIcon } from "@heroicons/react/24/outline";

interface PaginationProps {
  children: ReactNode;
  currentPage: number;
  totalPages: number;
  pageSize: number;
  totalCount: number;
  onPageChange: (page: number) => void;
  onPageSizeChange: (pageSize: number) => void;
  pageSizeOptions?: number[];
  isLoading?: boolean;
}

export function PaginationWrapper({
  children,
  currentPage,
  totalPages,
  pageSize,
  totalCount,
  onPageChange,
  onPageSizeChange,
  pageSizeOptions = [12, 24, 48],
  isLoading = false,
}: PaginationProps) {
  const hasPrevious = currentPage > 1;
  const hasNext = currentPage < totalPages;
  const startItem = Math.min((currentPage - 1) * pageSize + 1, totalCount);
  const endItem = Math.min(currentPage * pageSize, totalCount);
  const buttonStyle = "w-10 h-10 flex items-center justify-center bg-[var(--color-primary)] text-white rounded-md disabled:opacity-30 disabled:cursor-not-allowed hover:opacity-90 transition-opacity";

  return (
    <div className="flex flex-col gap-6">
      <div className="flex justify-between items-center">
        <div className="text-sm text-text-secondary">
          {totalCount > 0 ? (
            <>Showing {startItem} - {endItem} of {totalCount}</>
          ) : (
            <>No items found</>
          )}
        </div>

        <div className="flex items-center gap-2">
          <FormSelect
            id="pageSize"
            label="Items per Page"
            value={pageSize}
            onChange={(_, value) => onPageSizeChange(Number(value))}
            disablePlaceholder
            options={pageSizeOptions}
            type="number"
            getOptionLabel={(v) => v.toString()}
            getOptionValue={(v) => v}
            overrideClass="px-3 py-1 border border-gray-300 rounded-md text-sm focus:outline-none focus:ring-2 focus:ring-[var(--color-primary)] disabled:opacity-50 disabled:cursor-not-allowed"
          />
        </div>
      </div>

      <div>{children}</div>

      {totalPages > 1 && (
        <div className="flex justify-center items-center gap-4 mt-4">
          <button
            onClick={() => onPageChange(currentPage - 1)}
            disabled={!hasPrevious || isLoading}
            aria-label="Previous Page"
            className={buttonStyle}>
            <ChevronLeftIcon className="h-4 w-4" />
          </button>

          <span className="text-text-primary font-medium min-w-[120px] text-center">
            Page {currentPage} of {totalPages}
          </span>

          <button
            onClick={() => onPageChange(currentPage + 1)}
            disabled={!hasNext || isLoading}
            aria-label="Next Page"
            className={buttonStyle}>
            <ChevronRightIcon className="h-4 w-4" />
          </button>
        </div>
      )}
    </div>
  );
}