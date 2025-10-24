import ProductCard from './ProductCard';
import type { Product } from '../../types/product';

interface ProductsGridProps {
  products: Product[];
  view?: 'grid' | 'list';
}

export default function ProductsGrid({ products }: ProductsGridProps) {
  if (!products || products.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center py-16 px-4">
        <h3 className="text-xl font-semibold text-text-primary mb-2">No Products Found</h3>
        <p className="text-text-secondary">Please check back later or adjust your search criteria.</p>
      </div>
    );
  }

  return (
     <ul className='grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-2'>
      {products.map((product) => (
        <ProductCard key={product.productId} product={product} />
      ))}
    </ul>
  );
}

