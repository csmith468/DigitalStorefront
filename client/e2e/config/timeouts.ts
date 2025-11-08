export const TIMEOUTS = {
  // Fast DOM updates like animations or chips appearing (2s)
  DOM_UPDATE: 2000,

  // Standard API call + render like search or filters (5s)
  API_CALL: 5000,

  // Full page loading (10s)
  PAGE_LOAD: 10000,

  // Navigation between pages for route changes and fetching data (15s)
  NAVIGATION: 15000,

  // Form submission, validation, API call(s), redirect (20s)
  FORM_SUBMIT: 20000,
} as const;