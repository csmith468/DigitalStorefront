import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom'
import CategorySidebar from './components/CategorySidebar'
import ProductsView from './pages/ProductsView'
import { Header } from './components/common/Header'
import './App.css'
import SearchResults from './pages/SearchResults'
import { EditProductPage } from './pages/admin/EditProductPage'
import { CreateProductPage } from './pages/admin/CreateProductPage'
import { AdminProductList } from './components/admin/AdminProductList'
import { ProtectedRoute } from './components/auth/ProtectedRoute'
import { HomePage } from './pages/HomePage'

function App() {
  return (
    <Router>
      <div className="app">
        <Header />

        <div className="app-layout">
          <CategorySidebar />

          <main className="app-main">
            <Routes>
              <Route path="/" element={<HomePage />}/>
              <Route path="/search" element={<SearchResults />} />
              <Route path="/products/:categorySlug/:subcategorySlug" element={<ProductsView />} />

              <Route path="/admin" element={<Navigate to="/admin/products" replace />} />
                <Route path="/admin/products" element={
                  <ProtectedRoute>
                    <AdminProductList />
                  </ProtectedRoute>
                } />
                <Route path="/admin/products/create" element={
                  <ProtectedRoute>
                    <CreateProductPage />
                  </ProtectedRoute>
                } />
                <Route path="/admin/products/:id/edit" element={
                  <ProtectedRoute>
                    <EditProductPage />
                  </ProtectedRoute>
                } />
            </Routes>
          </main>
        </div>
      </div>
    </Router>
  );
}

export default App;
