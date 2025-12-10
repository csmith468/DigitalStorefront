import { describe, it, expect, vi, beforeEach } from 'vitest';
import { fetchers } from '../fetchers';
import apiClient from '../apiClient';

vi.mock('../apiClient', () => ({
  default: {
    get: vi.fn(),
    post: vi.fn(),
    put: vi.fn(),
    delete: vi.fn(),
  },
}));

describe('fetchers', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('get', () => {
    it('calls apiClient.get with url and config', async () => {
      const mockData = { id: 1, name: 'Test' };
      vi.mocked(apiClient.get).mockResolvedValue({ data: mockData });

      const result = await fetchers.get('/test-url', { params: { foo: 'bar' } });

      expect(apiClient.get).toHaveBeenCalledWith('/test-url', { params: { foo: 'bar' } });
      expect(result).toEqual(mockData);
    });

    it('returns response.data', async () => {
      const mockData = [1, 2, 3];
      vi.mocked(apiClient.get).mockResolvedValue({ data: mockData });

      const result = await fetchers.get<number[]>('/numbers');

      expect(result).toEqual([1, 2, 3]);
    });

    it('works without config parameter', async () => {
      vi.mocked(apiClient.get).mockResolvedValue({ data: 'success' });

      const result = await fetchers.get('/simple');

      expect(apiClient.get).toHaveBeenCalledWith('/simple', undefined);
      expect(result).toBe('success');
    });
  });

  describe('post', () => {
    it('calls apiClient.post with url, data, and config', async () => {
      const mockResponse = { id: 123 };
      const postData = { name: 'New Item' };
      vi.mocked(apiClient.post).mockResolvedValue({ data: mockResponse });

      const result = await fetchers.post('/create', postData, { headers: { 'X-Custom': 'header' } });

      expect(apiClient.post).toHaveBeenCalledWith('/create', postData, { headers: { 'X-Custom': 'header' } });
      expect(result).toEqual(mockResponse);
    });

    it('returns response.data', async () => {
      const mockResponse = { created: true };
      vi.mocked(apiClient.post).mockResolvedValue({ data: mockResponse });

      const result = await fetchers.post('/items', { name: 'test' });

      expect(result).toEqual({ created: true });
    });

    it('works without data or config', async () => {
      vi.mocked(apiClient.post).mockResolvedValue({ data: null });

      await fetchers.post('/trigger');

      expect(apiClient.post).toHaveBeenCalledWith('/trigger', undefined, undefined);
    });
  });

  describe('put', () => {
    it('calls apiClient.put with url, data, and config', async () => {
      const mockResponse = { updated: true };
      const updateData = { name: 'Updated' };
      vi.mocked(apiClient.put).mockResolvedValue({ data: mockResponse });

      const result = await fetchers.put('/update/1', updateData);

      expect(apiClient.put).toHaveBeenCalledWith('/update/1', updateData, undefined);
      expect(result).toEqual(mockResponse);
    });

    it('returns response.data', async () => {
      vi.mocked(apiClient.put).mockResolvedValue({ data: { id: 1, name: 'Updated' } });

      const result = await fetchers.put('/items/1', { name: 'Updated' });

      expect(result).toEqual({ id: 1, name: 'Updated' });
    });
  });

  describe('delete', () => {
    it('calls apiClient.delete with url and config', async () => {
      vi.mocked(apiClient.delete).mockResolvedValue({ data: undefined });

      await fetchers.delete('/items/1');

      expect(apiClient.delete).toHaveBeenCalledWith('/items/1', undefined);
    });

    it('returns response.data', async () => {
      vi.mocked(apiClient.delete).mockResolvedValue({ data: { deleted: true } });

      const result = await fetchers.delete<{ deleted: boolean }>('/items/1');

      expect(result).toEqual({ deleted: true });
    });

    it('works with config parameter', async () => {
      vi.mocked(apiClient.delete).mockResolvedValue({ data: undefined });

      await fetchers.delete('/items/1', { headers: { Authorization: 'Bearer token' } });

      expect(apiClient.delete).toHaveBeenCalledWith('/items/1', { headers: { Authorization: 'Bearer token' } });
    });
  });
});
