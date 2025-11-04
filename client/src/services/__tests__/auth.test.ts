import { describe, it, expect, vi, beforeEach } from 'vitest';
import { fetchers } from '../fetchers';
import { authService } from '../auth';
import { mockAuthResponse, mockLoginRequest, mockRegisterRequest } from '../../tests/fixtures/auth-fixtures';

vi.mock('../fetchers', () => ({
  fetchers: {
    get: vi.fn(),
    post: vi.fn(),
    put: vi.fn(),
    delete: vi.fn(),
  },
}));

describe('Auth Service', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    sessionStorage.clear();
  });

  describe('login', () => {
    it('calls fetchers.post with correct URL and data', async () => {
      vi.mocked(fetchers.post).mockResolvedValue(mockAuthResponse);

      const result = await authService.login(mockLoginRequest);

      expect(fetchers.post).toHaveBeenCalledWith('/auth/login', mockLoginRequest);
      expect(result).toEqual(mockAuthResponse);
    });
  });

  describe('register', () => {
    it('calls fetchers.post with correct URL and data', async () => {
      vi.mocked(fetchers.post).mockResolvedValue(mockAuthResponse);

      const result = await authService.register(mockRegisterRequest);

      expect(fetchers.post).toHaveBeenCalledWith('/auth/register', mockRegisterRequest);
      expect(result).toEqual(mockAuthResponse);
    });
  });

  describe('refreshToken', () => {
    it('calls fetchers.post with correct URL', async () => {
      vi.mocked(fetchers.post).mockResolvedValue(mockAuthResponse);

      const result = await authService.refreshToken();

      expect(fetchers.post).toHaveBeenCalledWith('/auth/refresh-token');
      expect(result).toEqual(mockAuthResponse);
    });
  });

  describe('logout', () => {
    it('removes token from sessionStorage', () => {
      sessionStorage.setItem('token', 'test-token');
      authService.logout();
      expect(sessionStorage.getItem('token')).toBeNull();
    });

    it('does nothing if no token exists', () => {
      expect(sessionStorage.getItem('token')).toBeNull();
      authService.logout();
      expect(sessionStorage.getItem('token')).toBeNull();
    });
  });

  describe('getStoredToken', () => {
    it('returns token from sessionStorage when it exists', () => {
      sessionStorage.setItem('token', 'test-token-123');
      const result = authService.getStoredToken();
      expect(result).toBe('test-token-123');
    });

    it('returns null when no token exists', () => {
      const result = authService.getStoredToken();
      expect(result).toBeNull();
    });
  });

  describe('setStoredToken', () => {
    it('stores token in sessionStorage', () => {
      authService.setStoredToken('new-token-456');
      expect(sessionStorage.getItem('token')).toBe('new-token-456');
    });

    it('overwrites existing token', () => {
      sessionStorage.setItem('token', 'old-token');
      authService.setStoredToken('new-token');
      expect(sessionStorage.getItem('token')).toBe('new-token');
    });
  });
});