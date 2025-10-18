import { BrowserRouter as Router, Routes, Route } from 'react-router-dom'
import CategorySidebar from './components/CategorySidebar'
import ProductsView from './pages/ProductsView'
import './App.css'

function App() {
  console.log('App component is mounting');
  return (
    <Router>
      <div className="app">
        <header className="app-header">
          <h1 className="pl-16 lg:pl-0">Digital Collectibles Marketplace</h1>
        </header>

        <div className="app-layout">
          <CategorySidebar />

          <main className="app-main">
            <Routes>
              <Route path="/" element={
                <div>
                  <h2>Welcome to the Marketplace</h2>
                  <p>Select a category from the sidebar to browse products.</p>
                </div>
              } />
              <Route path="/products/:categorySlug/:subcategorySlug" element={<ProductsView />} />
            </Routes>
          </main>
        </div>
      </div>
    </Router>
  )
}

export default App
