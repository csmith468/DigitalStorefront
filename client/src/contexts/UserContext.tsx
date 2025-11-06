// State: user, isAuthenticated, isLoading
// Actions: login(dto), register(dto), logout()

import React, { createContext, useState, useEffect, useMemo, useCallback } from "react";
import type { ReactNode } from "react";
import type { User, Auth, LoginRequest, RegisterRequest } from "../types/auth";
import { authService } from "../services/auth";
import { AuthModal } from "../components/auth/AuthModal";
import toast from "react-hot-toast";
import { useSearchParams } from "react-router-dom";
import { ErrorMessages } from "../constants/messages";

export interface UserContextType {
  user: User | null;
  isAuthenticated: boolean;
  roles: string[];
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
  const [roles, setRoles] = useState<string[]>([]);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [authModalOpen, setAuthModalOpen] = useState(false);
  const [searchParams, setSearchParams] = useSearchParams();
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
    const authParam = searchParams.get('auth');
    const reasonParam = searchParams.get('reason');

    if (authParam === 'login' || authParam === 'register') {
      openAuthModal(authParam);
      if (reasonParam === 'session-expired')
        toast.error(ErrorMessages.Auth.sessionExpired);

      setSearchParams({});
    }
  }, [searchParams, setSearchParams]);

  const setUserAndTokenFromAuthResponse = useCallback((response: Auth) => {
    authService.setStoredToken(response.token);
    setUser({ userId: response.userId, username: response.username });
    setRoles(response.roles);
  }, []);

  const login = useCallback(async (dto: LoginRequest) => {
    const response = await authService.login(dto);
    setUserAndTokenFromAuthResponse(response);
  }, [setUserAndTokenFromAuthResponse]);

  const register = useCallback(async (dto: RegisterRequest) => {
    const response = await authService.register(dto);
    setUserAndTokenFromAuthResponse(response);
  }, [setUserAndTokenFromAuthResponse]);

  const logout = useCallback(() => {
    authService.logout();
    setUser(null);
    window.location.href = '/';
  }, []);

  const openAuthModal = useCallback((mode: 'login' | 'register') => {
    setAuthMode(mode);
    setAuthModalOpen(true);
  }, []);

  const value = useMemo<UserContextType>(() => ({
    user,
    isAuthenticated: !!user,
    roles,
    isLoading,
    login,
    register,
    logout,
    openAuthModal,
    closeAuthModal: () => setAuthModalOpen(false),
  }), [user, roles, isLoading, login, register, logout, openAuthModal]);

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
