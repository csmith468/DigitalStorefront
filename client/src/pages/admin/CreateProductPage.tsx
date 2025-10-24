import { useNavigate } from 'react-router-dom';
import { ProductForm } from '../../components/admin/ProductForm';

export function CreateProductPage() {
  const navigate = useNavigate();

  return (
    <div className="container mx-auto px-4 py-8 max-w-4xl">
      <div className="mb-8">
        <h1 className="text-4xl font-bold text-text-primary">Create New Product</h1>
        <div className="w-24 h-1 rounded-full mt-2" 
          style={{ background: `linear-gradient(90deg, var(--color-primary) 0%, var(--color-accent) 100%)` }}
        ></div>
      </div>

      <div className="bg-white rounded-lg shadow p-6">
        <ProductForm
          onSuccess={() => navigate('/admin/products')}
          onCancel={() => navigate('/admin/products')}
        />
      </div>
    </div>
  );
}