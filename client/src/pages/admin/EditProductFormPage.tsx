import { useParams, useNavigate } from 'react-router-dom';
import { useProduct } from '../../hooks/queries/useProducts';
import { LoadingScreen } from '../../components/primitives/LoadingScreen';
import { ProductFormPageShared } from '../../components/admin/ProductFormPageShared';

export function EditProductFormPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { data: product, isLoading, refetch } = useProduct(Number(id));

  if (isLoading) return <LoadingScreen message="Loading product..." />;
  if (!product) return <div>Product not found</div>;

  return (
    <ProductFormPageShared
      product={product}
      mode="edit"
      title={`Edit Product: ${product.name}`}
      onSuccess={() => navigate('/admin/products')}
      onCancel={() => navigate('/admin/products')}
      onImagesChange={() => refetch()}
    />
  );
}