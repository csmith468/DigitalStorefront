import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { LoginForm } from '../LoginForm';
import { UserContext } from '../../../contexts/UserContext';
import { createMockUserContext } from '../../../tests/fixtures';

const username = 'testUser';
const password = 'password123';

describe('LoginForm', () => {
  const mockOnSuccess = vi.fn();
  const mockOnCancel = vi.fn();
  const mockOnSwitchToRegister = vi.fn();
  const mockLogin = vi.fn();

  beforeEach(() => {
    vi.clearAllMocks();
  });

  function renderLoginForm() {
    const context = createMockUserContext({ login: mockLogin });

    return render(
      <UserContext.Provider value={context}>
        <LoginForm
          onSuccess={mockOnSuccess}
          onCancel={mockOnCancel}
          onSwitchToRegister={mockOnSwitchToRegister}
        />
      </UserContext.Provider>
    );
  }

  it('renders login form with username and password fields', () => {
    renderLoginForm();

    expect(screen.getByLabelText(/username/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/password/i)).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /login/i })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /cancel/i })).toBeInTheDocument();
  });

  it('does not submit when username is empty', async () => {
    const user = userEvent.setup();
    renderLoginForm();

    const passwordInput = screen.getByLabelText(/password/i);
    await user.type(passwordInput, password);

    const submitButton = screen.getByRole('button', { name: /login/i });
    await user.click(submitButton);

    expect(mockLogin).not.toHaveBeenCalled();
  });

  it('does not submit when password is empty', async () => {
    const user = userEvent.setup();
    renderLoginForm();

    const usernameInput = screen.getByLabelText(/username/i);
    await user.type(usernameInput, username);

    const submitButton = screen.getByRole('button', { name: /login/i });
    await user.click(submitButton);

    expect(mockLogin).not.toHaveBeenCalled();
  });

  it('allows form submission when both fields are filled', async () => {
    const user = userEvent.setup();
    mockLogin.mockResolvedValue(undefined);
    renderLoginForm();

    const usernameInput = screen.getByLabelText(/username/i);
    const passwordInput = screen.getByLabelText(/password/i);

    await user.type(usernameInput, username);
    await user.type(passwordInput, password);

    const submitButton = screen.getByRole('button', { name: /login/i });
    await user.click(submitButton);

    await waitFor(() => {
      expect(mockLogin).toHaveBeenCalledWith({
        username: username,
        password: password,
      });
    });

    expect(mockOnSuccess).toHaveBeenCalled();
  });

  it('calls login and onSuccess when form is submitted successfully', async () => {
    const user = userEvent.setup();
    mockLogin.mockResolvedValue(undefined);
    renderLoginForm();

    const usernameInput = screen.getByLabelText(/username/i);
    const passwordInput = screen.getByLabelText(/password/i);
    const submitButton = screen.getByRole('button', { name: /login/i });

    await user.type(usernameInput, username);
    await user.type(passwordInput, password);
    await user.click(submitButton);

    await waitFor(() => {
      expect(mockLogin).toHaveBeenCalledWith({
        username: username,
        password: password,
      });
    });

    expect(mockOnSuccess).toHaveBeenCalled();
  });

  it('shows server error when login fails', async () => {
    const user = userEvent.setup();
    mockLogin.mockRejectedValue({
      response: { data: 'Invalid credentials' },
    });
    renderLoginForm();

    const usernameInput = screen.getByLabelText(/username/i);
    const passwordInput = screen.getByLabelText(/password/i);
    const submitButton = screen.getByRole('button', { name: /login/i });

    await user.type(usernameInput, 'wronguser');
    await user.type(passwordInput, 'wrongpass');
    await user.click(submitButton);

    expect(await screen.findByText(/invalid credentials/i)).toBeInTheDocument();
    expect(mockOnSuccess).not.toHaveBeenCalled();
  });

  it('shows default error message when login fails without response data', async () => {
    const user = userEvent.setup();
    mockLogin.mockRejectedValue(new Error('Network error'));
    renderLoginForm();

    const usernameInput = screen.getByLabelText(/username/i);
    const passwordInput = screen.getByLabelText(/password/i);
    const submitButton = screen.getByRole('button', { name: /login/i });

    await user.type(usernameInput, username);
    await user.type(passwordInput, password);
    await user.click(submitButton);

    expect(await screen.findByText(/invalid username or password/i)).toBeInTheDocument();
    expect(mockOnSuccess).not.toHaveBeenCalled();
  });

  it('calls onCancel when cancel button is clicked', async () => {
    const user = userEvent.setup();
    renderLoginForm();

    const cancelButton = screen.getByRole('button', { name: /cancel/i });
    await user.click(cancelButton);

    expect(mockOnCancel).toHaveBeenCalled();
  });

  it('calls onSwitchToRegister when register link is clicked', async () => {
    const user = userEvent.setup();
    renderLoginForm();

    const registerButton = screen.getByRole('button', { name: /register/i });
    await user.click(registerButton);

    expect(mockOnSwitchToRegister).toHaveBeenCalled();
  });

  it('shows validation error when username is only whitespace', async () => {
    const user = userEvent.setup();
    renderLoginForm();

    const usernameInput = screen.getByLabelText(/username/i);
    const passwordInput = screen.getByLabelText(/password/i);

    await user.type(usernameInput, '    ');
    await user.type(passwordInput, password);

    const submitButton = screen.getByRole('button', { name: /login/i });
    await user.click(submitButton);

    expect(mockLogin).not.toHaveBeenCalled();
  });
});