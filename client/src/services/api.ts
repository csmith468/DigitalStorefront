import axios from 'axios';

const API_BASE_URL = 'http://localhost:5000'; //import.meta.env.VITE_API_URL || 

const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json'
  },
  timeout: 10000,
});

apiClient.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token');
    if (token)
      config.headers.Authorization = `Bearer ${token}`;
    return config;
  },
  (error) => {
    console.error('Request error:', error);
    return Promise.reject(error);
  }
);

apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('token');
      window.location.href = '/?auth=login&reason=session-expired';
    }
    console.error('Response Error: ', error);
    return Promise.reject(error);
  }
);

export default apiClient;