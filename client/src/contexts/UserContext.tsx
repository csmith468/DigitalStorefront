// State: user, isAuthenticated, isLoading
// Actions: login(dto), register(dto), logout()

import React, { createContext, useState, useEffect } from "react";
import type { ReactNode } from "react";
import type { User, Auth, LoginRequest, RegisterRequest } from "../types/auth";
import { authService } from "../services/auth";
import { AuthModal } from "../components/auth/AuthModal";
import toast from "react-hot-toast";

export interface UserContextType {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (dto: LoginRequest) => Promise<void>;
  register: (dto: RegisterRequest) => Promise<void>;
  logout: () => void;
  openAuthModal: (mode: 'login' | 'register') => void;
  closeAuthModal: () => void;
}

export const UserContext = createContext<UserContextType | undefined>(undefined); // store

export const UserProvider: React.FC<{ children: ReactNode }> = ({
  children,
}) => {
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [authModalOpen, setAuthModalOpen] = useState(false);
  const [authMode, setAuthMode] = useState<'login' | 'register'>('login');

  useEffect(() => {
    const initAuth = async () => {
      const token = authService.getStoredToken();
      if (token) {
        try {
          const response = await authService.refreshToken();
          setUserAndTokenFromAuthResponse(response);
        } catch (error) {
          authService.logout();
        }
      }

      setIsLoading(false);
    };
    initAuth();
  }, []);

  useEffect(() => {
    const params = new URLSearchParams(window.location.search);
    const authParam = params.get('auth');
    const reasonParam = params.get('reason');

    if (authParam === 'login' || authParam === 'register') {
      openAuthModal(authParam);
      if (reasonParam === 'session-expired')
        toast.error('Your session has expired. Please log in again.');

      window.history.replaceState({}, '', '/');
    }
  }, []);

  const setUserAndTokenFromAuthResponse = (response: Auth) => {
    authService.setStoredToken(response.token);
    setUser({ userId: response.userId, username: response.username });
  };

  const login = async (dto: LoginRequest) => {
    const response = await authService.login(dto);
    setUserAndTokenFromAuthResponse(response);
  };

  const register = async (dto: RegisterRequest) => {
    const response = await authService.register(dto);
    setUserAndTokenFromAuthResponse(response);
  };

  const logout = () => {
    authService.logout();
    setUser(null);
    window.location.href = '/';
  };

  const openAuthModal = (mode: 'login' | 'register') => {
    setAuthMode(mode);
    setAuthModalOpen(true);
  };

  const value: UserContextType = {
    user,
    isAuthenticated: !!user,
    isLoading,
    login,
    register,
    logout,
    openAuthModal,
    closeAuthModal: () => setAuthModalOpen(false),
  };

  return (
    <UserContext.Provider value={value}>
      {children}
      <AuthModal
        isOpen={authModalOpen}
        onClose={() => setAuthModalOpen(false)}
        initialMode={authMode}
      />
    </UserContext.Provider>
  );
};
