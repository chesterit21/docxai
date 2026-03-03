import { Navigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext";

interface ProtectedRouteProps {
  children: React.ReactNode;
}

const LoadingSpinner = () => (
  <div className="flex items-center justify-center min-h-screen">
    <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
  </div>
);

export const ProtectedRoute = ({ children }: ProtectedRouteProps) => {
  const {
    isAuthenticated,
    isLoading,
    isSuperAdminSetup,
    isDatabaseConfigured,
  } = useAuth();

  if (isLoading) {
    return <LoadingSpinner />;
  }

  if (!isSuperAdminSetup || !isDatabaseConfigured) {
    return <Navigate to="/initial-setup" replace />;
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  return <>{children}</>;
};

export const PublicRoute = ({ children }: ProtectedRouteProps) => {
  const { isAuthenticated, isSuperAdminSetup } = useAuth();

  if (isAuthenticated && isSuperAdminSetup) {
    return <Navigate to="/document" replace />;
  }

  return <>{children}</>;
};

export const SetupRoute = ({ children }: ProtectedRouteProps) => {
  const { isSuperAdminSetup, isDatabaseConfigured, isLoading } = useAuth();

  if (isLoading) {
    return <LoadingSpinner />;
  }

  if (isSuperAdminSetup && isDatabaseConfigured) {
    return <Navigate to="/login" replace />;
  }

  return <>{children}</>;
};
