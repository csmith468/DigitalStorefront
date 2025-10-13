import apiClient from './api';
import type { Category } from '../types/category';

export const categoryService = {
  async getCategories(): Promise<Category[]> {
    const response = await apiClient.get<Category[]>('/category');
    return response.data;
  },
};