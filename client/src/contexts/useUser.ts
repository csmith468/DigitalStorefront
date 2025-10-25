import { useContext } from "react";
import { UserContext, type UserContextType } from "./UserContext";

// NOTE: Could move to hooks but I feel like it should go with UserContext since 
// it's a Context API wrapper and this is the only non-React Query non-utility hook

export const useUser = (): UserContextType => {
  const context = useContext(UserContext);
  if (!context) throw new Error("useUser must be used within UserProvider.");
  return context;
};
