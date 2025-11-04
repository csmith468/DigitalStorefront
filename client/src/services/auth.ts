import { fetchers } from './fetchers';
import type { Auth, LoginRequest, RegisterRequest } from '../types/auth';

export const authService = {
  login: (dto: LoginRequest): Promise<Auth> => fetchers.post<Auth>('/auth/login', dto),

  register: (dto: RegisterRequest): Promise<Auth> => fetchers.post<Auth>('/auth/register', dto),

  refreshToken: (): Promise<Auth> => fetchers.post<Auth>('/auth/refresh-token'),

  logout: () => {
    sessionStorage.removeItem('token');
  },

  getStoredToken: (): string | null => {
    return sessionStorage.getItem('token');
  },

  // NOTE: using session storage so it clears on tab close (unlike localStorage)
  // In production, I'd store them in httpOnly cookies to prevent XSS attacks
  setStoredToken: (token: string): void => {
    sessionStorage.setItem('token', token);
  }
}