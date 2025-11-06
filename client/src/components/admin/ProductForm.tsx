import type { ProductDetail, ProductFormRequest } from "../../types/product";
import { useCreateProduct, useUpdateProduct } from "../../hooks/queries/useProducts";
import { useCategories, useTags } from "../../hooks/queries/useMetadata";
import { useProductTypes, usePriceTypes } from "../../hooks/queries/useMetadata";
import { FormInput } from "../primitives/FormInput";
import { FormTextArea } from "../primitives/FormTextArea";
import { FormSelect } from "../primitives/FormSelect";
import { FormCheckbox } from "../primitives/FormCheckbox";
import { FormShell } from "../primitives/FormShell";
import { OverlappingLabelBox } from "../primitives/OverlappingLabelBox";
import { FormChipInput } from "../primitives/FormChipInput";
import { useCallback } from "react";
import type { PriceType, ProductType } from "../../types/metadata";

export type ProductFormMode = 'edit' | 'view' | 'try';

interface ProductFormProps {
  existingProduct?: ProductDetail; // if exists, edit mode
  onSuccess: (product?: ProductDetail) => void;
  onCancel: () => void;
  mode?: ProductFormMode;
}

export function ProductForm ({
  existingProduct,
  onSuccess,
  onCancel,
  mode = 'view', // defaulting to view only because I made the pages that use this visible to anyone 
                 // (not only when logged in) so view mode is the safest default
}: ProductFormProps) {
  const isEditing = !!existingProduct;
  const isViewOnly = mode === 'view';
  const hideSubmit = mode !== 'edit';

  // State (equivalent to data())
  const initial: ProductFormRequest = {
    name: existingProduct?.name || "",
    slug: existingProduct?.slug || "",
    productTypeId: existingProduct?.productTypeId || 0,
    description: existingProduct?.description || null,
    isTradeable: existingProduct?.isTradeable || false,
    isNew: existingProduct?.isNew || true,
    isPromotional: existingProduct?.isPromotional || false,
    isExclusive: existingProduct?.isExclusive || false,
    price: existingProduct?.price || 0,
    premiumPrice: existingProduct?.premiumPrice || 0,
    priceTypeId: existingProduct?.priceTypeId || 0,
    subcategoryIds: existingProduct?.subcategories?.map((s) => s.subcategoryId) || [],
    tags: existingProduct?.tags?.map((t) => t.name) || [],
  };

  // Hooks (queries and mutations)
  const { data: categories } = useCategories();
  const { data: productTypes } = useProductTypes();
  const { data: priceTypes } = usePriceTypes();
  const { data: tags } = useTags();

  const createMutation = useCreateProduct();
  const updateMutation = useUpdateProduct();

  const loading = createMutation.isPending || updateMutation.isPending;
  const submitText = isEditing ? "Update Product" : "Create Product";

  // Validation
  const validate = (formData: ProductFormRequest): string | null => {
    if (!formData.name.trim()) return "Name is required";
    if (!formData.slug.trim()) return "Slug is required";
    if (formData.productTypeId === 0) return "Product type is required";
    if (formData.priceTypeId === 0) return "Price type is required";
    if (formData.subcategoryIds.length === 0) return "Select at least one subcategory";
    if (formData.price <= 0 || formData.premiumPrice <= 0) return "Prices must be greater than 0";
    if (formData.premiumPrice > formData.price) return "Premium price cannot exceed regular price";
    if (formData.priceTypeId === 1 && (!Number.isInteger(formData.premiumPrice) || !Number.isInteger(formData.price))) {
      return "Coin prices must be integers";
    }
    return null;
  };

  const onSubmit = async (formData: ProductFormRequest) => {
    let result;
    if (isEditing) {
      result = await updateMutation.mutateAsync({
        productId: existingProduct.productId,
        product: formData,
      });
    } else {
      result = await createMutation.mutateAsync(formData);
    }
    onSuccess(result);
  };

  return (
    <FormShell<ProductFormRequest>
      initial={initial}
      validate={validate}
      onSubmit={onSubmit}
      onCancel={onCancel}
      loading={loading}
      submitText={submitText}
      disableSubmit={hideSubmit}
      hideSubmit={hideSubmit}
      children={({ data: formData, updateField }) => {

        const handleSubcategoryToggle = useCallback((subcategoryId: number, checked: boolean) => {
          updateField("subcategoryIds", 
            checked
              ? [...formData.subcategoryIds, subcategoryId]
              : formData.subcategoryIds.filter((id) => id !== subcategoryId)
          );
        }, [updateField, formData.subcategoryIds]);

        return (
          <>
            <div className="grid grid-cols-2 gap-4">
              <div>
                <FormInput
                  id="name"
                  label="Name"
                  required
                  value={formData.name}
                  placeholder="Product Name"
                  onChange={(f, v) => updateField(f, v)}
                  disabled={isViewOnly}
                />
              </div>
              <div>
                <FormInput
                  id="slug"
                  label="Slug"
                  required
                  value={formData.slug}
                  placeholder="product-slug"
                  onChange={(f, v) => updateField(f, v)}
                  disabled={isViewOnly}
                />
              </div>
            </div>
            <div>
              <FormTextArea
                id="description"
                label="Description"
                value={formData.description || ""}
                placeholder="Description..."
                onChange={(f, v) => updateField(f, v || null)}
                disabled={isViewOnly}
              />
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div>
                <FormSelect<ProductType>
                  id="productTypeId"
                  label="Product Type"
                  required
                  value={formData.productTypeId}
                  type="number"
                  disabled={isViewOnly}
                  onChange={(f, v) => updateField(f, v)}
                  options={productTypes || []}
                  getOptionValue={(pt) => pt.productTypeId}
                  getOptionLabel={(pt) => pt.typeName}
                  placeholder="Select Product Type..."
                />
              </div>
              <div>
                <FormSelect<PriceType>
                  id="priceTypeId"
                  label="Price Type"
                  required
                  value={formData.priceTypeId}
                  type="number"
                  disabled={isViewOnly}
                  onChange={(f, v) => updateField(f, v)}
                  options={priceTypes || []}
                  placeholder="Select Price Type..."
                  getOptionValue={(pt) => pt.priceTypeId}
                  getOptionLabel={(pt) => `${pt.priceTypeName} (${pt.icon})`}
                />
              </div>
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div>
                <FormInput
                  id="price"
                  label="Price"
                  required
                  type="number"
                  value={formData.price}
                  placeholder=""
                  onChange={(f, v) => updateField(f, v)}
                  disabled={isViewOnly}
                  min="0"
                  step={formData.priceTypeId === 1 ? "100" : "0.01"}
                />
              </div>
              <div>
                <FormInput
                  id="premiumPrice"
                  label="Premium Price"
                  required
                  type="number"
                  value={formData.premiumPrice}
                  placeholder=""
                  onChange={(f, v) => updateField(f, v)}
                  disabled={isViewOnly}
                  min="0"
                  step={formData.priceTypeId === 1 ? "100" : "0.01"}
                />
              </div>
            </div>
            <div>
              <FormChipInput
                id="tags"
                label="Search Tags"
                value={formData.tags}
                onChange={updateField}
                suggestions={tags?.map(t => t.name) || []}
                placeholder="Type tags like 'dog', 'furniture', or 'fruit'..."
                helperText="Press enter or space to add multiple tags (new or existing) to help users find your product."
                maxItems={10}
                disabled={isViewOnly}
              />
            </div>

            <OverlappingLabelBox label="Attributes" columns={2}>
              <FormCheckbox
                id="isNew"
                label="New"
                checked={formData.isNew}
                onChange={(f, v) => updateField(f, v)}
                disabled={isViewOnly}
              />
              <FormCheckbox
                id="isTradeable"
                label="Tradeable"
                checked={formData.isTradeable}
                onChange={(f, v) => updateField(f, v)}
                disabled={isViewOnly}
              />
              <FormCheckbox
                id="isExclusive"
                label="Exclusive"
                checked={formData.isExclusive}
                onChange={(f, v) => updateField(f, v)}
                disabled={isViewOnly}
              />
              <FormCheckbox
                id="isPromotional"
                label="Promotional"
                checked={formData.isPromotional}
                onChange={(f, v) => updateField(f, v)}
                disabled={isViewOnly}
              />
            </OverlappingLabelBox>

            <OverlappingLabelBox label="Categories" required columns={3}>
              {categories?.map((category) => (
                <div key={category.categoryId}>
                  <h5 className="font-semibold text-gray-700 mb-2">{category.name}</h5>
                  <div className="space-y-2 pl-2">
                    {category.subcategories.map((subcategory) => (
                      <FormCheckbox
                        id={`subcategory_${subcategory.subcategoryId.toString()}`}
                        label={subcategory.name}
                        key={subcategory.subcategoryId}
                        checked={formData.subcategoryIds.includes(subcategory.subcategoryId)}
                        onChange={(_, value) => handleSubcategoryToggle(subcategory.subcategoryId, value)}
                        disabled={isViewOnly}
                      />
                    ))}
                  </div>
                </div>
              ))}
            </OverlappingLabelBox>
          </>
        );
      }}
    />
  );
};
