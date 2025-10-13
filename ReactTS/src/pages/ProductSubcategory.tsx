import { useState, useEffect } from "react";
import { useParams } from "react-router-dom";
import ProductGrid from "../components/product/ProductGrid";
import { productsService } from "../services/products";
import type { Product } from "../types/product";

function ProductSubcategory() {
  const { slug } = useParams();
  const [products, setProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchProducts = async () => {
      try {
        setLoading(true);
        setError(null);

        console.log('Fetching products for slug:', slug);
        const data = slug ? await productsService.getProductsBySubcategory(slug) : [];
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
  }, [slug]); // refetch if slug changes

  if (loading) {
    return <div className="loading-state">Loading products...</div>;
  }

  if (error) {
    return <div className="error-state">{error}</div>;
  }

  return (
    <div className="product-subcategory-page">
      <h1 className="page-title">Products</h1>
      <ProductGrid products={products} view="grid" />
    </div>
  );
}

export default ProductSubcategory;