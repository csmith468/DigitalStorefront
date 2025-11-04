import type { ReactElement } from "react";
import { UserContext, type UserContextType } from "../contexts/UserContext";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, type RenderOptions } from "@testing-library/react";
import { createMockUserContext } from "./fixtures";
import { createMemoryRouter, RouterProvider } from "react-router-dom";
import userEvent from "@testing-library/user-event";
import { vi } from "vitest";

function createTestQueryClient(): QueryClient {
  return new QueryClient({
    defaultOptions: {
      queries: { retry: false },
      mutations: { retry: false },
    },
  });
}

export function createQueryWrapper() {
  const queryClient = createTestQueryClient();

  const Wrapper = ({ children }: { children: React.ReactNode }) => (
    <QueryClientProvider client={queryClient}>
      {children}
    </QueryClientProvider>
  );

  return Wrapper;
}

/**
 * Render with all providers: QueryClient + UserContext + Router
 * 
 * @example
 * const { user } = renderWithProviders(<MyComponent />);
 * 
 * @example
 * // With authenticated user
 * renderWithProviders(<MyComponent />, {
 *   userContext: { 
 *     isAuthenticated: true, 
 *     user: { userId: 1, username: 'testuser' },
 *     roles: ['Admin', 'ProductWriter', 'ImageManager']
 *   }
 * });
*/
export function renderWithProviders(
  ui: ReactElement,
  options?: {
    userContext?: Partial<UserContextType>;
    queryClient?: QueryClient;
    renderOptions?: Omit<RenderOptions, 'wrapper'>;
  }
) {
  const queryClient = options?.queryClient ?? createTestQueryClient();
  const userContext = createMockUserContext(options?.userContext);

  // Create router with all providers
  const router = createMemoryRouter(
    [ { path: '/', element: ui } ],
    { initialEntries: ['/'] }
  );

  const AllProviders = ({ children }: { children: React.ReactNode }) => (
    <QueryClientProvider client={queryClient}>
      <UserContext.Provider value={userContext}>
        {children}
      </UserContext.Provider>
    </QueryClientProvider>
  );

  return {
    user: userEvent.setup(),
    queryClient,
    userContext,
    ...render(
      <AllProviders>
        <RouterProvider router={router} />
      </AllProviders>,
      options?.renderOptions
    ),
  };
}

export function mockLogger() {
  return {
    logger: {
      error: vi.fn(),
      log: vi.fn(),
    },
  };
}

// NOTE: I know some prefer re-exporting userEvent and/or everything from 
// @testing-library/react, but I'd prefer to explicitly import in my test files 
// so I can tell what is a custom utility
