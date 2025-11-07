import { describe, it, expect, vi, beforeEach } from 'vitest';
import { fetchers } from '../../fetchers';
import {
  getProductById,
  getProductBySlug,
  getProducts,
  getProductsByCategory,
  getProductsBySubcategory,
  createProduct,
  updateProduct,
  deleteProduct,
} from '../index';
import type { ProductDetail, ProductFormRequest, Product } from '../../../types/product';
import type { PaginatedResponse, ProductFilterParams } from '../../../types/pagination';
import { mockPaginatedResponse, mockProduct, mockProductDetail, mockProductFormRequest } from '../../../tests/fixtures/product-fixtures';

vi.mock('../../fetchers', () => ({
  fetchers: {
    get: vi.fn(),
    post: vi.fn(),
    put: vi.fn(),
    delete: vi.fn(),
  },
}));


describe('Product Service', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('getProductById', () => {
    it('calls fetchers.get with correct URL and returns product', async () => {
      vi.mocked(fetchers.get).mockResolvedValue(mockProductDetail);

      const result = await getProductById(1);

      expect(fetchers.get).toHaveBeenCalledWith('/products/1', { signal: undefined });
      expect(result).toEqual(mockProductDetail);
    });

    it('passes AbortSignal to fetchers.get', async () => {
      vi.mocked(fetchers.get).mockResolvedValue(mockProductDetail);

      const abortController = new AbortController();
      await getProductById(1, abortController.signal);

      expect(fetchers.get).toHaveBeenCalledWith('/products/1', { signal: abortController.signal });
    });
  });

  describe('getProductBySlug', () => {
    it('calls fetchers.get with correct URL and returns product', async () => {
      vi.mocked(fetchers.get).mockResolvedValue(mockProductDetail);

      const result = await getProductBySlug('test-product');

      expect(fetchers.get).toHaveBeenCalledWith('/products/slug/test-product', { signal: undefined });
      expect(result).toEqual(mockProductDetail);
    });

    it('passes AbortSignal to fetchers.get', async () => {
      vi.mocked(fetchers.get).mockResolvedValue(mockProductDetail);

      const abortController = new AbortController();
      await getProductBySlug('test-product', abortController.signal);

      expect(fetchers.get).toHaveBeenCalledWith('/products/slug/test-product', { signal: abortController.signal });
    });
  });

  describe('getProducts', () => {
    it('calls fetchers.get with correct URL and params', async () => {
      vi.mocked(fetchers.get).mockResolvedValue(mockPaginatedResponse);

      const params: ProductFilterParams = {
        page: 1,
        pageSize: 12,
        search: 'test',
      };

      const result = await getProducts(params);

      expect(fetchers.get).toHaveBeenCalledWith('/products', { params, signal: undefined });
      expect(result).toEqual(mockPaginatedResponse);
    });

    it('passes AbortSignal to fetchers.get', async () => {
      vi.mocked(fetchers.get).mockResolvedValue(mockPaginatedResponse);

      const params: ProductFilterParams = { page: 1, pageSize: 12 };
      const abortController = new AbortController();

      await getProducts(params, abortController.signal);

      expect(fetchers.get).toHaveBeenCalledWith('/products', { params, signal: abortController.signal });
    });
  });

  describe('getProductsByCategory', () => {
    it('calls fetchers.get with correct URL and params', async () => {
      vi.mocked(fetchers.get).mockResolvedValue(mockPaginatedResponse);

      await getProductsByCategory('dogs', 1, 12);

      expect(fetchers.get).toHaveBeenCalledWith('/products/category/dogs', {
        params: { page: 1, pageSize: 12 },
        signal: undefined,
      });
    });

    it('passes AbortSignal to fetchers.get', async () => {
      vi.mocked(fetchers.get).mockResolvedValue(mockPaginatedResponse);

      const abortController = new AbortController();
      await getProductsByCategory('dogs', 1, 12, abortController.signal);

      expect(fetchers.get).toHaveBeenCalledWith('/products/category/dogs', {
        params: { page: 1, pageSize: 12 },
        signal: abortController.signal,
      });
    });
  });

  describe('getProductsBySubcategory', () => {
    it('calls fetchers.get with correct URL and params', async () => {
      vi.mocked(fetchers.get).mockResolvedValue(mockPaginatedResponse);

      await getProductsBySubcategory('dogs', 2, 24);

      expect(fetchers.get).toHaveBeenCalledWith('/products/subcategory/dogs', {
        params: { page: 2, pageSize: 24 },
        signal: undefined,
      });
    });

    it('passes AbortSignal to fetchers.get', async () => {
      vi.mocked(fetchers.get).mockResolvedValue(mockPaginatedResponse);

      const abortController = new AbortController();
      await getProductsBySubcategory('dogs', 2, 24, abortController.signal);

      expect(fetchers.get).toHaveBeenCalledWith('/products/subcategory/dogs', {
        params: { page: 2, pageSize: 24 },
        signal: abortController.signal,
      });
    });
  });

  describe('createProduct', () => {
    it('calls fetchers.post with correct URL and data', async () => {
      vi.mocked(fetchers.post).mockResolvedValue(mockProductDetail);

      const result = await createProduct(mockProductFormRequest);

      expect(fetchers.post).toHaveBeenCalledWith('/products', mockProductFormRequest);
      expect(result).toEqual(mockProductDetail);
    });
  });

  describe('updateProduct', () => {
    it('calls fetchers.put with correct URL and data', async () => {
      vi.mocked(fetchers.put).mockResolvedValue(mockProductDetail);

      const result = await updateProduct(1, mockProductFormRequest);

      expect(fetchers.put).toHaveBeenCalledWith('/products/1', mockProductFormRequest);
      expect(result).toEqual(mockProductDetail);
    });
  });

  describe('deleteProduct', () => {
    it('calls fetchers.delete with correct URL', async () => {
      vi.mocked(fetchers.delete).mockResolvedValue(undefined);
      await deleteProduct(1);
      expect(fetchers.delete).toHaveBeenCalledWith('/products/1');
    });
  });
});