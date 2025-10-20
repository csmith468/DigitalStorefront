import { useState } from "react";
import { useUser } from "../../contexts/UserContext";
import type { RegisterRequest } from "../../types/auth";
import { FormInput } from "../primitives/FormInput";

interface RegisterFormProps {
  onSuccess: () => void;
  onSwitchToLogin: () => void;
}

export const RegisterForm: React.FC<RegisterFormProps> = ({ onSuccess, onSwitchToLogin }) => {
  const { register } = useUser();

  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [firstName, setFirstName] = useState('');
  const [lastName, setLastName] = useState('');
  const [email, setEmail] = useState('');
  const [error, setError] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setIsSubmitting(true);

    try {
      const dto: RegisterRequest = {
        username: username,
        password: password,
        confirmPassword: confirmPassword,
        firstName: firstName != '' ? firstName : undefined,
        lastName: lastName != '' ? lastName : undefined,
        email: email != '' ? email : undefined
      };
      await register(dto);
      onSuccess();
    } catch (err: any) {
      setError(err.response?.data || 'Registration failed. Please try again.');
    } finally {
      setIsSubmitting(false);
    }
  };

  const divRowClassName: string = "grid grid-cols-1 md:grid-cols-2 gap-4";
  
  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      {/* Error Message */}
      {error && (
        <div className="rounded-md bg-red-50 p-3">
            <p className="text-sm text-red-800">{error}</p>
          </div>
      )}

      {/* Username & Password Inputs */}
      <div className={divRowClassName}>
        <FormInput id="username" label="Username" required value={username} onChange={setUsername} />
        <FormInput id="email" label="Email" type="email" value={email} onChange={setEmail} />
      </div>
      <div className={divRowClassName}>
        <FormInput id="password" label="Password" type="password" required value={password} onChange={setPassword} />
        <FormInput id="confirmPassword" label="Confirm Password" type="password" required value={confirmPassword} onChange={setConfirmPassword} />
      </div>
      <div className={divRowClassName}>
        <FormInput id="firstName" label="First Name" value={firstName} onChange={setFirstName} />
        <FormInput id="lastName" label="Last Name" value={lastName} onChange={setLastName} />
      </div>

      {/* Submit */}
      <button type="submit" disabled={isSubmitting} 
        className="w-full py-2 px-4 rounded-md shadow-sm text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 disabled:opacity-50 disabled:cursor-not-allowed"
      >{isSubmitting ? "Creating Account..." : "Create Account"}</button>

      {/* Switch to Login */}
      <div className="text-center text-sm">
        <span className="text-gray-600">Already have an account? </span>
        <button type="button" onClick={onSwitchToLogin} className="text-blue-600 hover:text-blue-500 font-medium">Login</button>
      </div>
    </form>
  );
};