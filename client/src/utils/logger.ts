const isDev = import.meta.env.DEV;

export const logger = {
  error: (message: string, error?: unknown) => {
    if (isDev) {
      console.error(message, error);
    }

    // In production, this will go to Application Insights
    // if (import.meta.env.PROD) {
    //   appInsights.trackException({ exception: error, properties: { message } });
    // }
  },

  warn: (message: string, data?: unknown) => {
    if (isDev) {
      console.warn(message, data);
    }
  },

  info: (message: string, data?: unknown) => {
    if (isDev) {
      console.info(message, data);
    }
  },

  debug: (message: string, data?: unknown) => {
    if (isDev) {
      console.debug(message, data);
    }
  },
};