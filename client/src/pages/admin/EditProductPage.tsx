import { useParams, useNavigate } from 'react-router-dom';
import { useProduct } from '../../hooks/useProducts';
import { ProductForm } from '../../components/admin/ProductForm';
import { LoadingScreen } from '../../components/primitives/LoadingScreen';

export function EditProductPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const productId = Number(id);

  const { data: product, isLoading, error } = useProduct(productId);

  if (isLoading) {
    return <LoadingScreen message="Loading product..." />;
  }

  if (error || !product) {
    return (
      <div className="container mx-auto px-4 py-8">
        <div className="text-danger text-center">
          <p className="text-lg font-semibold mb-2">Error</p>
          <p>Failed to load product. Please try again.</p>
        </div>
      </div>
    );
  }

  return (
    <div className="container mx-auto px-4 py-8 max-w-4xl">
      <div className="mb-8">
        <h1 className="text-4xl font-bold text-text-primary">Edit Product: {product.name}</h1>
        <div className="w-24 h-1 rounded-full mt-2" 
          style={{ background: `linear-gradient(90deg, var(--color-primary) 0%, var(--color-accent) 100%)` }}
        ></div>
      </div>

      <div className="bg-white rounded-lg shadow p-6">
        <ProductForm
          existingProduct={product}
          onSuccess={() => navigate('/admin/products')}
          onCancel={() => navigate('/admin/products')}
        />
      </div>
    </div>
  );
}