import { useUser } from "../../hooks/useUser";
import type { RegisterRequest } from "../../types/auth";
import { FormInput } from "../primitives/FormInput";
import { FormShell } from "../primitives/FormShell";

interface RegisterFormProps {
  onSuccess: () => void;
  onCancel: () => void;
  onSwitchToLogin: () => void;
}

export function RegisterForm ({
  onSuccess,
  onCancel,
  onSwitchToLogin,
}: RegisterFormProps) {
  const { register } = useUser();

  const initial: RegisterRequest = {
    username: '',
    password: '',
    confirmPassword: '',
    firstName: '',
    lastName: '',
    email: ''
  };

  const onSubmit = async (form: RegisterRequest) => {
    const dto: RegisterRequest = {
      username: form.username,
      password: form.password,
      confirmPassword: form.confirmPassword,
      firstName: form.firstName != '' ? form.firstName : undefined,
      lastName: form.lastName != '' ? form.lastName : undefined,
      email: form.email != '' ? form.email : undefined
    };
    await register(dto);
    onSuccess();
  };

  const validate = (form: RegisterRequest): string | null => {
    if (!form.username.trim()) return "Username is required";
    if (!form.password) return "Password is required";
    if (!form.confirmPassword) return "Please confirm your password";
    if (form.password !== form.confirmPassword) return "Passwords do not match";
    if (form.email && !/^\S+@\S+\.\S+$/.test(form.email))
      return "Enter a valid email";
    return null;
  };

  const row: string = "grid grid-cols-1 md:grid-cols-2 gap-4";

  return (
    <FormShell<RegisterRequest>
      initial={initial}
      validate={validate}
      onSubmit={onSubmit}
      onCancel={onCancel}
      submitText="Register">
      {({ data: form, updateField }) => (
        <>
          <div className={row}>
            <FormInput
              id="username"
              label="Username"
              required
              value={form.username}
              onChange={(field, value) => updateField(field, value)}
            />
            <FormInput
              id="email"
              label="Email"
              type="email"
              value={form.email ?? ""}
              onChange={(field, value) => updateField(field, value)}
            />
          </div>
          <div className={row}>
            <FormInput
              id="password"
              label="Password"
              type="password"
              required
              value={form.password}
              onChange={(field, value) => updateField(field, value)}
            />
            <FormInput
              id="confirmPassword"
              label="Confirm Password"
              type="password"
              required
              value={form.confirmPassword}
              onChange={(field, value) => updateField(field, value)}
            />
          </div>
          <div className={row}>
            <FormInput
              id="firstName"
              label="First Name"
              value={form.firstName ?? ""}
              onChange={(field, value) => updateField(field, value)}
            />
            <FormInput
              id="lastName"
              label="Last Name"
              value={form.lastName ?? ""}
              onChange={(field, value) => updateField(field, value)}
            />
          </div>

          <div className="text-center text-sm">
            <span className="text-gray-600">Already have an account? </span>
            <button
              type="button"
              onClick={onSwitchToLogin}
              className="text-blue-600 hover:text-blue-500 font-medium">
              Login
            </button>
          </div>
        </>
      )}
    </FormShell>
  );
};
