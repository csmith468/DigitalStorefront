import { useNavigate } from 'react-router-dom';
import { ProductForm } from '../../components/admin/ProductForm';
import { PageHeader } from '../../components/primitives/PageHeader';

export function CreateProductPage() {
  const navigate = useNavigate();

  return (
    <div className="container mx-auto px-4 py-8 max-w-4xl">
      <PageHeader 
        title="Create New Product"
        returnLink='/admin/products'
        returnText='Back to Products'
      />

      <div className="bg-white rounded-lg shadow p-6">
        <ProductForm
          onSuccess={() => navigate('/admin/products')}
          onCancel={() => navigate('/admin/products')}
        />
      </div>
    </div>
  );
}