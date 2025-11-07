import type { Auth, LoginRequest, RegisterRequest, User } from '../../types/auth';
import type { UserContextType } from '../../contexts/UserContext';
import { vi } from 'vitest';

const userId = 1;
const username = 'testUser';
const password = 'password123';

export const mockAuthResponse: Auth = {
  userId: userId,
  username: username,
  token: 'token-123',
  roles: ['Admin']
};

export const mockLoginRequest: LoginRequest = {
  username,
  password,
};

export const mockRegisterRequest: RegisterRequest = {
  username,
  password,
  confirmPassword: password,
  firstName: 'John',
  lastName: 'Doe',
  email: 'test@test.com'
};

export const mockUser: User = {
  userId,
  username
}

export function createMockUserContext(overrides: Partial<UserContextType> = {}): UserContextType {
  return {
    user: null,
    isAuthenticated: false,
    roles: [],
    isLoading: false,
    login: vi.fn(),
    register: vi.fn(),
    logout: vi.fn(),
    openAuthModal: vi.fn(),
    closeAuthModal: vi.fn(),
    ...overrides,
  };
}

export function createMockUseUserReturn(roles: string[] = []) {
  const hasRole = (roleName: string) => roles.includes(roleName);
  const isAdmin = () => hasRole('Admin');
  const canManageProducts = () => hasRole('Admin') || hasRole('ProductWriter');

  return {
    user: roles.length > 0 ? { userId: 1, username: 'testuser' } : null,
    isAuthenticated: roles.length > 0,
    roles,
    isLoading: false,
    login: vi.fn(),
    register: vi.fn(),
    logout: vi.fn(),
    openAuthModal: vi.fn(),
    closeAuthModal: vi.fn(),
    hasRole,
    isLoggedIn: () => roles.length > 0,
    isAdmin,
    canManageProducts,
    canManageImages: () => hasRole('Admin') || hasRole('ImageManager'),
  };
}