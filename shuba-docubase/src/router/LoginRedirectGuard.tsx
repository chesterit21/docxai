import type { JSX } from "react";
import { useAuth } from "../context/AuthContext";
import { Navigate } from "react-router-dom";
import { useEffect, useState } from "react";

const LoginRedirectGuard = ({ children }: { children: JSX.Element }) => {
  const {
    isAuthenticated,
    checkSuperAdminStatus,
    isSuperAdminSetup,
    isLoading,
  } = useAuth();
  const [checked, setChecked] = useState(false);

  useEffect(() => {
    const check = async () => {
      if (!isAuthenticated && !isLoading) {
        await checkSuperAdminStatus();
      }
      setChecked(true);
    };

    check();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [isAuthenticated, isLoading]);

  // If still loading, show nothing or spinner
  if (isLoading || !checked) {
    return null;
  }

  // If already authenticated, redirect to dashboard
  if (isAuthenticated) {
    return <Navigate to="/document" replace />;
  }

  // If not authenticated and superadmin not setup, redirect to setup
  if (!isSuperAdminSetup) {
    return <Navigate to="/initial-setup" replace />;
  }

  // Otherwise show login page
  return children;
};

export default LoginRedirectGuard;
