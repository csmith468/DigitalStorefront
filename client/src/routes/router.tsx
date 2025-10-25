import { createBrowserRouter, Navigate, Outlet } from "react-router-dom";
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ReactQueryDevtools } from '@tanstack/react-query-devtools';
import { Toaster } from "react-hot-toast";
import { toasterConfig } from "../config/toastConfig";
import { AppLayout } from "./AppLayout";
import { HomePage } from "../pages/HomePage";
import { AdminProductList } from "../components/admin/AdminProductList";
import { CreateProductPage } from "../pages/admin/CreateProductPage";
import { EditProductPage } from "../pages/admin/EditProductPage";
import { ProductDetailPage } from "../pages/ProductDetailPage";
import { ProductsView } from "../pages/ProductsView";
import { ProtectedRoute } from "../components/auth/ProtectedRoute";
import { RouteErrorPage } from "./RouteErrorPage";
import { SearchResults } from "../pages/SearchResults";
import { UserProvider } from "../contexts/UserContext";

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
          { path: "/admin", 
            element: <ProtectedRoute><Outlet /></ProtectedRoute>,
            children: [
              { index: true, element: <Navigate to="/admin/products" replace /> },
              { path: "products", element: <AdminProductList /> },
              { path: "products/create", element: <CreateProductPage /> },
              { path: "products/:id/edit", element: <EditProductPage /> },
            ]
          }
        ]
      }
    ]
  }
])