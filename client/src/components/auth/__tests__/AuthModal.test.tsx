import { describe, it, expect, vi } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import { renderWithProviders } from '../../../tests/test-utils';
import { AuthModal } from '../AuthModal';

// Mock the child forms to isolate AuthModal testing
vi.mock('../LoginForm', () => ({
  LoginForm: ({ onSuccess, onCancel, onSwitchToRegister }: any) => (
    <div data-testid="login-form">
      <button onClick={onSuccess}>Login Success</button>
      <button onClick={onCancel}>Cancel</button>
      <button onClick={onSwitchToRegister}>Switch to Register</button>
    </div>
  ),
}));

vi.mock('../RegisterForm', () => ({
  RegisterForm: ({ onSuccess, onCancel, onSwitchToLogin }: any) => (
    <div data-testid="register-form">
      <button onClick={onSuccess}>Register Success</button>
      <button onClick={onCancel}>Cancel</button>
      <button onClick={onSwitchToLogin}>Switch to Login</button>
    </div>
  ),
}));

describe('AuthModal', () => {
  describe('Rendering', () => {
    it('does not render when isOpen is false', () => {
      renderWithProviders(
        <AuthModal isOpen={false} onClose={vi.fn()} />
      );

      expect(screen.queryByTestId('login-form')).not.toBeInTheDocument();
      expect(screen.queryByTestId('register-form')).not.toBeInTheDocument();
    });

    it('renders login form by default when isOpen is true', () => {
      renderWithProviders(
        <AuthModal isOpen={true} onClose={vi.fn()} />
      );

      expect(screen.getByTestId('login-form')).toBeInTheDocument();
      expect(screen.queryByTestId('register-form')).not.toBeInTheDocument();
    });

    it('renders with "Sign In" title for login mode', () => {
      renderWithProviders(
        <AuthModal isOpen={true} onClose={vi.fn()} initialMode="login" />
      );

      expect(screen.getByText('Sign In')).toBeInTheDocument();
    });

    it('renders register form when initialMode is register', () => {
      renderWithProviders(
        <AuthModal isOpen={true} onClose={vi.fn()} initialMode="register" />
      );

      expect(screen.getByTestId('register-form')).toBeInTheDocument();
      expect(screen.queryByTestId('login-form')).not.toBeInTheDocument();
    });

    it('renders with "Create Account" title for register mode', () => {
      renderWithProviders(
        <AuthModal isOpen={true} onClose={vi.fn()} initialMode="register" />
      );

      expect(screen.getByText('Create Account')).toBeInTheDocument();
    });
  });

  describe('Mode Switching', () => {
    it('switches from login to register when onSwitchToRegister is called', async () => {
      const { user } = renderWithProviders(
        <AuthModal isOpen={true} onClose={vi.fn()} initialMode="login" />
      );

      expect(screen.getByTestId('login-form')).toBeInTheDocument();

      await user.click(screen.getByRole('button', { name: /switch to register/i }));

      expect(screen.getByTestId('register-form')).toBeInTheDocument();
      expect(screen.queryByTestId('login-form')).not.toBeInTheDocument();
    });

    it('switches from register to login when onSwitchToLogin is called', async () => {
      const { user } = renderWithProviders(
        <AuthModal isOpen={true} onClose={vi.fn()} initialMode="register" />
      );

      expect(screen.getByTestId('register-form')).toBeInTheDocument();

      await user.click(screen.getByRole('button', { name: /switch to login/i }));

      expect(screen.getByTestId('login-form')).toBeInTheDocument();
      expect(screen.queryByTestId('register-form')).not.toBeInTheDocument();
    });
  });

  describe('Close Behavior', () => {
    it('calls onClose when cancel is clicked', async () => {
      const onClose = vi.fn();
      const { user } = renderWithProviders(
        <AuthModal isOpen={true} onClose={onClose} />
      );

      await user.click(screen.getByRole('button', { name: /cancel/i }));

      expect(onClose).toHaveBeenCalled();
    });

    it('calls onClose when login succeeds', async () => {
      const onClose = vi.fn();
      const { user } = renderWithProviders(
        <AuthModal isOpen={true} onClose={onClose} />
      );

      await user.click(screen.getByRole('button', { name: /login success/i }));

      expect(onClose).toHaveBeenCalled();
    });

    it('calls onClose when register succeeds', async () => {
      const onClose = vi.fn();
      const { user } = renderWithProviders(
        <AuthModal isOpen={true} onClose={onClose} initialMode="register" />
      );

      await user.click(screen.getByRole('button', { name: /register success/i }));

      expect(onClose).toHaveBeenCalled();
    });
  });

  describe('Mode Reset', () => {
    it('resets to initial mode when modal reopens', async () => {
      const { user, rerender } = renderWithProviders(
        <AuthModal isOpen={true} onClose={vi.fn()} initialMode="login" />
      );

      await user.click(screen.getByRole('button', { name: /switch to register/i }));
      expect(screen.getByTestId('register-form')).toBeInTheDocument();

      rerender(<AuthModal isOpen={false} onClose={vi.fn()} initialMode="login" />);
      rerender(<AuthModal isOpen={true} onClose={vi.fn()} initialMode="login" />);

      await waitFor(() => { expect(screen.getByTestId('login-form')).toBeInTheDocument(); });
    });
  });
});
