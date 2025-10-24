import { BrowserRouter as Router, Routes, Route } from 'react-router-dom'
import CategorySidebar from './components/CategorySidebar'
import ProductsView from './pages/ProductsView'
import { Header } from './components/common/Header'
import './App.css'
import SearchResults from './pages/SearchResults'

function App() {
  return (
    <Router>
      <div className="app">
        <Header />

        <div className="app-layout">
          <CategorySidebar />

          <main className="app-main">
            <Routes>
              <Route path="/" element={
                <div>
                  <h2>Welcome to the Marketplace</h2>
                  <p>Select a category from the sidebar to browse products.</p>
                </div>
              }/>
              <Route path="/search" element={<SearchResults />} />
              <Route
                path="/products/:categorySlug/:subcategorySlug"
                element={<ProductsView />}
              />
            </Routes>
          </main>
        </div>
      </div>
    </Router>
  );
}

export default App;
