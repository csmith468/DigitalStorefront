import { createBrowserRouter, Navigate, Outlet } from "react-router-dom";
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ReactQueryDevtools } from '@tanstack/react-query-devtools';
import { Toaster } from "react-hot-toast";
import { toasterConfig } from "../config/toastConfig";
import { AppLayout } from "./AppLayout";
import { HomePage } from "../pages/HomePage";
import { AdminProductList } from "../components/admin/AdminProductList";
import { CreateProductFormPage } from "../pages/admin/CreateProductFormPage";
import { EditProductFormPage } from "../pages/admin/EditProductFormPage";
import { ViewProductFormPage } from "../pages/admin/ViewProductFormPage";
import { ProductDetailPage } from "../pages/ProductDetailPage";
import { ProductsView } from "../pages/ProductsView";
import { ProtectedRoute } from "../components/auth/ProtectedRoute";
import { RouteErrorPage } from "./RouteErrorPage";
import { SearchResults } from "../pages/SearchResults";
import { UserProvider } from "../contexts/UserContext";
import { TryProductFormPage } from "../pages/admin/TryProductFormPage";

export const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 1000 * 60 * 5, // 5 minutes
      retry: 1,
      refetchOnWindowFocus: false
    },
  },
});

function RootLayout() {
  return (
    <QueryClientProvider client={queryClient}>
      <UserProvider>
        <Outlet />
        <Toaster {...toasterConfig} />
        <ReactQueryDevtools initialIsOpen={false} />
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
          { path: "/admin", element: <Navigate to="/admin/products" replace /> },
          { path: "/admin/products", element: <AdminProductList /> },
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