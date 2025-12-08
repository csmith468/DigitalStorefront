export const MessageBuilder = {
  error: {
    action: (action: string, entity: string) => `Failed to ${action} ${entity}. Please try again.`,
    notFound: (entity: string) => `${entity} not found.`,
  },
  success: {
    action: (pastTense: string, entity: string) => `${entity} ${pastTense}!`,
  },
} as const;

export const ErrorMessages = {
  Product: {
    createFailed: MessageBuilder.error.action('create', 'product'),
    updateFailed: MessageBuilder.error.action('update', 'product'),
    deleteFailed: MessageBuilder.error.action('delete', 'product'),
    loadFailed: MessageBuilder.error.action('load', 'products'),
    notFound: MessageBuilder.error.notFound('Product'),
  },

  Image: {
    uploadFailed: MessageBuilder.error.action('upload', 'image'),
    deleteFailed: MessageBuilder.error.action('delete', 'image'),
    setPrimaryFailed: MessageBuilder.error.action('set as primary', 'image'),
    reorderFailed: MessageBuilder.error.action('re-order', 'images'),
    invalidType: 'Please select a valid image file (JPG, PNG, GIF, or WebP)',
    tooLarge: (maxSizeMB: number) => `File size must be less than ${maxSizeMB} MB`,
  },

  Checkout: {
    createFailed: MessageBuilder.error.action('create', 'order'),
  },

  Auth: {
    sessionExpired: 'Your session has expired. Please log in again.',
    loginFailed: 'Invalid username or password.',
    registerFailed: MessageBuilder.error.action('register', 'user'),
    unauthorized: 'You do not have permission to perform this action.',
  },

  Network: {
    connectionFailed: 'Connection failed. Please check your internet connection.',
    timeout: 'Request timed out. Please try again.',
    serverError: 'Server error. Please try again later.',
  },

  Generic: {
    unexpectedError: 'An unexpected error occurred. Please try again.',
    requiredField: 'This field is required.',
  },
} as const;

export const SuccessMessages = {
  Product: {
    created: MessageBuilder.success.action('created', 'Product'),
    updated: MessageBuilder.success.action('updated', 'Product'),
    deleted: MessageBuilder.success.action('deleted', 'Product'),
    addedToCart: 'Shopping cart functionality coming soon!',
  },

  Image: {
    uploaded: MessageBuilder.success.action('uploaded', 'Image'),
    deleted: MessageBuilder.success.action('deleted', 'Image'),
    setPrimary: MessageBuilder.success.action('set as primary', 'Image'),
    reordered: MessageBuilder.success.action('re-ordered', 'Images'),
  },

  Checkout: {
    created: 'Order placed successfully!',
  },

  Auth: {
    loggedIn: 'Welcome back!',
    loggedOut: 'Logged out successfully.',
    registered: 'Registration successful!',
  },
} as const;