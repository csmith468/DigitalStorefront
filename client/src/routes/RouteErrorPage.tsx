import { ExclamationTriangleIcon } from "@heroicons/react/24/outline";
import { isRouteErrorResponse, useNavigate, useRouteError } from "react-router-dom";


export function RouteErrorPage() {
  const error = useRouteError();
  const navigate = useNavigate();

  const isRouteError = isRouteErrorResponse(error);
  const status = isRouteError ? error.status : 500;
  const statusText = isRouteError ? error.statusText : 'Error';
  const message = isRouteError
    ? error.data?.messsage || error.statusText
    : (error instanceof Error ? error.message : 'An unexpected error occurrred');

  return (
    <div className="container mx-auto px-4 py-8">
      <div className="flex justify-center items-center min-h-[400px]">
        <div className="bg-white rounded-lg shadow-md p-8 max-w-md w-full">
          <div 
            className="flex items-center justify-center w-16 h-16 mx-auto rounded-full mb-4"
            style={{ backgroundColor: 'var(--color-warning-light)' }}
          ><ExclamationTriangleIcon className="w-8 h-8" style={{ color: 'var(--color-warning)' }}/>
          </div>

          <div 
            className="text-6xl font-bold text-center mb-2"
            style={{ 
              background: 'linear-gradient(90deg, var(--color-primary) 0%, var(--color-accent) 100%)',
              WebkitBackgroundClip: 'text',
              WebkitTextFillColor: 'transparent',
              backgroundClip: 'text'
            }}
          >{status}
          </div>

          <h1 className="text-2xl font-bold mb-2 text-center"style={{ color: 'var(--color-text-primary)' }}>{statusText}</h1>
          <p className="mb-6 text-center" style={{ color: 'var(--color-text-secondary)' }}>{message}</p>

          {import.meta.env.DEV && error instanceof Error && error.stack && (
            <details className="mb-6">
              <summary 
                className="cursor-pointer mb-2 font-medium"
                style={{ color: 'var(--color-text-primary)' }}
              >Stack Trace
              </summary>
              <pre 
                className="text-xs overflow-auto p-4 rounded"
                style={{ 
                  backgroundColor: 'var(--color-hover-bg)',
                  border: '1px solid var(--color-border)',
                  color: 'var(--color-text-secondary)'
                }}
              >{error.stack}
              </pre>
            </details>
          )}

          <div className="flex gap-3">
            <button
              onClick={() => navigate(-1)}
              className="flex-1 px-4 py-2 rounded transition-colors"
              style={{
                backgroundColor: 'var(--color-border)',
                color: 'var(--color-text-primary)'
              }}
            >Go Back
            </button>
            <button
              onClick={() => navigate('/')}
              className="flex-1 px-4 py-2 rounded transition-colors"
              style={{
                backgroundColor: 'var(--color-primary)',
                color: 'white'
              }}
              onMouseEnter={(e) =>
                e.currentTarget.style.backgroundColor = 'var(--color-primary-dark)'
              }
              onMouseLeave={(e) =>
                e.currentTarget.style.backgroundColor = 'var(--color-primary)'
              }
            >Go Home
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}