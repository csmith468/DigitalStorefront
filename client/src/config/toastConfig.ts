import type { ToasterProps } from 'react-hot-toast';

export const toasterConfig: ToasterProps = {
  position: 'top-right',
  containerStyle: {
    top: '100px',
    zIndex: 40,
  },
  toastOptions: {
    duration: 4000,
    style: {
      background: '#fff',
      color: '#333',
    },
    success: {
      style: {
        background: '#98dd57ff',
        color: '#065f28ff',
        border: '1px solid #34d399',
      },
      iconTheme: {
        primary: '#10b981',
        secondary: '#d1fae5',
      },
    },
    error: {
      style: {
        background: '#fee2e2',
        color: '#991b1b',
        border: '1px solid #f87171',
      },
      iconTheme: {
        primary: '#ef4444',
        secondary: '#fee2e2',
      },
    },
  },
};
