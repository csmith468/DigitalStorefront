export interface AuthResponse {
  userId: number;
  username: string;
  token: string;
}

export interface LoginDto {
  username: string;
  password: string;
}

export interface RegisterDto {
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