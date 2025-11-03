import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, type RenderOptions } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import type { ReactElement } from "react";
import { createMemoryRouter, RouterProvider } from "react-router-dom";
import { vi } from "vitest";

export function renderWithRouter(
  ui: ReactElement,
  options?: Omit<RenderOptions, 'wrapper'>
) {
  // needed to support useBlocker
  const router = createMemoryRouter(
    [ { path: '/', element: ui } ],
    { initialEntries: ['/'] }
  );

  return {
    user: userEvent.setup(),
    ...render(<RouterProvider router={router} />, options)
  };
};

export function createQueryWrapper() {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false },
      mutations: { retry: false },
    },
  });
    
  const Wrapper = ({ children }: { children: React.ReactNode }) => (
    <QueryClientProvider client={queryClient}>
      {children}
    </QueryClientProvider>
  );

  return Wrapper;
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