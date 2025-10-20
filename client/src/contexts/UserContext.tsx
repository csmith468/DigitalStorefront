// State: user, isAuthenticated, isLoading
// Actions: login(dto), register(dto), logout()

import React, { createContext, useContext, useState, useEffect } from 'react';
import type { ReactNode } from 'react';
import type { User, Auth, LoginRequest, RegisterRequest } from '../types/auth';
import { authService } from '../services/auth';


interface UserContextType {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (dto: LoginRequest) => Promise<void>;
  register: (dto: RegisterRequest) => Promise<void>;
  logout: () => void;
}

const UserContext = createContext<UserContextType | undefined>(undefined);

export const UserProvider: React.FC<{ children: ReactNode }>  = ({ children }) => {
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(true);

  // like created()
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
    }
    initAuth();
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

  const value: UserContextType = {
    user,
    isAuthenticated: !!user,
    isLoading,
    login,
    register,
    logout
  };

  return <UserContext.Provider value={value}>{children}</UserContext.Provider>
};

export const useUser = (): UserContextType => {
  const context = useContext(UserContext);
  if (!context)
    throw new Error('useUser must be used within UserProvider.');
  return context;
}