import { useParams, useNavigate, useSearchParams } from 'react-router-dom';
import { useProduct } from '../../hooks/queries/useProducts';
import { ProductForm } from '../../components/admin/ProductForm';
import { LoadingScreen } from '../../components/primitives/LoadingScreen';
import { useEffect, useState } from 'react';
import { ProductImageManager } from '../../components/admin/ProductImageManager';
import { PageHeader } from '../../components/primitives/PageHeader';

type TabType = 'details' | 'images';

export function EditProductPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [searchParams, setSearchParams] = useSearchParams();
  const productId = Number(id);

  const initialTab = (searchParams.get('tab') === 'images' ? 'images' : 'details') as TabType;
  const [activeTab, setActiveTab] = useState<TabType>(initialTab);

  useEffect(() => {
    const tabParam = searchParams.get('tab');
    if (tabParam === 'images' || tabParam === 'details')
      setActiveTab(tabParam as TabType);
  }, [searchParams]);

  const { data: product, isLoading, error, refetch } = useProduct(productId);

  const handleTabChange = (tab: TabType) => {
    setActiveTab(tab);
    setSearchParams({ tab });
  };

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

  const tabs = [
    { id: 'details' as TabType, label: 'Product Details' },
    { id: 'images' as TabType, label: 'Images', badge: product.images?.length || 0 },
  ]

  return (
    <div className="container mx-auto px-4 py-8 max-w-4xl">
      <PageHeader 
        title={`Edit Product: ${product.name}`}
        returnLink='/admin/products'
        returnText='Back to Products'
      />

      <div className="mb-6">
        <nav className="flex gap-1 border-b-2 border-gray-200">
          {tabs.map((tab) => (
            <button
              key={tab.id}
              onClick={() => handleTabChange(tab.id)}
              className={`px-6 py-3 font-medium text-sm rounded-t-lg transition-all flex items-center gap-2 relative ${
                activeTab === tab.id
                  ? 'bg-gradient-to-br from-[var(--color-primary)] to-[var(--color-accent)] text-white shadow-lg -mb-0.5 border-b-2 border-transparent'
                  : 'bg-gray-200 text-gray-700 hover:bg-gray-300 hover:text-gray-900 border border-gray-300 border-b-0'
              }`}
            >
              <span>{tab.label}</span>
              {tab.badge !== undefined && (
                <span className={`px-2 py-0.5 rounded-full text-xs font-semibold ${
                  activeTab === tab.id 
                    ? 'bg-white/20 text-white border border-white/30' 
                    : 'bg-gray-400 text-gray-800'
                }`}>
                  {tab.badge}
                </span>
              )}
            </button>
          ))}
        </nav>
      </div>

      <div className="bg-white rounded-lg shadow p-6">
        {activeTab === 'details' ? (
          <ProductForm
            existingProduct={product}
            onSuccess={() => navigate('/admin/products')}
            onCancel={() => navigate('/admin/products')}
          />
        ) : (
          <ProductImageManager
            productId={productId}
            images={product.images || []}
            onImagesChange={() => refetch()}
          />
        )}
      </div>
    </div>
    );
  }