import { useState } from "react";
import { useUser } from "../../contexts/useUser";
import { FormInput } from "../primitives/FormInput";
import { FormShell } from "../primitives/FormShell";

interface LoginFormProps {
  onSuccess: () => void;
  onCancel: () => void;
  onSwitchToRegister: () => void;
}

export function LoginForm ({
  onSuccess,
  onCancel,
  onSwitchToRegister,
}: LoginFormProps) {
  const { login } = useUser();
  const [serverError, setServerError] = useState<string | null>(null);

  const onSubmit = async (form: { username: string; password: string }) => {
    try {
      setServerError(null);
      await login({ username: form.username, password: form.password });
      onSuccess();
    } catch (error: any) {
      const message = error.response?.data || "Invalid username or password";
      setServerError(message);
    }
  };

  const validate = (data: {
    username: string;
    password: string;
  }): string | null => {
    setServerError(null);

    if (!data.username.trim()) return "Username is required";
    if (!data.password) return "Password is required";
    return null;
  };

  return (
    <FormShell<{ username: string; password: string }>
      initial={{ username: "", password: "" }}
      onSubmit={onSubmit}
      onCancel={onCancel}
      validate={validate}
      submitText="Login"
      enableUnsavedChangesWarning={false}
      >
      {({ data, updateField }) => (
        <>
          <FormInput
            id="username"
            label="Username"
            required
            value={data.username}
            onChange={(f, v) => updateField(f, v)}
          />
          <FormInput
            id="password"
            label="Password"
            type="password"
            required
            value={data.password}
            onChange={(f, v) => updateField(f, v)}
          />

          <div className="text-center text-sm">
            <span className="text-gray-600">Don't have an account? </span>
            <button
              type="button"
              onClick={onSwitchToRegister}
              className="text-blue-600 hover:text-blue-500 font-medium">
              Register
            </button>
          </div>
          
          {serverError && (
            <div className="bg-red-50 border border-red-200 text-red-800 px-4 py-3 rounded">
              {serverError}
            </div>
          )}
        </>
      )}
    </FormShell>
  );
};