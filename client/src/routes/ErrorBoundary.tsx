import { ExclamationTriangleIcon } from "@heroicons/react/24/outline";
import { Component, type ErrorInfo, type ReactNode } from "react";
import { logger } from "../utils/logger";


interface ErrorBoundaryProps {
  children: ReactNode;
  fallback?: ReactNode;
}

interface ErrorBoundaryState {
  hasError: boolean;
  error: Error | null;
}

// I know I could use react-error-boundary but I wanted to write a class-based component 
export class ErrorBoundary extends Component<ErrorBoundaryProps, ErrorBoundaryState> {
  
  public state: ErrorBoundaryState = {
    hasError: false,
    error: null
  };

  // Lifecycle method to catch error (during render)
  public static getDerivedStateFromError(error: Error): ErrorBoundaryState {
  return { hasError: true, error };
  }

  // Log errors (TBD to where)
  public componentDidCatch(error: Error, errorInfo: ErrorInfo): void {
    logger.error('ErrorBoundary caught: ', { error, errorInfo });
  }

  private handleReset = () => {
    this.setState({ hasError: false, error: null });
  };

  public render() {
    if (this.state.hasError) {
      if (this.props.fallback) {
        return this.props.fallback;
      }
      return (
        <div className="container mx-auto px-4 py-8">
          <div className="flex justify-center items-center min-h-[400px]">
            <div className="bg-white rounded-lg shadow-md p-8 max-w-md w-full">
              <div 
                className="flex items-center justify-center w-12 h-12 mx-auto rounded-full mb-4"
                style={{ backgroundColor: 'var(--color-danger-light)' }}
              >
                <ExclamationTriangleIcon 
                  className="w-6 h-6" 
                  style={{ color: 'var(--color-danger)' }} 
                />
              </div>

              <h1 
                className="text-2xl font-bold mb-2 text-center"
                style={{ color: 'var(--color-text-primary)' }}
              >Something Went Wrong
              </h1>

              <p 
                className="mb-6 text-center"
                style={{ color: 'var(--color-text-secondary)' }}
              >We encountered an unexpected error. Please try again.
              </p>

              {/* (Only show error in dev) */}
              {import.meta.env.DEV && this.state.error && (
                <div 
                  className="mb-6 p-4 rounded"
                  style={{ 
                    backgroundColor: 'var(--color-hover-bg)',
                    border: '1px solid var(--color-border)'
                  }}
                >
                  <p 
                    className="text-sm font-mono break-words"
                    style={{ color: 'var(--color-danger)' }}
                  >
                    {this.state.error.message}
                  </p>
                </div>
              )}

              <div className="flex gap-3">
                <button
                  onClick={this.handleReset}
                  className="flex-1 px-4 py-2 rounded transition-colors"
                  style={{ backgroundColor: 'var(--color-primary)', color: 'white' }}
                  onMouseEnter={(e) => e.currentTarget.style.backgroundColor = 'var(--color-primary-dark)' }
                  onMouseLeave={(e) => e.currentTarget.style.backgroundColor = 'var(--color-primary)' }
                >Try Again
                </button>
                <button
                  onClick={() => window.location.href = '/'}
                  className="flex-1 px-4 py-2 rounded transition-colors"
                  style={{
                    backgroundColor: 'var(--color-border)',
                    color: 'var(--color-text-primary)'
                  }}
                >Go Home
                </button>
              </div>
            </div>
          </div>
        </div>
      );
    }

    return this.props.children;
  }
}