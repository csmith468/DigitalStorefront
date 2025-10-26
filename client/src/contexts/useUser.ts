import { useContext } from "react";
import { UserContext, type UserContextType } from "./UserContext";

// NOTE: Could move to hooks but I feel like it should go with UserContext since 
// it's a Context API wrapper and this is the only non-React Query non-utility hook

interface UseUserReturn extends UserContextType {
  hasRole: (roleName: string) => boolean;
  isAdmin: () => boolean;
  canManageProducts: () => boolean;
  canManageImages: () => boolean;
}

export const useUser = (): UseUserReturn => {
  const context = useContext(UserContext);
  if (!context) throw new Error("useUser must be used within UserProvider.");

  const hasRole = (roleName: string) => {
    return context.roles?.includes(roleName) ?? false;
  };

  const isAdmin = () => hasRole('Admin');
  const canManageProducts = () => hasRole('Admin') || hasRole('ProductWriter');
  const canManageImages = () => hasRole('Admin') || hasRole('ImageManager');

  return {
    ...context,
    hasRole,
    isAdmin,
    canManageProducts,
    canManageImages
  };
};
