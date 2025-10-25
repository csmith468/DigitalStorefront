import { createBrowserRouter, Navigate, Outlet } from "react-router-dom";
import { AppLayout } from "./AppLayout";
import { HomePage } from "../pages/HomePage";
import SearchResults from "../pages/SearchResults";
import ProductsView from "../pages/ProductsView";
import { ProtectedRoute } from "../components/auth/ProtectedRoute";
import { AdminProductList } from "../components/admin/AdminProductList";
import { CreateProductPage } from "../pages/admin/CreateProductPage";
import { EditProductPage } from "../pages/admin/EditProductPage";
import { RouteErrorPage } from "./RouteErrorPage";

// NOTE (to self): Reminds me of Angular route guards
export const router = createBrowserRouter([
  {
    element: <AppLayout />,
    errorElement: <RouteErrorPage />,
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
])