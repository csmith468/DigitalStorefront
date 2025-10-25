import { useNavigate } from 'react-router-dom';
import { ProductForm } from '../../components/admin/ProductForm';
import { PageHeader } from '../../components/primitives/PageHeader';
import type { ProductDetail } from '../../types/product';

export function CreateProductPage() {
  const navigate = useNavigate();

  const handleSuccess = (product?: ProductDetail) => {
    console.log('handleSuccess called with:', product);
    if (product) {
      navigate(`/admin/products/${product.productId}/edit?tab=images`);
    } else {
      console.log('No product received, going to list');
      navigate('/admin/products');
    }
  };

  return (
    <div className="container mx-auto px-4 py-8 max-w-4xl">
      <PageHeader 
        title="Create New Product"
        returnLink='/admin/products'
        returnText='Back to Products'
      />

      <div className="bg-white rounded-lg shadow p-6">
        <ProductForm
          onSuccess={handleSuccess}
          onCancel={() => navigate('/admin/products')}
        />
      </div>
    </div>
  );
}