import { useParams, Link } from 'react-router-dom';
import { useState } from 'react';
import { useProductBySlug } from '../hooks/queries/useProducts';
import { LoadingScreen } from '../components/primitives/LoadingScreen';
import { ChevronLeftIcon, ChevronRightIcon } from '@heroicons/react/24/outline';
import toast from 'react-hot-toast';
import { SuccessMessages } from '../constants/messages';

export function ProductDetailPage() {
  const { slug } = useParams<{ slug: string }>();
  const [currentImageIndex, setCurrentImageIndex] = useState(0);

  const { data: product, isLoading, error } = useProductBySlug(slug as string);

  const handleAddToCart = () => {
    toast.success(SuccessMessages.Product.addedToCart);
  };

  const handlePromotional = () => {
    toast.success('Promotional items are display only!');
  };

  if (isLoading) {
    return <LoadingScreen message="Loading product..." />;
  }

  if (error || !product) {
    return (
      <div className="container mx-auto px-4 py-8">
        <div className="text-center">
          <p className="text-lg font-semibold text-red-600 mb-2">Product Not Found</p>
          <Link to="/" className="text-primary hover:underline">Return to Home</Link>
        </div>
      </div>
    );
  }

  const images = product.images && product.images.length > 0
    ? product.images
    : product.primaryImage
      ? [product.primaryImage]
      : [];

  const currentImage = images[currentImageIndex];

  const nextImage = () => {
    setCurrentImageIndex((prev) => (prev + 1) % images.length);
  };

  const prevImage = () => {
    setCurrentImageIndex((prev) => (prev - 1 + images.length) % images.length);
  };

  const baseBadge = "px-3 py-1 rounded-full text-sm font-semibold";

  return (
    <div className="container mx-auto px-4 py-8 max-w-6xl">
      <nav className="mb-6 text-sm text-text-secondary">
        <Link to="/" className="hover:text-primary">Home</Link>
        <span className="mx-2">/</span>
        <span className="text-text-primary">{product.name}</span>
      </nav>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
        <div className="space-y-4">
          <div className="relative aspect-square bg-gray-100 rounded-lg overflow-hidden border-2 border-gray-200">
            {currentImage ? (
              <img
                src={currentImage.imageUrl}
                alt={currentImage.altText || product.name}
                className="w-full h-full object-cover"
              />
            ) : (
              <div className="w-full h-full flex items-center justify-center text-gray-400">
                No Image Available
              </div>
            )}

            {images.length > 1 && (
              <>
                <button
                  onClick={prevImage}
                  className="absolute left-2 top-1/2 -translate-y-1/2 p-2 bg-white/80 rounded-full hover:bg-white transition-colors"
                  aria-label="Previous image"
                >
                  <ChevronLeftIcon className="h-6 w-6" />
                </button>
                <button
                  onClick={nextImage}
                  className="absolute right-2 top-1/2 -translate-y-1/2 p-2 bg-white/80 rounded-full hover:bg-white transition-colors"
                  aria-label="Next image"
                >
                  <ChevronRightIcon className="h-6 w-6" />
                </button>
                <div className="absolute bottom-4 left-1/2 -translate-x-1/2 text-white bg-black/50 px-3 py-1 rounded-full text-sm">
                  {currentImageIndex + 1} / {images.length}
                </div>
              </>
            )}

            {product.isTradeable && (
              <span className="absolute top-4 right-4 px-3 py-1 bg-[var(--color-info)] text-white text-sm font-semibold rounded-md shadow-md">
                TRADEABLE
              </span>
            )}
          </div>

          {images.length > 1 && (
            <div className="flex gap-2 overflow-x-auto">
              {images.map((image, index) => (
                <button
                  key={image.productImageId}
                  onClick={() => setCurrentImageIndex(index)}
                  className={`flex-shrink-0 w-20 h-20 rounded-md overflow-hidden border-2 transition-colors ${
                    index === currentImageIndex ? 'border-primary' : 'border-gray-200 hover:border-gray-300'
                  }`}
                >
                  <img
                    src={image.imageUrl}
                    alt={image.altText || `${product.name} ${index + 1}`}
                    className="w-full h-full object-cover"
                  />
                </button>
              ))}
            </div>
          )}
        </div>

        <div className="space-y-6">
          <div>
            <h1 className="text-3xl font-bold text-text-primary mb-2">
              {product.name}
            </h1>

            <div className="flex flex-wrap gap-2 mb-4">
              {product.isNew && (
                <span className={`${baseBadge} bg-[var(--color-success)] text-white`}>
                  NEW
                </span>
              )}
              {product.isExclusive && (
                <span className={`${baseBadge} bg-[var(--color-secondary)] text-white`}>
                  Exclusive
                </span>
              )}
              {product.isTradeable && (
                <span className={`${baseBadge} bg-[var(--color-info)] text-white`}>
                  Tradeable
                </span>
              )}
            </div>
          </div>

          {product.description && (
            <div>
              <h2 className="text-lg font-semibold text-text-primary mb-2">
                Description
              </h2>
              <p className="text-text-secondary leading-relaxed">
                {product.description}
              </p>
            </div>
          )}

          <div className="bg-gray-50 rounded-lg p-6 space-y-3">
            <div className="flex justify-between items-center">
              <span className="text-text-secondary">Regular Price:</span>
              <span className="text-2xl font-bold text-[var(--color-info)]">
                {product.priceIcon} {product.price.toLocaleString()}
              </span>
            </div>
            <div className="flex justify-between items-center">
              <span className="text-text-secondary">Premium Price:</span>
              <span className="text-2xl font-bold text-[var(--color-link-hover)]">
                {product.priceIcon} {product.premiumPrice.toLocaleString()}
              </span>
            </div>
          </div>

          {product.isPromotional ? (
            <button
              onClick={handlePromotional}
              className="w-full px-6 py-4 text-lg bg-[var(--color-accent)] text-white rounded-lg hover:opacity-90 transition-opacity font-semibold"
            >Promotional Item
            </button>
          ) : (
            <button
              onClick={handleAddToCart}
              className="w-full px-6 py-4 text-lg bg-[var(--color-primary)] text-white rounded-lg hover:opacity-90 transition-opacity font-semibold"
            >Add To Cart
            </button>
          )}

          <div className="text-sm text-text-secondary space-y-1">
            <p>SKU: {product.sku}</p>
            {product.subcategories && product.subcategories.length > 0 && (
              <p>Categories: {product.subcategories.map(s => s.name).join(', ')}</p>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}