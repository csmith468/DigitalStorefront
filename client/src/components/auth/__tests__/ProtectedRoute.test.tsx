import { describe, it, expect, vi } from 'vitest';
import { screen } from '@testing-library/react';
import { render } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { ProtectedRoute } from '../ProtectedRoute';
import { UserContext, type UserContextType } from '../../../contexts/UserContext';
import { createMockUserContext } from '../../../tests/fixtures';

const PROTECTED_CONTENT = 'Protected Content';

function renderProtectedRoute(contextOverrides: Partial<UserContextType> = {}) {
  const context = createMockUserContext(contextOverrides);
  return {
    ...render(
      <UserContext.Provider value={context}>
        <ProtectedRoute>
          <div>{PROTECTED_CONTENT}</div>
        </ProtectedRoute>
      </UserContext.Provider>
    ),
    context,
  };
}

describe('ProtectedRoute', () => {
  it('shows loading screen when authentication is loading', () => {
    renderProtectedRoute({ isLoading: true });

    expect(screen.getByText(/loading/i)).toBeInTheDocument();
    expect(screen.queryByText(PROTECTED_CONTENT)).not.toBeInTheDocument();
  });

  it('shows authentication message when user is not authenticated', () => {
    renderProtectedRoute({ isAuthenticated: false });

    expect(screen.getByText(/authentication required/i)).toBeInTheDocument();
    expect(screen.getByText(/you need to be logged in/i)).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /sign in/i })).toBeInTheDocument();
    expect(screen.queryByText(PROTECTED_CONTENT)).not.toBeInTheDocument();
  });

  it('renders children when user is authenticated', () => {
    renderProtectedRoute({
      isAuthenticated: true,
      user: { userId: 1, username: 'testuser' },
    });

    expect(screen.getByText(PROTECTED_CONTENT)).toBeInTheDocument();
    expect(screen.queryByText(/authentication required/i)).not.toBeInTheDocument();
  });

  it('calls openAuthModal when sign in button is clicked', async () => {
    const user = userEvent.setup();
    const openAuthModal = vi.fn();

    renderProtectedRoute({ openAuthModal });

    await user.click(screen.getByRole('button', { name: /sign in/i }));

    expect(openAuthModal).toHaveBeenCalledWith('login');
  });

  it('does not render children while loading', () => {
    renderProtectedRoute({
      isAuthenticated: true,
      isLoading: true,
    });

    expect(screen.getByText(/loading/i)).toBeInTheDocument();
    expect(screen.queryByText(PROTECTED_CONTENT)).not.toBeInTheDocument();
  });

  it('handles transition from loading to authenticated', () => {
    const { rerender } = renderProtectedRoute({ isLoading: true });

    expect(screen.getByText(/loading/i)).toBeInTheDocument();

    rerender(
      <UserContext.Provider value={createMockUserContext({
        isAuthenticated: true,
        user: { userId: 1, username: 'testuser' },
      })}>
        <ProtectedRoute>
          <div>{PROTECTED_CONTENT}</div>
        </ProtectedRoute>
      </UserContext.Provider>
    );

    expect(screen.queryByText(/loading/i)).not.toBeInTheDocument();
    expect(screen.getByText(PROTECTED_CONTENT)).toBeInTheDocument();
  });
});