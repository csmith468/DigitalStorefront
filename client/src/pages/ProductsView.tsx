import { useEffect, useMemo } from "react";
import { useParams } from "react-router-dom";
import { isViewAllSubcategory } from "../types/subcategory";
import { useProductsByCategory, useProductsBySubcategory } from "../hooks/useProducts";
import { useCategories } from "../hooks/useCategories";
import { usePagination } from "../hooks/usePagination";
import { ProductsGrid } from "../components/product/ProductsGrid";

export default function ProductsView() {
  const { categorySlug, subcategorySlug } = useParams();
  const { data: categories } = useCategories();
  const pagination = usePagination();

  useEffect(() => {
    pagination.resetToFirstPage();
  }, [categorySlug, subcategorySlug]);

  const isViewAllInCategory = categorySlug && subcategorySlug && isViewAllSubcategory(subcategorySlug);

  const pageTitle = useMemo(() => {
    if (!categories || !categorySlug) return 'Products';
    const category = categories.find(c => c.slug === categorySlug);
    if (!category) return 'Products';
    if (!subcategorySlug || isViewAllInCategory) return category.name;
    const subcategory = category.subcategories.find(s => s.slug === subcategorySlug);
    return subcategory ? `${category.name}: ${subcategory.name}` : category.name;
  }, [categories, categorySlug, subcategorySlug, isViewAllInCategory]);

  const { data, isLoading, error } = isViewAllInCategory
      ? useProductsByCategory(categorySlug || '', pagination.page, pagination.pageSize)
      : useProductsBySubcategory(subcategorySlug || '', pagination.page, pagination.pageSize);

  return (
    <ProductsGrid
      title={pageTitle}
      data={data}
      isLoading={isLoading}
      error={error}
      {...pagination}
    />
  );
}
