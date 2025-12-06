import axios from 'axios';
import { logger } from '../utils/logger';
import axiosRetry from 'axios-retry';

const API_BASE_URL = `${import.meta.env.VITE_API_URL || 'http://localhost:5000'}/api`;

const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json'
  },
  timeout: 10000,
});

axiosRetry(apiClient, {
  retries: 3,
  retryDelay: axiosRetry.exponentialDelay,
  retryCondition: (error) => {
    if (axios.isCancel(error) || error.code === 'ERR_CANCELED') 
      return false;

    const isNetworkError = axiosRetry.isNetworkOrIdempotentRequestError(error);
    const isServerError = error.response?.status !== undefined && error.response.status >= 500;
    return isNetworkError || isServerError;
  },
  onRetry: (retryCount, error, requestConfig) => {
    logger.warn(`Retrying request (${retryCount}/3): `, {
      url: requestConfig.url,
      method: requestConfig.method,
      error: error.message
    });
  },
});

apiClient.interceptors.request.use(
  (config) => {
    const token = sessionStorage.getItem('token');
    if (token)
      config.headers.Authorization = `Bearer ${token}`;

    // Added idempotency key for mutation requests (POST, PUT, DELETE)
    // The key is stored on config so retries use the same key
    const mutationMethods = ['post', 'put', 'delete'];
    if (config.method && mutationMethods.includes(config.method.toLowerCase())) {
      if (!config.headers['Idempotency-Key']) {
        config.headers['Idempotency-Key'] = crypto.randomUUID();
      }
    }

    return config;
  },
  (error) => {
    logger.error('Request error:', error);
    return Promise.reject(error);
  }
);

apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    const isAuthEndpoint = error.config?.url?.includes('/auth/');

    if (axios.isCancel(error) || error.code === 'ERR_CANCELED') {
      logger.debug('Request canceled:', error.config?.url);
      return Promise.reject(error);
    }

    if (error.response?.status === 401 && !isAuthEndpoint) {
      sessionStorage.removeItem('token');
      window.location.href = '/?auth=login&reason=session-expired';
    }
    logger.error('Response Error: ', error);
    return Promise.reject(error);
  }
);

export default apiClient;