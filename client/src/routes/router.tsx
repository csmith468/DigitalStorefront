import { createBrowserRouter, Navigate, Outlet } from "react-router-dom";
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ReactQueryDevtools } from '@tanstack/react-query-devtools';
import { AppLayout } from "./AppLayout";
import { HomePage } from "../pages/HomePage";
import SearchResults from "../pages/SearchResults";
import ProductsView from "../pages/ProductsView";
import { ProtectedRoute } from "../components/auth/ProtectedRoute";
import { AdminProductList } from "../components/admin/AdminProductList";
import { CreateProductPage } from "../pages/admin/CreateProductPage";
import { EditProductPage } from "../pages/admin/EditProductPage";
import { RouteErrorPage } from "./RouteErrorPage";
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
          { path: "/admin", 
            element: <ProtectedRoute><Outlet /></ProtectedRoute>,
            children: [
              { index: true, element: <Navigate to="/admin/products" replace /> },
              { path: "products", element: <AdminProductList /> },
              { path: "products/create", element: <CreateProductPage /> },
              {
                path: "products/:id/edit",
                element: <Navigate to="details" replace />
              },
              { path: "products/:id/edit/details", element: <EditProductPage /> },
              { path: "products/:id/edit/images", element: <EditProductPage /> },
            ]
          }
        ]
      }
    ]
  }
])