import { createBrowserRouter, Outlet } from "react-router-dom";
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ReactQueryDevtools } from '@tanstack/react-query-devtools';
import { lazy, Suspense } from "react";
import { Toaster } from "react-hot-toast";
import { toasterConfig } from "../config/toastConfig";
import { loadStripe } from "@stripe/stripe-js";
import { Elements } from "@stripe/react-stripe-js";
import { AppLayout } from "./AppLayout";
import { HomePage } from "../pages/HomePage";
import { ProductDetailPage } from "../pages/ProductDetailPage";
import { ProductsView } from "../pages/ProductsView";
import { ProtectedRoute } from "../components/auth/ProtectedRoute";
import { RouteErrorPage } from "./RouteErrorPage";
import { SearchResults } from "../pages/SearchResults";
import { UserProvider } from "../contexts/UserContext";

// Lazy load admin pages
const AdminPage = lazy(() => import("../pages/admin/AdminPage").then(m => ({ default: m.AdminPage })));
const CreateProductFormPage = lazy(() => import("../pages/admin/CreateProductFormPage").then(m => ({ default: m.CreateProductFormPage })));
const EditProductFormPage = lazy(() => import("../pages/admin/EditProductFormPage").then(m => ({ default: m.EditProductFormPage })));
const ViewProductFormPage = lazy(() => import("../pages/admin/ViewProductFormPage").then(m => ({ default: m.ViewProductFormPage })));
const TryProductFormPage = lazy(() => import("../pages/admin/TryProductFormPage").then(m => ({ default: m.TryProductFormPage })));

function LoadingFallback() {
  return (
    <div className="flex items-center justify-center min-h-screen">
      <div className="text-center">
        <div className="inline-block h-8 w-8 animate-spin rounded-full border-4 border-solid border-primary border-r-transparent"></div>
        <p className="mt-2 text-gray-600">Loading...</p>
      </div>
    </div>
  );
}

export const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 1000 * 60 * 5, // 5 minutes
      retry: 1,
      refetchOnWindowFocus: false
    },
  },
});

const stripePromise = loadStripe(import.meta.env.VITE_STRIPE_PUBLISHABLE_KEY);

function RootLayout() {
  return (
    <QueryClientProvider client={queryClient}>
      <UserProvider>
        <Elements stripe={stripePromise}>
          <Suspense fallback={<LoadingFallback />}>
            <Outlet />
          </Suspense>
          <Toaster {...toasterConfig} />
          <ReactQueryDevtools initialIsOpen={false} />
        </Elements>
      </UserProvider>
    </QueryClientProvider>
  );
}

// NOTE (to self): Reminds me of Angular route guards
export const router = createBrowserRouter([
  {
    element: <RootLayout />,
    errorElement: <RouteErrorPage />,
    children: [
      {
        element: <AppLayout />,
        children: [
          { path: "/", element: <HomePage /> },
          { path: "/search", element: <SearchResults /> },
          { path: "/products/:categorySlug/:subcategorySlug", element: <ProductsView /> },
          { path: "/product/:slug", element: <ProductDetailPage /> },
          { path: "/admin", element: <AdminPage /> },
          { path: "/admin/products/try", element: <TryProductFormPage /> },
          { path: "/admin/products/:id/view", element: <ViewProductFormPage /> },
          {
            path: "/admin/products/create",
            element: <ProtectedRoute><CreateProductFormPage /></ProtectedRoute>
          },
          {
            path: "/admin/products/:id/edit",
            element: <ProtectedRoute><EditProductFormPage /></ProtectedRoute>
          },
        ]
      }
    ]
  }
])