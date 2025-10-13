import ProductCard from './ProductCard';
import type { Product } from '../../types/product';

interface ProductGridProps {
  products: Product[];
  view?: 'grid' | 'list';
}

function ProductGrid({ products, view = 'grid' }: ProductGridProps) {
  if (!products || products.length === 0) 
    return <div className="empty-state">No products found.</div>;

  return (
    <ul className={`product-grid ${view == 'list' ? 'list-view' : 'grid-view'}`}>
      {products.map((product) => (
        <ProductCard key={product.productId} product={product} />
      ))}
    </ul>
  );
}

export default ProductGrid;