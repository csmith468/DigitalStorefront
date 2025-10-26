export interface Auth {
  userId: number;
  username: string;
  token: string;
  roles: string[];
}

export interface LoginRequest {
  username: string;
  password: string;
}

export interface RegisterRequest {
  username: string;
  password: string;
  confirmPassword: string;
  firstName?: string;
  lastName?: string;
  email?: string;
}

export interface User {
  userId: number;
  username: string;
}