import { useNavigate } from 'react-router-dom';
import { ProductFormPageShared } from '../../components/admin/ProductFormPageShared';
import type { ProductDetail } from '../../types/product';

const SAMPLE_PRODUCT: ProductDetail = {
  productId: 0,
  name: 'Sample Product',
  slug: 'sample-product',
  productTypeId: 1,
  isTradeable: false,
  isNew: true,
  isPromotional: false,
  isExclusive: false,
  isDemoProduct: false, // demo products are read-only so this is not technically a demo product
  sku: 'SAM-00000',
  price: 9.99,
  premiumPrice: 8.99,
  priceIcon: "$",
  primaryImage: null,
  description: 'Try editing me!',
  priceTypeId: 2,
  images: [],
  subcategories: [],
  tags: []
};

export function TryProductFormPage() {
  const navigate = useNavigate();

  return (
    <ProductFormPageShared
      product={SAMPLE_PRODUCT}
      mode="try"
      title="Try the Product Form"
      onSuccess={() => {}}
      onCancel={() => navigate('/admin/products')}
      onImagesChange={() => {}}
    />
  );
}