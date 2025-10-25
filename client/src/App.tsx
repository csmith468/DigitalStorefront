import { RouterProvider } from 'react-router-dom'
import { router } from './routes/router'
import './App.css'
import { ErrorBoundary } from './routes/ErrorBoundary';

function App() {
  return( 
    <ErrorBoundary>
      <RouterProvider router={router} />
    </ErrorBoundary>
  )
}

export default App;
