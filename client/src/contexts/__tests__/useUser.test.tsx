import { describe, it, expect, vi } from 'vitest';
import { renderHook } from '@testing-library/react';
import { useUser } from '../useUser';
import { UserContext, type UserContextType } from '../UserContext';
import { createMockUserContext } from '../../tests/fixtures';

describe('useUser', () => {
  const wrapper = (contextValue: UserContextType) =>
    ({ children }: { children: React.ReactNode }) => (
      <UserContext.Provider value={contextValue}>
        {children}
      </UserContext.Provider>
    );

  describe('Error Handling', () => {
    it('throws error when used outside UserProvider', () => {
      // Suppress console.error for this test
      const spy = vi.spyOn(console, 'error').mockImplementation(() => {});

      expect(() => {
        renderHook(() => useUser());
      }).toThrow('useUser must be used within UserProvider.');

      spy.mockRestore();
    });
  });

  describe('hasRole', () => {
    it('returns true when user has the role', () => {
      const context = createMockUserContext({ roles: ['Admin', 'ProductWriter'] });
      const { result } = renderHook(() => useUser(), { wrapper: wrapper(context) });

      expect(result.current.hasRole('Admin')).toBe(true);
      expect(result.current.hasRole('ProductWriter')).toBe(true);
    });

    it('returns false when user does not have the role', () => {
      const context = createMockUserContext({ roles: ['ProductWriter'] });
      const { result } = renderHook(() => useUser(), { wrapper: wrapper(context) });

      expect(result.current.hasRole('Admin')).toBe(false);
      expect(result.current.hasRole('ImageManager')).toBe(false);
    });

    it('returns false when roles array is empty', () => {
      const context = createMockUserContext({ roles: [] });
      const { result } = renderHook(() => useUser(), { wrapper: wrapper(context) });

      expect(result.current.hasRole('Admin')).toBe(false);
    });
  });

  describe('isLoggedIn', () => {
    it('returns true when user exists', () => {
      const context = createMockUserContext({
        user: { userId: 1, username: 'testuser' }
      });
      const { result } = renderHook(() => useUser(), { wrapper: wrapper(context) });

      expect(result.current.isLoggedIn()).toBe(true);
    });

    it('returns false when user is null', () => {
      const context = createMockUserContext({ user: null });
      const { result } = renderHook(() => useUser(), { wrapper: wrapper(context) });

      expect(result.current.isLoggedIn()).toBe(false);
    });
  });

  describe('isAdmin', () => {
    it('returns true when user has Admin role', () => {
      const context = createMockUserContext({ roles: ['Admin'] });
      const { result } = renderHook(() => useUser(), { wrapper: wrapper(context) });

      expect(result.current.isAdmin()).toBe(true);
    });

    it('returns false when user does not have Admin role', () => {
      const context = createMockUserContext({ roles: ['ProductWriter'] });
      const { result } = renderHook(() => useUser(), { wrapper: wrapper(context) });

      expect(result.current.isAdmin()).toBe(false);
    });
  });

  describe('canManageProducts', () => {
    it('returns true when user has Admin role', () => {
      const context = createMockUserContext({ roles: ['Admin'] });
      const { result } = renderHook(() => useUser(), { wrapper: wrapper(context) });

      expect(result.current.canManageProducts()).toBe(true);
    });

    it('returns true when user has ProductWriter role', () => {
      const context = createMockUserContext({ roles: ['ProductWriter'] });
      const { result } = renderHook(() => useUser(), { wrapper: wrapper(context) });

      expect(result.current.canManageProducts()).toBe(true);
    });

    it('returns false when user has neither role', () => {
      const context = createMockUserContext({ roles: ['ImageManager'] });
      const { result } = renderHook(() => useUser(), { wrapper: wrapper(context) });

      expect(result.current.canManageProducts()).toBe(false);
    });
  });

  describe('canManageImages', () => {
    it('returns true when user has Admin role', () => {
      const context = createMockUserContext({ roles: ['Admin'] });
      const { result } = renderHook(() => useUser(), { wrapper: wrapper(context) });

      expect(result.current.canManageImages()).toBe(true);
    });

    it('returns true when user has ImageManager role', () => {
      const context = createMockUserContext({ roles: ['ImageManager'] });
      const { result } = renderHook(() => useUser(), { wrapper: wrapper(context) });

      expect(result.current.canManageImages()).toBe(true);
    });

    it('returns false when user has neither role', () => {
      const context = createMockUserContext({ roles: ['ProductWriter'] });
      const { result } = renderHook(() => useUser(), { wrapper: wrapper(context) });

      expect(result.current.canManageImages()).toBe(false);
    });
  });

  describe('Context Passthrough', () => {
    it('passes through all UserContext properties', () => {
      const context = createMockUserContext({
        user: { userId: 1, username: 'testuser' },
        isAuthenticated: true,
        roles: ['Admin'],
        isLoading: false,
      });
      const { result } = renderHook(() => useUser(), { wrapper: wrapper(context) });

      expect(result.current.user).toEqual({ userId: 1, username: 'testuser' });
      expect(result.current.isAuthenticated).toBe(true);
      expect(result.current.roles).toEqual(['Admin']);
      expect(result.current.isLoading).toBe(false);
      expect(result.current.login).toBeDefined();
      expect(result.current.register).toBeDefined();
      expect(result.current.logout).toBeDefined();
    });
  });
});