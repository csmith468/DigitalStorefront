import apiClient from './api';
import type { AuthResponse, LoginDto, RegisterDto } from '../types/auth';

export const authService = {
  login: async (dto: LoginDto): Promise<AuthResponse> => {
    const response = await apiClient.post<AuthResponse>('/auth/login', dto);
    return response.data;
  },
  register: async (dto: RegisterDto): Promise<AuthResponse> => {
    const response = await apiClient.post<AuthResponse>('/auth/register', dto);
    return response.data;
  },
  refreshToken: async (): Promise<AuthResponse> => {
    const response = await apiClient.post<AuthResponse>('/auth/refresh-token');
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