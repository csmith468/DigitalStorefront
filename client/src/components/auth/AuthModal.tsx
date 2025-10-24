import { useState, useEffect } from 'react';
import { Modal } from '../primitives/Modal';
import { LoginForm } from './LoginForm';
import { RegisterForm } from './RegisterForm';

type AuthMode = 'login' | 'register';

interface AuthModalProps {
  isOpen: boolean;
  onClose: () => void;
  initialMode?: AuthMode;
}

export const AuthModal: React.FC<AuthModalProps> = ({
  isOpen,
  onClose,
  initialMode = "login",
}) => {
  const [mode, setMode] = useState<AuthMode>(initialMode);

  useEffect(() => {
    if (isOpen) setMode(initialMode);
  }, [isOpen, initialMode]);

  const handleClose = () => {
    setMode(initialMode);
    onClose();
  };

  const handleSuccess = () => {
    setMode(initialMode);
    onClose();
  };

  const title = (mode === 'login' ? 'Sign In' : 'Create Account');

  return (
    <Modal isOpen={isOpen} onClose={onClose} title={title}>
      {mode === "login" ? (
        <LoginForm
          onSuccess={handleSuccess}
          onCancel={handleClose}
          onSwitchToRegister={() => setMode("register")}
        />
      ) : (
        <RegisterForm
          onSuccess={handleSuccess}
          onCancel={handleClose}
          onSwitchToLogin={() => setMode("login")}
        />
      )}
    </Modal>
  );
};