import { useContext } from "react";
import { UserContext, type UserContextType } from "../contexts/UserContext"

export const useUser = (): UserContextType => {
  const context = useContext(UserContext);
  if (!context)
    throw new Error('useUser must be used within UserProvider.');
  return context;
}