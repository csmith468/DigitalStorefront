import { BrowserRouter as Router, Routes, Route } from 'react-router-dom'
import CategorySidebar from './components/CategorySidebar'
import ProductSubcategory from './pages/ProductSubcategory'
import './App.css'

function App() {
  console.log('App component is mounting');
  return (
    <Router>
      <div className="app">
        <header className="app-header">
          <h1>Digital Collectibles Marketplace</h1>
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
              <Route path="/subcategory/:slug" element={<ProductSubcategory />} />
            </Routes>
          </main>
        </div>
      </div>
    </Router>
  )
}

export default App
