import apiClient from './api';
import type { Category } from '../types/category';

export const getCategories = async (): Promise<Category[]> => {
  const response = await apiClient.get<Category[]>('/category');
  return response.data;
};