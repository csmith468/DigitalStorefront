import { useState, useEffect } from "react";
import { useParams } from "react-router-dom";
import ProductGrid from "../components/product/ProductGrid";
import { productsService } from "../services/products";
import type { Product } from "../types/product";
import { isViewAllSubcategory } from "../types/subcategory";

function ProductsView() {
  const { categorySlug, subcategorySlug } = useParams();
  const [products, setProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchProducts = async () => {
      try {
        setLoading(true);
        setError(null);
        console.log('here')

        let data: Product[] = [];
        
        if (categorySlug && subcategorySlug && isViewAllSubcategory(subcategorySlug))
          data = await productsService.getProductsByCategory(categorySlug);
        else if (subcategorySlug)
          data = await productsService.getProductsBySubcategory(subcategorySlug) 

        console.log('Fetched products:', data);
        setProducts(data);
      } catch (err) {
        console.error("Failed to fetch products:", err);
        setError("Failed to load products. Please try again later.");
      } finally {
        setLoading(false);
      }
    }

    fetchProducts();
  }, [categorySlug, subcategorySlug]);

  if (loading) {
    return (
      <div className="container mx-auto px-4 py-8">
        <div className="flex justify-center items-center min-h-[400px]">
          <div className="text-lg text-text-secondary">Loading products...</div>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="container mx-auto px-4 py-8">
        <div className="flex justify-center items-center min-h-[400px]">
          <div className="text-danger text-center">
            <p className="text-lg font-semibold mb-2">Error</p>
            <p>{error}</p>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="container mx-auto px-4 py-8">
      <div className="mb-8">
        <h1 className="text-4xl font-bold mb-2 text-text-primary">
          Products
        </h1>
        <div className="w-24 h-1 rounded-full" style={{ background: `linear-gradient(90deg, var(--color-primary) 0%, var(--color-accent) 100%)` }}></div>
      </div>
      <ProductGrid products={products} />
    </div>
  );
}

export default ProductsView;