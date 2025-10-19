import { useState } from "react";
import { useUser } from "../../contexts/UserContext";
import { FormInput } from "../primitives/FormInput";

interface LoginFormProps {
  onSuccess: () => void;
  onSwitchToRegister: () => void;
}

export const LoginForm: React.FC<LoginFormProps> = ({ onSuccess, onSwitchToRegister }) => {
  const { login } = useUser();

  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setIsSubmitting(true);

    try {
      await login({ username, password });
      onSuccess();
    } catch (err: any) {
      setError(err.response?.data || 'Login failed. Please try again.');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      {/* Error Message */}
      {error && (
        <div className="rounded-md bg-red-50 p-3">
            <p className="text-sm text-red-800">{error}</p>
          </div>
      )}

      {/* Username & Password Inputs */}
      
      <FormInput id="username" label="Username" required value={username} onChange={setUsername} />
      <FormInput id="password" label="Password" type="password" required value={password} onChange={setPassword} />

      {/* Submit */}
      <button type="submit" disabled={isSubmitting} 
        className="w-full py-2 px-4 rounded-md shadow-sm text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 disabled:opacity-50 disabled:cursor-not-allowed"
      >{isSubmitting ? "Signing In..." : "Sign In"}</button>

      {/* Switch to Register */}
      <div className="text-center text-sm">
        <span className="text-gray-600">Don't have an account? </span>
        <button type="button" onClick={onSwitchToRegister} className="text-blue-600 hover:text-blue-500 font-medium">Register</button>
      </div>
    </form>
  );
};