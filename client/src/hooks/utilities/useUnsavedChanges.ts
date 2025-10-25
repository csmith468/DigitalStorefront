import { useEffect } from "react";
import { useBlocker } from "react-router-dom";

interface UseUnsavedChangesOptions {
  isDirty: boolean;
}

export function UseUnsavedChanges({ isDirty }: UseUnsavedChangesOptions) {
  // Block browser close/refresh/back button (React Router navigation)
  const blocker = useBlocker(
    ({ currentLocation, nextLocation }) =>
      isDirty && currentLocation.pathname !== nextLocation.pathname
  );

  useEffect(() => {
    const handleBeforeUnload = (e: BeforeUnloadEvent) => {
      if (isDirty) {
        e.preventDefault();
        // @ts-ignore - returnValue deprecated but required for beforeunload
        e.returnValue = '';
      }
    };

    window.addEventListener('beforeunload', handleBeforeUnload);
    return () => window.removeEventListener('beforeunload', handleBeforeUnload);
  }, [isDirty]);

  return {
    showPrompt: blocker.state === 'blocked',
    proceed: () => blocker.proceed?.(),
    reset: () => blocker.reset?.()
  };
}