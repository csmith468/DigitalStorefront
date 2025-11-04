import { describe, it, expect, vi, beforeEach } from 'vitest';
import { fetchers } from '../fetchers';
import { getCategories, getTags, getProductTypes, getPriceTypes } from '../metadata';
import { mockCategories, mockTags, mockProductTypes, mockPriceTypes } from '../../tests/fixtures/metadata-fixtures';

vi.mock('../fetchers', () => ({
  fetchers: {
    get: vi.fn(),
    post: vi.fn(),
    put: vi.fn(),
    delete: vi.fn(),
  },
}));

describe('Metadata Service', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('getCategories', () => {
    it('calls fetchers.get with correct URL and returns categories', async () => {
      vi.mocked(fetchers.get).mockResolvedValue(mockCategories);

      const result = await getCategories();

      expect(fetchers.get).toHaveBeenCalledWith('/metadata/categories', { signal: undefined });
      expect(result).toEqual(mockCategories);
    });

    it('passes AbortSignal to fetchers.get', async () => {
      vi.mocked(fetchers.get).mockResolvedValue(mockCategories);

      const abortController = new AbortController();
      await getCategories(abortController.signal);

      expect(fetchers.get).toHaveBeenCalledWith('/metadata/categories', { signal: abortController.signal });
    });
  });

  describe('getTags', () => {
    it('calls fetchers.get with correct URL and returns tags', async () => {
      vi.mocked(fetchers.get).mockResolvedValue(mockTags);

      const result = await getTags();

      expect(fetchers.get).toHaveBeenCalledWith('/metadata/tags', { signal: undefined });
      expect(result).toEqual(mockTags);
    });

    it('passes AbortSignal to fetchers.get', async () => {
      vi.mocked(fetchers.get).mockResolvedValue(mockTags);

      const abortController = new AbortController();
      await getTags(abortController.signal);

      expect(fetchers.get).toHaveBeenCalledWith('/metadata/tags', { signal: abortController.signal });
    });
  });

  describe('getProductTypes', () => {
    it('calls fetchers.get with correct URL and returns product types', async () => {
      vi.mocked(fetchers.get).mockResolvedValue(mockProductTypes);

      const result = await getProductTypes();

      expect(fetchers.get).toHaveBeenCalledWith('/metadata/product-types', { signal: undefined });
      expect(result).toEqual(mockProductTypes);
    });

    it('passes AbortSignal to fetchers.get', async () => {
      vi.mocked(fetchers.get).mockResolvedValue(mockProductTypes);

      const abortController = new AbortController();
      await getProductTypes(abortController.signal);

      expect(fetchers.get).toHaveBeenCalledWith('/metadata/product-types', { signal: abortController.signal });
    });
  });

  describe('getPriceTypes', () => {
    it('calls fetchers.get with correct URL and returns price types', async () => {
      vi.mocked(fetchers.get).mockResolvedValue(mockPriceTypes);

      const result = await getPriceTypes();

      expect(fetchers.get).toHaveBeenCalledWith('/metadata/price-types', { signal: undefined });
      expect(result).toEqual(mockPriceTypes);
    });

    it('passes AbortSignal to fetchers.get', async () => {
      vi.mocked(fetchers.get).mockResolvedValue(mockPriceTypes);

      const abortController = new AbortController();
      await getPriceTypes(abortController.signal);

      expect(fetchers.get).toHaveBeenCalledWith('/metadata/price-types', { signal: abortController.signal });
    });
  });
})