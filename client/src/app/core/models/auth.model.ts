export interface LoginRequest {
  email: string;
  password: string;
  provider?: string;
}

export interface RegisterRequest {
  name: string;
  alias?: string;
  email: string;
  password: string;
}

export interface AuthResponse {
  token?: string | null;
  expiresAt?: string | null;
  requiresMfa: boolean;
  mfaSessionId?: string | null;
}

export interface RegisterResponse {
  userId: string;
}

export interface UserProfile {
  id: string;
  email: string;
  name: string;
  alias?: string | null;
  roleName: string;
  isActive: boolean;
  lastLoginDate?: string | null;
}
