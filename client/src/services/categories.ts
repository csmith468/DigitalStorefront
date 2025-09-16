import apiClient from './api';
import type { Category } from '../types/category';

export const categoryService = {
  getMenu: (): Promise<Category[]> => apiClient.get('/category').then(res => res.data)
};