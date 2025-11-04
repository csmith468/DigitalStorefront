import { beforeEach, describe, expect, it, vi } from "vitest";
import { UserContext, UserProvider } from "../UserContext";
import { authService } from "../../services/auth";
import { renderHook, waitFor, act } from "@testing-library/react";
import { useContext } from "react";
import { mockAuthResponse, mockLoginRequest, mockRegisterRequest } from "../../tests/fixtures";


vi.mock('../../services/auth', () => ({
  authService: {
    login: vi.fn(),
    register: vi.fn(),
    refreshToken: vi.fn(),
    logout: vi.fn(),
    getStoredToken: vi.fn(),
    setStoredToken: vi.fn(),
  },
}));

vi.mock('../../components/auth/AuthModal.tsx', () => ({
  AuthModal: () => null,
}));

describe('UserContext', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    sessionStorage.clear();

    // reset window.location.href for logout tests
    delete (window as any).location; 
    (window as any).location = { href: '' };
  });

  const wrapper = ({ children }: { children: React.ReactNode }) => (
    <UserProvider>{children}</UserProvider>
  );

  describe('Initial auth check', () => {
    it('starts with isLoading true and sets to false after init', async () => {
      vi.mocked(authService.getStoredToken).mockReturnValue(null);

      const { result } = renderHook(() => useContext(UserContext), { wrapper });

      await waitFor(() => {
        expect(result.current?.isLoading).toBe(false);
      });
      expect(result.current?.user).toBeNull();
      expect(result.current?.isAuthenticated).toBe(false);
    });

    it('restores user session if token exists', async () => {
      vi.mocked(authService.getStoredToken).mockReturnValue('stored-token');
      vi.mocked(authService.refreshToken).mockResolvedValue(mockAuthResponse);

      const { result } = renderHook(() => useContext(UserContext), { wrapper });

      await waitFor(() => {
        expect(result.current?.isLoading).toBe(false);
      });

      expect(authService.refreshToken).toHaveBeenCalled();
      expect(result.current?.user).toEqual({
        userId: mockAuthResponse.userId,
        username: mockAuthResponse.username,
      });
      expect(result.current?.roles).toEqual(mockAuthResponse.roles);
      expect(result.current?.isAuthenticated).toBe(true);
    });

    it('clears token if refresh fails', async () => {
      vi.mocked(authService.getStoredToken).mockReturnValue('expired-token');
      vi.mocked(authService.refreshToken).mockRejectedValue(new Error('Token expired'));

      const { result } = renderHook(() => useContext(UserContext), { wrapper });

      await waitFor(() => {
        expect(result.current?.isLoading).toBe(false);
      });

      expect(authService.logout).toHaveBeenCalled();
      expect(result.current?.user).toBeNull();
      expect(result.current?.isAuthenticated).toBe(false);
    });
  });

  describe('login', () => {
    it('calls login and sets user and roles', async () => {
      vi.mocked(authService.getStoredToken).mockReturnValue(null);
      vi.mocked(authService.login).mockResolvedValue(mockAuthResponse);

      const { result } = renderHook(() => useContext(UserContext), { wrapper });

      await waitFor(() => {
        expect(result.current?.isLoading).toBe(false);
      });

      await act(async () => {
        await result.current?.login(mockLoginRequest);
      });

      await waitFor(() => {
        expect(result.current?.isAuthenticated).toBe(true);
      });

      expect(authService.login).toHaveBeenCalledWith(mockLoginRequest);
      expect(authService.setStoredToken).toHaveBeenCalledWith(mockAuthResponse.token);
      expect(result.current?.user).toEqual({
        userId: mockAuthResponse.userId,
        username: mockAuthResponse.username,
      });
      expect(result.current?.roles).toEqual(mockAuthResponse.roles);
    });
  });

  describe('register', () => {
    it('calls register and sets user/roles', async () => {
      vi.mocked(authService.getStoredToken).mockReturnValue(null);
      vi.mocked(authService.register).mockResolvedValue(mockAuthResponse);

      const { result } = renderHook(() => useContext(UserContext), { wrapper });

      await waitFor(() => {
        expect(result.current?.isLoading).toBe(false);
      });

      await act(async () => {
        await result.current?.register(mockRegisterRequest);
      });

      await waitFor(() => {
        expect(result.current?.isAuthenticated).toBe(true);
      });

      expect(authService.register).toHaveBeenCalledWith(mockRegisterRequest);
      expect(authService.setStoredToken).toHaveBeenCalledWith(mockAuthResponse.token);
      expect(result.current?.user).toEqual({
        userId: mockAuthResponse.userId,
        username: mockAuthResponse.username,
      });
      expect(result.current?.isAuthenticated).toBe(true);
    });
  });

  describe('logout', () => {
    it('clears user and redirects to homepage', async () => {
      vi.mocked(authService.getStoredToken).mockReturnValue('token');
      vi.mocked(authService.refreshToken).mockResolvedValue(mockAuthResponse);
      vi.mocked(authService.setStoredToken).mockImplementation(() => {});

      const { result } = renderHook(() => useContext(UserContext), { wrapper });

      await waitFor(() => {
        expect(result.current?.isAuthenticated).toBe(true);
      });

      act(() => {
        result.current?.logout();
      });

      expect(authService.logout).toHaveBeenCalled();
      expect(window.location.href).toBe('/');
    });
  });

  describe('auth modal', () => {
    it('openAuthModal and closeAuthModal do not throw errors', async () => {
      vi.mocked(authService.getStoredToken).mockReturnValue(null);

      const { result } = renderHook(() => useContext(UserContext), { wrapper });

      await waitFor(() => {
        expect(result.current).toBeDefined();
      });

      // Verify they exist and don't throw
      act(() => {
        result.current?.openAuthModal('login');
        result.current?.closeAuthModal();
      });

      expect(result.current).toBeDefined();
    });
  });
});