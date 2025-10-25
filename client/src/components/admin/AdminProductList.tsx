import { useNavigate } from "react-router-dom";
import { useProducts } from "../../hooks/useProducts";
import { LoadingScreen } from "../primitives/LoadingScreen";
import { usePagination } from "../../hooks/usePagination";
import { useState } from "react";
import { useProductTypes } from "../../hooks/useCommon";
import { PaginationWrapper } from "../primitives/PaginationWrapper";
import { LockClosedIcon, MagnifyingGlassIcon, PencilIcon } from "@heroicons/react/24/outline";
import { FormInput } from "../primitives/FormInput";
import { FormSelect } from "../primitives/FormSelect";
import { PageHeader } from "../primitives/PageHeader";

export function AdminProductList() {
  const navigate = useNavigate();
  const [searchQuery, setSearchQuery] = useState("");
  const [activeSearch, setActiveSearch] = useState('');
  const [productTypeId, setProductTypeId] = useState<number | undefined>(undefined);
  const pagination = usePagination({
    initialPageSize: 10,
    pageSizeOptions: [10, 25, 50],
  });

  const { data: productTypes } = useProductTypes();
  const { data, isLoading, error } = useProducts({
    search: activeSearch,
    productTypeId,
    page: pagination.page,
    pageSize: pagination.pageSize,
  });

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    setActiveSearch(searchQuery.trim());
    pagination.resetToFirstPage();
  };

  if (isLoading) {
    return <LoadingScreen message="Loading Products..." />;
  }

  if (error) {
    return (
      <div className="container mx-auto px-4 py-8">
        <div className="text-danger text-center">
          <p className="text-lg font-semibold mb-2">Error</p>
          <p>Failed to load products. Please try again.</p>
        </div>
      </div>
    );
  }

  const products = data?.items || [];

  const tableHeaderStyle = "px-6 py-3 text-xs font-medium text-gray-500 uppercase tracking-wider";

  return (
    <div className="container mx-auto px-4 py-8">
      <div className="flex justify-between items-center mb-8">
        <PageHeader 
          title="Product Management"
          returnLink='/'
          returnText='Back to Home' // eventually admin main
        />
        <button
          onClick={() => navigate("/admin/products/create")}
          className="px-4 py-2 bg-[var(--color-primary)] text-white rounded-md hover:opacity-90 font-medium">
          + Create New Product
        </button>
      </div>

      <div className="mb-6 grid grid-cols-1 md:grid-cols-2 gap-4">
        <form onSubmit={handleSearch} className="flex gap-2 items-end">
          <div className="flex-1">
            <FormInput
              id="search"
              label="Search Products"
              value={searchQuery}
              placeholder="Search Products..."
              onChange={(_, value) => setSearchQuery(value as string)}
            />
          </div>
          <button
            type="submit"
            aria-label="Search"
            className="px-4 py-2 bg-[var(--color-primary)] text-white rounded-md hover:opacity-90 h-[42px] flex items-center justify-center">
            <MagnifyingGlassIcon className="h-5 w-5" />
          </button>
        </form>

        <FormSelect
          id="productTypeId"
          label="Product Type"
          value={productTypeId || ""}
          onChange={(_, value) =>
            setProductTypeId(value ? Number(value) : undefined)
          }
          options={productTypes || []}
          getOptionValue={(pt) => pt.productTypeId}
          getOptionLabel={(pt) => pt.typeName}
          placeholder="All Types"
        />
      </div>

      <PaginationWrapper
        {...pagination}
        totalPages={data?.totalPages || 0}
        totalCount={data?.totalCount || 0}>
        <div className="overflow-x-auto bg-white rounded-lg shadow">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className={tableHeaderStyle}>Thumbnail</th>
                <th className={tableHeaderStyle}>Name</th>
                <th className={tableHeaderStyle}>Type</th>
                <th className={tableHeaderStyle}>Prices</th>
                <th className={`${tableHeaderStyle} text-right`}>Actions</th>
              </tr>
            </thead>
            <tbody className="bg-white divide-y divide-gray-200">
              {products.length === 0 ? (
                <tr>
                  <td colSpan={5} className="px-6 py-8 text-center text-gray-500">
                    No products found.
                  </td>
                </tr>
              ) : (
                products.map((product) => {
                  const productType = productTypes?.find(
                    (pt) => pt.productTypeId === product.productTypeId
                  );

                  return (
                    <tr key={product.productId} className="hover:bg-gray-50">
                      <td className="px-6 py-4 whitespace-nowrap">
                        {product.primaryImage ? (
                          <img
                            src={product.primaryImage.imageUrl}
                            alt={product.name}
                            className="h-12 w-12 object-cover rounded"
                          />
                        ) : (
                          <div className="h-12 w-12 bg-gray-200 rounded flex items-center justify-center text-gray-400 text-xs">
                            No Image
                          </div>
                        )}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap">
                        <div className="text-sm font-medium text-gray-900">{product.name}</div>
                        <div className="text-sm text-gray-500">{product.slug}</div>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                        {productType?.typeName || "Unknown"}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                        {product.priceIcon} {product.premiumPrice}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                        {/* {product.isDemoProduct ? (
                          <div className="flex items-center justify-end gap-2 text-gray-400">
                            <LockClosedIcon className="h-5 w-5" />
                            <span>Demo Product</span>
                          </div>
                        ) : ( */}
                          <div className="flex items-center justify-end gap-2">
                            <button
                              onClick={() => navigate(`/admin/products/${product.productId}/edit`)}
                              className="text-[var(--color-primary)] hover:text-[var(--color-primary-light)] flex items-center gap-1">
                              <PencilIcon className="h-5 w-5" />
                              Edit
                            </button>
                          </div>
                        {/* )} */}
                      </td>
                    </tr>
                  );
                })
              )}
            </tbody>
          </table>
        </div>
      </PaginationWrapper>
    </div>
  );
}
