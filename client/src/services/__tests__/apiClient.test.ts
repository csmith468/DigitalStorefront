import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import apiClient from '../apiClient';

vi.mock('../../utils/logger', () => ({
  logger: {
    warn: vi.fn(),
    error: vi.fn(),
    debug: vi.fn(),
  },
}));

describe('apiClient', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    sessionStorage.clear();
  });

  afterEach(() => {
    sessionStorage.clear();
  });

  describe('Request Interceptor', () => {
    it('adds Authorization header when token exists in sessionStorage', async () => {
      sessionStorage.setItem('token', 'test-token-123');

      const mockAdapter = vi.fn((config) => {
        expect(config.headers.Authorization).toBe('Bearer test-token-123');
        return Promise.resolve({ data: {}, status: 200, statusText: 'OK', headers: {}, config });
      });

      apiClient.defaults.adapter = mockAdapter;

      await apiClient.get('/test');

      expect(mockAdapter).toHaveBeenCalled();
    });

    it('does not add Authorization header when no token exists', async () => {
      const mockAdapter = vi.fn((config) => {
        expect(config.headers.Authorization).toBeUndefined();
        return Promise.resolve({ data: {}, status: 200, statusText: 'OK', headers: {}, config });
      });

      apiClient.defaults.adapter = mockAdapter;

      await apiClient.get('/test');

      expect(mockAdapter).toHaveBeenCalled();
    });
  });

  describe('Response Interceptor - Success', () => {
    it('passes through successful responses', async () => {
      const mockAdapter = vi.fn((config) =>
        Promise.resolve({
          data: { success: true },
          status: 200,
          statusText: 'OK',
          headers: {},
          config
        })
      );

      apiClient.defaults.adapter = mockAdapter;

      const response = await apiClient.get('/test');

      expect(response.data).toEqual({ success: true });
      expect(response.status).toBe(200);
    });
  });

  describe('Response Interceptor - 401 Errors', () => {
    it('removes token and redirects on 401 for non-auth endpoints', async () => {
      sessionStorage.setItem('token', 'old-token');

      delete (window as any).location;
      (window as any).location = { href: '' };

      const mockAdapter = vi.fn(() =>
        Promise.reject({
          response: { status: 401, data: 'Unauthorized' },
          config: { url: '/products' },
        })
      );

      apiClient.defaults.adapter = mockAdapter;

      try {
        await apiClient.get('/products');
      } catch (error) {
        // Expected to throw
      }

      expect(sessionStorage.getItem('token')).toBeNull();
      expect(window.location.href).toBe('/?auth=login&reason=session-expired');
    });

    it('does not redirect on 401 for auth endpoints', async () => {
      sessionStorage.setItem('token', 'old-token');

      delete (window as any).location;
      (window as any).location = { href: '' };

      const mockAdapter = vi.fn(() =>
        Promise.reject({
          response: { status: 401, data: 'Unauthorized' },
          config: { url: '/auth/login' },
        })
      );

      apiClient.defaults.adapter = mockAdapter;

      try {
        await apiClient.post('/auth/login', {});
      } catch (error) {
        // Expected to throw
      }

      expect(sessionStorage.getItem('token')).toBe('old-token');
      expect(window.location.href).toBe(''); // shouldn't redirect
    });
  });

  describe('Response Interceptor - Canceled Requests', () => {
    it('handles canceled requests without redirecting', async () => {
      delete (window as any).location;
      (window as any).location = { href: '' };

      const canceledError = {
        code: 'ERR_CANCELED',
        config: { url: '/products' },
      };

      const mockAdapter = vi.fn(() => Promise.reject(canceledError));

      apiClient.defaults.adapter = mockAdapter;

      try {
        await apiClient.get('/products');
      } catch (error: any) {
        expect(error.code).toBe('ERR_CANCELED');
      }

      expect(window.location.href).toBe(''); // shouldn't redirect
    });
  });

  describe('Configuration', () => {
    it('has correct base URL configuration', () => {
      expect(apiClient.defaults.baseURL).toContain('/api');
    });

    it('has correct timeout configuration', () => {
      expect(apiClient.defaults.timeout).toBe(10000);
    });

    it('has correct Content-Type header', () => {
      expect(apiClient.defaults.headers['Content-Type']).toBe('application/json');
    });
  });
});