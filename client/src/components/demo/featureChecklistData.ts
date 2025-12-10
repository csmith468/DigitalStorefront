export interface FeatureItem {
  id: string;
  title: string;
  techNote: string;
  path?: string; // internal navigation
  externalUrl?: string; // opens in new tab
  completionPaths?: string[];
}

export const STORAGE_KEY = 'portfolio-demo-progress';

export const features: FeatureItem[] = [
  {
    id: 'home',
    title: 'Read the home page',
    techNote: 'Project overview and tech stack',
    path: '/',
    completionPaths: ['/'],
  },
  {
    id: 'payment',
    title: 'Complete a test purchase',
    techNote: 'Stripe PaymentIntent, webhooks, idempotency',
    path: '/products/seasonal/all',
  },
  {
    id: 'admin-form',
    title: 'Try the admin form',
    techNote: 'Complex validation, tag autocomplete, drag-drop images',
    path: '/admin',
    completionPaths: ['/admin/products/try', '/admin/products/create'],
  },
  {
    id: 'github',
    title: 'View source on GitHub',
    techNote: '.NET 8, React 19, SQL Server, Azure',
    externalUrl: 'https://github.com/csmith468/DigitalStorefront',
  },
];