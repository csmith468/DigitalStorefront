import { useSearchParams } from 'react-router-dom';
import { useEffect, useState } from 'react';
import { ProductForm } from './ProductForm';
import { ProductImageManager } from './ProductImageManager';
import { PageHeader } from '../primitives/PageHeader';
import type { ProductDetail } from '../../types/product';
import type { ProductFormMode } from './ProductForm';
import { useUser } from '../../contexts/useUser';
import { SectionErrorBoundary } from '../common/SectionErrorBoundary';

type TabType = 'details' | 'images';

interface ProductFormPageSharedProps {
  product: ProductDetail;
  mode: ProductFormMode;
  title: string;
  onSuccess: () => void;
  onCancel: () => void;
  onImagesChange: () => void;
}

export function ProductFormPageShared({ 
  product, mode, title, onSuccess, onCancel, onImagesChange 
}: ProductFormPageSharedProps) {
  const [searchParams, setSearchParams] = useSearchParams();
  const initialTab = (searchParams.get('tab') === 'images' ? 'images' : 'details') as TabType;
  const [activeTab, setActiveTab] = useState<TabType>(initialTab);

  const { isLoggedIn } = useUser();

  useEffect(() => {
    const tabParam = searchParams.get('tab');
    if (tabParam === 'images' || tabParam === 'details')
      setActiveTab(tabParam as TabType);
  }, [searchParams]);

  const handleTabChange = (tab: TabType) => {
    setActiveTab(tab);
    setSearchParams({ tab });
  };

  const tabs = [
    { id: 'details' as TabType, label: 'Product Details' },
    { id: 'images' as TabType, label: 'Images', badge: product.images?.length || 0 },
  ]

  return (
    <div className="container mx-auto px-4 py-8 max-w-4xl">
      <PageHeader title={title} returnLink='/admin' returnText='Back to Products' />

      {mode === 'view' && (
        <div className="bg-blue-50 border border-blue-200 p-4 rounded-md mb-6">
          <p className="text-blue-800">
            <strong>Read-Only Demo:</strong> This shows what the product form looks like.
            {!isLoggedIn && (
              <>
                <br /><a href="/?auth=signup" className="text-blue-600 underline font-medium">
                  Create an Account
                </a> to add your own products!
              </>
            )}
          </p>
        </div>
      )}

      {mode === 'try' && (
        <div className="bg-green-50 border border-green-200 p-4 rounded-md mb-6">
          <p className="text-green-800">
            <strong>Try It Out!</strong> Test the form. Changes won't be saved.
            {!isLoggedIn && (
              <>
                <br /><a href="/?auth=signup" className="text-blue-600 underline font-medium">
                  Create an Account
                </a> to add your own products!
              </>
            )}
          </p>
        </div>
      )}
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
              }`}>
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
          <SectionErrorBoundary
            sectionName="Product Form"
            fallbackMessage="Failed to load the product form. Please try refreshing the page."
          >
            <ProductForm existingProduct={product} mode={mode} onSuccess={onSuccess} onCancel={onCancel} />
          </SectionErrorBoundary>
        ) : (
          <SectionErrorBoundary
            sectionName="Product Images"
            fallbackMessage="Failed to load the image manager. You can switch to the Details tab to edit product information."
          >
            <ProductImageManager
              productId={product.productId}
              images={product.images || []}
              onImagesChange={onImagesChange}
              isViewOnly={mode !== 'edit'}
            />
          </SectionErrorBoundary>
        )}
      </div>
    </div>
  );
}