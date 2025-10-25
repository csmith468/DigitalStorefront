import { useUser } from "../../hooks/useUser";
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

  const onSubmit = async (form: { username: string; password: string }) => {
    await login({ username: form.username, password: form.password });
    onSuccess();
  };

  const validate = (data: {
    username: string;
    password: string;
  }): string | null => {
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
        </>
      )}
    </FormShell>
  );
};