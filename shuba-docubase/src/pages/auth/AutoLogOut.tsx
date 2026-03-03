import React, { useEffect, useCallback, useRef } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../../context/AuthContext";
const appConfig = JSON.parse(localStorage.getItem('appConfig') || '{}' )

const AutoLogout: React.FC = () => {
  const timeIdle = Object.keys(appConfig).length > 0 ? appConfig?.login?.idleTimeoutAfterMinutes : 10 | 10
  const navigate = useNavigate();
  const logoutTimer = useRef<NodeJS.Timeout | null>(null);
  const INACTIVITY_LIMIT = timeIdle * 60 * 1000;
  const { logout } = useAuth();
  
  const resetTimer = useCallback(() => {
    if (logoutTimer.current) {
      clearTimeout(logoutTimer.current);
    }

    logoutTimer.current = setTimeout(() => {
        logout();
        navigate('/login');
    }, INACTIVITY_LIMIT);
  }, []);

  useEffect(() => {
    const events = ["mousemove", "keydown", "scroll", "click"];
    const handleActivity = () => resetTimer();

    events.forEach((event) =>
      window.addEventListener(event, handleActivity)
    );

    resetTimer();

    return () => {
      events.forEach((event) =>
        window.removeEventListener(event, handleActivity)
      );
      if (logoutTimer.current) {
        clearTimeout(logoutTimer.current);
      }
    };
  }, [resetTimer]);

  return null;
};

export default AutoLogout;
