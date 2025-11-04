import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { RegisterForm } from '../RegisterForm';
import { UserContext } from '../../../contexts/UserContext';
import { createMockUserContext, mockRegisterRequest } from '../../../tests/fixtures';

describe('RegisterForm', () => {
  const mockOnSuccess = vi.fn();
  const mockOnCancel = vi.fn();
  const mockOnSwitchToLogin = vi.fn();
  const mockRegister = vi.fn();

  beforeEach(() => {
    vi.clearAllMocks();
  });

  function renderRegisterForm() {
    const context = createMockUserContext({ register: mockRegister });

    return render(
      <UserContext.Provider value={context}>
        <RegisterForm
          onSuccess={mockOnSuccess}
          onCancel={mockOnCancel}
          onSwitchToLogin={mockOnSwitchToLogin}
        />
      </UserContext.Provider>
    );
  }

  it('renders register form with all fields', () => {
    renderRegisterForm();

    expect(screen.getByLabelText(/^username/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/^email/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/^password/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/^confirm password/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/^first name/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/^last name/i)).toBeInTheDocument();
  });

  it('validates password confirmation matches', async () => {
    const user = userEvent.setup();
    renderRegisterForm();

    await user.type(screen.getByLabelText(/^username/i), mockRegisterRequest.username);
    await user.type(screen.getByLabelText(/^password/i), mockRegisterRequest.password);
    await user.type(screen.getByLabelText(/^confirm password/i), 'differentPassword');

    await user.click(screen.getByRole('button', { name: /register/i }));

    expect(mockRegister).not.toHaveBeenCalled();
  });

  it('validates email format when provided', async () => {
    const user = userEvent.setup();
    renderRegisterForm();

    await user.type(screen.getByLabelText(/^username/i), mockRegisterRequest.username);
    await user.type(screen.getByLabelText(/^password/i), mockRegisterRequest.password);
    await user.type(screen.getByLabelText(/^confirm password/i), mockRegisterRequest.confirmPassword);
    await user.type(screen.getByLabelText(/^email/i), 'invalid-email');

    await user.click(screen.getByRole('button', { name: /register/i }));

    expect(mockRegister).not.toHaveBeenCalled();
  });

  it('registers successfully with required fields only', async () => {
    const user = userEvent.setup();
    mockRegister.mockResolvedValue(undefined);
    renderRegisterForm();

    await user.type(screen.getByLabelText(/^username/i), mockRegisterRequest.username);
    await user.type(screen.getByLabelText(/^password/i), mockRegisterRequest.password);
    await user.type(screen.getByLabelText(/^confirm password/i), mockRegisterRequest.confirmPassword);

    await user.click(screen.getByRole('button', { name: /register/i }));

    await waitFor(() => {
      expect(mockRegister).toHaveBeenCalledWith({
        username: mockRegisterRequest.username,
        password: mockRegisterRequest.password,
        confirmPassword: mockRegisterRequest.confirmPassword,
        firstName: undefined,
        lastName: undefined,
        email: undefined,
      });
    });

    expect(mockOnSuccess).toHaveBeenCalled();
  });

  it('registers successfully with all fields including optional ones', async () => {
    const user = userEvent.setup();
    mockRegister.mockResolvedValue(undefined);
    renderRegisterForm();

    await user.type(screen.getByLabelText(/^username/i), mockRegisterRequest.username);
    await user.type(screen.getByLabelText(/^email/i), mockRegisterRequest.email!);
    await user.type(screen.getByLabelText(/^password/i), mockRegisterRequest.password);
    await user.type(screen.getByLabelText(/^confirm password/i), mockRegisterRequest.confirmPassword);
    await user.type(screen.getByLabelText(/first name/i), mockRegisterRequest.firstName!);
    await user.type(screen.getByLabelText(/last name/i), mockRegisterRequest.lastName!);

    await user.click(screen.getByRole('button', { name: /register/i }));

    await waitFor(() => {
      expect(mockRegister).toHaveBeenCalledWith({
        username: mockRegisterRequest.username,
        password: mockRegisterRequest.password,
        confirmPassword: mockRegisterRequest.confirmPassword,
        firstName: mockRegisterRequest.firstName,
        lastName: mockRegisterRequest.lastName,
        email: mockRegisterRequest.email,
      });
    });

    expect(mockOnSuccess).toHaveBeenCalled();
  });

  it('shows server error when registration fails', async () => {
    const user = userEvent.setup();
    mockRegister.mockRejectedValue({
      response: { data: 'Username already exists' },
    });
    renderRegisterForm();

    await user.type(screen.getByLabelText(/^username/i), mockRegisterRequest.username);
    await user.type(screen.getByLabelText(/^password/i), mockRegisterRequest.password);
    await user.type(screen.getByLabelText(/^confirm password/i), mockRegisterRequest.confirmPassword);

    await user.click(screen.getByRole('button', { name: /register/i }));

    expect(await screen.findByText(/username already exists/i)).toBeInTheDocument();
    expect(mockOnSuccess).not.toHaveBeenCalled();
  });

  it('calls onSwitchToLogin when login link is clicked', async () => {
    const user = userEvent.setup();
    renderRegisterForm();

    const loginButton = screen.getByRole('button', { name: /login/i });
    await user.click(loginButton);

    expect(mockOnSwitchToLogin).toHaveBeenCalled();
  });
});