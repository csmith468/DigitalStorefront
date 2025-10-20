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
    localStorage.removeItem('token');
  },
  getStoredToken: (): string | null => {
    return localStorage.getItem('token');
  },
  setStoredToken: (token: string): void => {
    localStorage.setItem('token', token);
  }
}