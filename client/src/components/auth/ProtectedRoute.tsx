import { useUser } from "../../hooks/useUser";
import { LoadingScreen } from "../primitives/LoadingScreen";

export function ProtectedRoute({ children }: { children: React.ReactNode }) {
  const { isAuthenticated, isLoading, openAuthModal } = useUser();

  if (isLoading) {
    return <LoadingScreen />;
  }

  if (!isAuthenticated) {
    return (
      <div className="max-w-2xl mx-auto px-4 py-16 text-center">
        <h2 className="text-2xl font-bold text-gray-900 mb-4">
          Authentication Required
        </h2>
        <p className="text-gray-600 mb-6">
          You need to be logged in to access the admin console.
        </p>
        <button
          onClick={openAuthModal}
          className="px-6 py-3 bg-blue-600 text-white font-semibold rounded-lg hover:bg-blue-700">
          Sign In
        </button>
      </div>
    );
  }

  return <>{children}</>;
}
