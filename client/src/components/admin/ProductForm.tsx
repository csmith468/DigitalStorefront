import type { ProductDetail, ProductFormRequest } from "../../types/product";
import { useCreateProduct, useUpdateProduct } from "../../hooks/useProducts";
import { useCategories } from "../../hooks/useCategories";
import { useProductTypes, usePriceTypes } from "../../hooks/useCommon";
import { FormInput } from "../primitives/FormInput";
import { FormTextArea } from "../primitives/FormTextArea";
import { FormSelect } from "../primitives/FormSelect";
import { FormCheckbox } from "../primitives/FormCheckbox";
import { FormShell } from "../primitives/FormShell";
import { OverlappingLabelBox } from "../primitives/OverlappingLabelBox";

interface ProductFormProps {
  existingProduct?: ProductDetail; // if exists, edit mode
  onSuccess: () => void;
  onCancel: () => void;
}

export const ProductForm: React.FC<ProductFormProps> = ({
  existingProduct,
  onSuccess,
  onCancel,
}) => {
  const isEditing = !!existingProduct;

  // State (equivalent to data())
  const initial: ProductFormRequest = {
    name: existingProduct?.name || "",
    slug: existingProduct?.slug || "",
    productTypeId: existingProduct?.productTypeId || 0,
    description: existingProduct?.description || null,
    isTradeable: existingProduct?.isTradeable || false,
    isNew: existingProduct?.isNew || false,
    isPromotional: existingProduct?.isPromotional || false,
    isExclusive: existingProduct?.isExclusive || false,
    price: existingProduct?.price || 0,
    premiumPrice: existingProduct?.premiumPrice || 0,
    priceTypeId: existingProduct?.priceTypeId || 0,
    subcategoryIds:
      existingProduct?.subcategories?.map((s) => s.subcategoryId) || [],
  };

  // Hooks (queries and mutations)
  const { data: categories } = useCategories();
  const { data: productTypes } = useProductTypes();
  const { data: priceTypes } = usePriceTypes();

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
    if (formData.priceTypeId === 2 && (!Number.isInteger(formData.premiumPrice) || !Number.isInteger(formData.price))) {
      return "Coin prices must be integers";
    }
    return null;
  };

  const onSubmit = async (formData: ProductFormRequest) => {
    if (isEditing) {
      await updateMutation.mutateAsync({
        productId: existingProduct.productId,
        product: formData,
      });
    } else {
      await createMutation.mutateAsync(formData);
    }
    onSuccess();
  };

  return (
    <FormShell<ProductFormRequest>
      initial={initial}
      validate={validate}
      onSubmit={onSubmit}
      onCancel={onCancel}
      loading={loading}
      submitText={submitText}
      children={({ data: formData, updateField }) => {
        const handleSubcategoryToggle = (subcategoryId: number, checked: boolean) => {
          updateField("subcategoryIds", 
            checked
              ? [...formData.subcategoryIds, subcategoryId]
              : formData.subcategoryIds.filter((id) => id !== subcategoryId)
          );
        };

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
                  onChange={(field, value) => updateField(field, value)}
                />
              </div>
              <div>
                <FormInput
                  id="slug"
                  label="Slug"
                  required
                  value={formData.slug}
                  placeholder="product-slug"
                  onChange={(field, value) => updateField(field, value)}
                />
              </div>
            </div>
            <div>
              <FormTextArea
                id="description"
                label="Description"
                value={formData.description || ""}
                placeholder="Description..."
                onChange={(field, value) => updateField(field, value || null)}
              />
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div>
                <FormSelect
                  id="productTypeId"
                  label="Product Type"
                  required
                  value={formData.productTypeId}
                  type="number"
                  onChange={(field, value) => updateField(field, value)}
                  options={productTypes || []}
                  getOptionValue={(pt) => pt.productTypeId}
                  getOptionLabel={(pt) => pt.typeName}
                  placeholder="Select Product Type..."
                />
              </div>
              <div>
                <FormSelect
                  id="priceTypeId"
                  label="Price Type"
                  required
                  value={formData.priceTypeId}
                  type="number"
                  onChange={(field, value) => updateField(field, value)}
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
                  onChange={(field, value) => updateField(field, value)}
                  min="0"
                  step={formData.priceTypeId === 1 ? "500" : "0.01"}
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
                  onChange={(field, value) => updateField(field, value)}
                  min="0"
                  step={formData.priceTypeId === 1 ? "500" : "0.01"}
                />
              </div>
            </div>

            <OverlappingLabelBox label="Tags" required columns={2}>
              <FormCheckbox
                id="isNew"
                label="New"
                checked={formData.isNew}
                onChange={(field, value) => updateField(field, value)}
              />
              <FormCheckbox
                id="isTradeable"
                label="Tradeable"
                checked={formData.isTradeable}
                onChange={(field, value) => updateField(field, value)}
              />
              <FormCheckbox
                id="isExclusive"
                label="Exclusive"
                checked={formData.isExclusive}
                onChange={(field, value) => updateField(field, value)}
              />
              <FormCheckbox
                id="isPromotional"
                label="Promotional"
                checked={formData.isPromotional}
                onChange={(field, value) => updateField(field, value)}
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
