import { useParams, useNavigate } from 'react-router-dom';
import { useProduct } from '../../hooks/queries/useProducts';
import { LoadingScreen } from '../../components/primitives/LoadingScreen';
import { ProductFormPageShared } from '../../components/admin/ProductFormPageShared';

export function ViewProductFormPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { data: product, isLoading } = useProduct(Number(id));

  if (isLoading) return <LoadingScreen message="Loading product..." />;
  if (!product) return <div>Product not found</div>;

  return (
    <ProductFormPageShared
      product={product}
      mode="view"
      title={`View Product: ${product.name}`}
      onSuccess={() => {}}
      onCancel={() => navigate('/admin')}
      onImagesChange={() => {}}
    />
  );
}