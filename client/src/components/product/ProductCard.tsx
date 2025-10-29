import { Link } from "react-router-dom";
import type { Product } from "../../types/product";
import { logger } from "../../utils/logger";
import toast from "react-hot-toast";

function ProductCard({ product }: { product: Product }) {
  const formatPrice = (price: number, priceType: string) => {
    return priceType + price.toLocaleString();
  };

  const handleAddToCart = (productId: number) => {
    logger.info(`Add product ${productId} to cart`);
    toast.success('Product added to cart!');
  };

  const baseBadge = "px-2 py-0.5 rounded-full text-xs font-semibold";

  const badgeStyles = {
    new: `${baseBadge} bg-[var(--color-success)] text-white`,
    exclusive: `${baseBadge} bg-[var(--color-secondary)] text-white`,
    tradeable: `${baseBadge} bg-[var(--color-danger)] text-white`,
  };

  return (
    <li className="bg-white rounded-lg shadow-lg hover:shadow-2xl transition-[transform,shadow] duration-300 hover:scale-[1.02] flex flex-col h-full overflow-hidden">
      <div className="p-4 flex flex-col flex-grow">
        <div className="relative w-full aspect-square border-2 border-border rounded-md overflow-hidden mb-3">
          {product.primaryImage ? (
            <img
              src={product.primaryImage.imageUrl}
              alt={product.primaryImage.altText || product.name}
              className="w-full h-full object-cover"
            />
          ) : (
            <div className="w-full h-full bg-border flex items-center justify-center text-text-secondary">
              No Image
            </div>
          )}

          {product.isTradeable && (
            <span className="absolute top-2 right-2 px-2 py-1 bg-[var(--color-info)] text-white text-xs font-semibold rounded-md shadow-md z-20">
              TRADEABLE
            </span>
          )}

          <Link
            className="absolute inset-0 z-10"
            title="More Info"
            to={`/product/${product.slug}`}>
            <span className="sr-only">View {product.name}</span>
          </Link>
        </div>

        <div className="mb-2 text-center">
          <Link
            to={`/product/${product.slug}`}
            className="text-base font-semibold text-text-primary hover:text-[var(--color-link-hover)] transition-colors">
            {product.name}
          </Link>
        </div>

        <div className="flex-grow"></div>

        <div className="flex flex-wrap gap-1 mb-3 justify-center">
          {product.isNew && <span className={badgeStyles.new}>NEW</span>}
          {product.isExclusive && (
            <span className={badgeStyles.exclusive}>Exclusive</span>
          )}
        </div>

        <div className="flex-grow"></div>

        <div className="space-y-1 mb-3">
          <div className="flex justify-between items-center gap-2">
            <span className="text-sm text-text-secondary flex-shrink-0">Price:</span>
            <span className="text-sm font-semibold text-[var(--color-info)] text-right">
              {formatPrice(product.price, product.priceIcon)}
            </span>
          </div>
          <div className="flex justify-between items-center gap-2">
            <span className="text-sm text-text-secondary flex-shrink-0">Premium:</span>
            <span className="text-sm font-semibold text-[var(--color-link-hover)] text-right">
              {formatPrice(product.premiumPrice, product.priceIcon)}
            </span>
          </div>
        </div>

        {product.isPromotional ? (
          <Link
            to={`/product/${product.slug}`}
            className="w-full px-4 py-2 text-sm bg-[var(--color-accent)] text-white rounded-md hover:opacity-90 transition-opacity font-medium text-center block no-underline">
            Promotional
          </Link>
        ) : (
          <button
            onClick={() => handleAddToCart(product.productId)}
            className="w-full px-4 py-2 text-sm bg-[var(--color-primary)] text-white rounded-md hover:opacity-90 transition-opacity font-medium">
            Add To Cart
          </button>
        )}
      </div>
    </li>
  );
}

export default ProductCard;
