import { useNavigate } from "react-router-dom";
import { useDeleteProduct, useProducts } from "../../hooks/queries/useProducts";
import { LoadingScreen } from "../primitives/LoadingScreen";
import { usePagination } from "../../hooks/utilities/usePagination";
import { useState } from "react";
import { useProductTypes } from "../../hooks/queries/useMetadata";
import { PaginationWrapper } from "../primitives/PaginationWrapper";
import { EyeIcon, MagnifyingGlassIcon, PencilIcon, TrashIcon } from "@heroicons/react/24/outline";
import { FormInput } from "../primitives/FormInput";
import { FormSelect } from "../primitives/FormSelect";
import { PageHeader } from "../primitives/PageHeader";
import { ConfirmModal } from "../primitives/ConfirmModal";
import { useUser } from "../../contexts/useUser";

export function AdminProductList() {
  const navigate = useNavigate();
  const [searchQuery, setSearchQuery] = useState("");
  const [activeSearch, setActiveSearch] = useState('');
  const [productTypeId, setProductTypeId] = useState<number | undefined>(undefined);
  const [productIdToDelete, setProductIdToDelete] = useState<number | null>(null);
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
  const { isAdmin, canManageProducts } = useUser();

  const deleteMutation = useDeleteProduct();

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    setActiveSearch(searchQuery.trim());
    pagination.resetToFirstPage();
  };

  const handleDelete = async () => {
    if (productIdToDelete === null) return;
    try {
      await deleteMutation.mutateAsync({ productId: productIdToDelete });
      setProductIdToDelete(null);
  
      const remainingItemsOnPage = (data?.totalCount || 0) - 1;
      const maxPage =  Math.ceil(remainingItemsOnPage / pagination.pageSize);
      if (pagination.page > maxPage && maxPage > 0)
        pagination.onPageChange(maxPage);
    } finally {
      setProductIdToDelete(null);
    }
  }

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
    <div className="mb-8">
      <div className="flex flex-col md:flex-row md:justify-between md:items-center gap-4">
        <PageHeader 
          title="Product Management"
          returnLink='/'
          returnText='Back to Home' // FUTURE: eventually admin main
        />
        <button
          onClick={() => navigate("/admin/products/create")}
          className="px-4 py-2 bg-[var(--color-primary)] text-white rounded-md hover:opacity-90 font-medium self-end md:self-auto">
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
        <div className="w-full overflow-x-auto bg-white rounded-lg shadow">
          <table className="w-full divide-y divide-gray-200 text-center">
            <thead className="bg-gray-50">
              <tr>
                <th className={tableHeaderStyle}>Thumbnail</th>
                <th className={tableHeaderStyle}>Name</th>
                <th className={tableHeaderStyle}>Type</th>
                <th className={tableHeaderStyle}>Prices</th>
                <th className={tableHeaderStyle}>Actions</th>
              </tr>
            </thead>
            <tbody className="bg-white divide-y divide-gray-200">
              <tr className="bg-green-50 hover:bg-green-100 border-b-2 border-green-300">
                <td className="px-6 py-4" colSpan={2}>
                  <div>
                    <div className="text-sm font-bold text-green-800">TRY IT OUT</div>
                    <div className="text-xs text-green-600">Test the product form without signing up</div>
                  </div>
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-sm text-green-700 font-sm">
                  Interactive Demo
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                  <div>FREE</div>
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-sm font-semibold">
                  <button
                    onClick={() => navigate('/admin/products/try')}
                    className="text-green-600 hover:text-green-800 inline-flex items-center gap-1"
                  >
                    <PencilIcon className="h-5 w-5" />
                    <span>Try Now</span>
                  </button>
                </td>
              </tr>
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
                      <td
                        onClick={() => navigate(`/admin/products/${product.productId}/edit?tab=images`)}
                        className="px-6 py-4 whitespace-nowrap cursor-pointer"
                      >
                        <div className="flex justify-center">
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
                        </div>
                      </td>
                      <td
                        onClick={() => navigate(`/admin/products/${product.productId}/edit?tab=details`)}
                        className="px-6 py-4 whitespace-nowrap cursor-pointer"
                      >
                        <div className="text-sm font-medium text-gray-900">{product.name}</div>
                        <div className="text-sm text-gray-500">{product.slug}</div>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                        {productType?.typeName || "Unknown"}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                        <div className="flex flex-col gap-1 items-center">
                          <div>{product.priceIcon} {product.price}</div>
                          <div className="text-gray-500">{product.priceIcon} {product.premiumPrice}</div>
                        </div>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm font-medium">
                        {!canManageProducts() || (product.isDemoProduct && !isAdmin()) ? (
                          <div className="flex items-center justify-center gap-2">
                            <button
                              onClick={() => navigate(`/admin/products/${product.productId}/view`)}
                              className="text-[var(--color-primary)] hover:text-[var(--color-primary-light)]"
                              aria-label="View Product"
                            >
                              <EyeIcon className="h-5 w-5" />
                            </button>
                          </div>
                        ) : (
                          <div className="flex items-center justify-center gap-2">
                            <button
                              onClick={() => navigate(`/admin/products/${product.productId}/edit`)}
                              aria-label="Edit Product"
                              className="text-[var(--color-primary)] hover:text-[var(--color-primary-light)] flex items-center gap-1">
                              <PencilIcon className="h-5 w-5" />
                            </button>
                            <button
                              onClick={() => setProductIdToDelete(product.productId)}
                              disabled={deleteMutation.isPending}
                              aria-label="Delete Product"
                              className="text-red-600 hover:text-red-800 flex items-center gap-1 disabled:opacity-50">
                              <TrashIcon className="h-5 w-5" />
                            </button>
                          </div>
                        )}
                      </td>
                    </tr>
                  );
                })
              )}
            </tbody>
          </table>
        </div>
      </PaginationWrapper>

      <ConfirmModal
        isOpen={productIdToDelete !== null}
        title="Delete Product"
        message="Are you sure you want to delete this product? This will also delete all associated images. This action cannot be undone."
        confirmButtonMessage="Delete"
        cancelButtonMessage="Cancel"
        onConfirm={handleDelete}
        onCancel={() => setProductIdToDelete(null)}
      />
    </div>
  );
}
