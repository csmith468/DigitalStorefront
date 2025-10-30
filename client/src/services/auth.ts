import apiClient from './api';
import type { Auth, LoginRequest, RegisterRequest } from '../types/auth';

export const authService = {
  login: async (dto: LoginRequest): Promise<Auth> => {
    const response = await apiClient.post<Auth>('/auth/login', dto);
    return response.data;
  },
  register: async (dto: RegisterRequest): Promise<Auth> => {
    const response = await apiClient.post<Auth>('/auth/register', dto);
    return response.data;
  },
  refreshToken: async (): Promise<Auth> => {
    const response = await apiClient.post<Auth>('/auth/refresh-token');
    return response.data;
  },
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