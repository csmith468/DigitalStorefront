import { Link } from "react-router-dom";
import type { Product } from "../../types/product";

function ProductCard({ product }: { product: Product }) {
  const formatPrice = (price: number) => {
    return price.toLocaleString();
  }

  const handleAddToCart = (productId: number) => {
    console.log(`Add product ${productId} to cart`);
  }

  return (
   <li className="product-card">
      <div className="product-container">
        <div className="product-image"> {product.productId}
          {product.primaryImage ? (
            <img 
              src={product.primaryImage.imageUrl} 
              alt={product.primaryImage.altText ||  product.name}
              width={120} height={120}
            />
          ) : (
            <div className="placeholder-image">No Image</div>
          )}

          <Link className="product-image-overlay" title="More Info" to={`/product/${product.slug}`}>
            <span className="sr-only">View {product.name}</span>
          </Link>
        </div>

        <div className="product-name">
          <Link to={`/product/${product.slug}`}>{product.name}</Link>
        </div>

        <div className="product-badges">
          {product.isNew && (
            <span className="badge badge-new">NEW</span>
          )}
          {product.isTradeable && (
            <span className="badge badge-tradeable">TRADEABLE</span>
          )}
          {product.isPromotional && (
            <span className="badge badge-promo">PROMO</span>
          )}
        </div>

        <div className="product-price">
          <div className="price-row">
            <span className="price-label">Price:</span>
            <span className="price-value coin">{formatPrice(product.price)}</span>
          </div>
          <div className="price-row">
            <span className="price-label">Premium:</span>
            <span className="price-value coin">{formatPrice(product.premiumPrice)}</span>
          </div>
        </div>

        {/* Action Buttons */}
        <div className="product-actions">
          <Link to={`/product/${product.slug}`} className="btn btn-secondary">More Info</Link>
          <button onClick={() => handleAddToCart(product.productId)} className="btn btn-primary">Add To Cart</button>
        </div>
      </div>
    </li>
  );
}

export default ProductCard;