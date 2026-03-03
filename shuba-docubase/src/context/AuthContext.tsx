import {
  createContext,
  useContext,
  useState,
  useEffect,
  useCallback,
} from "react";
import { jwtDecode } from "jwt-decode";

interface User {
  aud: string;
  company_id: string;
  company_name: string;
  email?: string;
  exp: number;
  family_name?: string;
  given_name?: string;
  iat?: null;
  iss?: string;
  name?: string;
  sub?: string;
  user_type: string;
  website?: string;
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"?: string;
}

interface AuthContextType {
  user: User | null;
  userId: number | null;
  token: string | null;
  adminToken: string | null;
  config: any;
  isAuthenticated: boolean;
  isLoading: boolean;
  isSuperAdminSetup: boolean;
  isDatabaseConfigured: boolean;
  isConnectionStringConfigured: boolean;
  checkSuperAdminStatus: () => Promise<boolean>;
  loginAdmin: (username: string, password: string) => Promise<boolean>;
  login: (username: string, password: string) => Promise<boolean>;
  logout: () => void;
  register: (userData: RegisterData) => Promise<boolean>;
  refreshStatus: () => Promise<void>;
}

interface RegisterData {
  name: string;
  username: string;
  password: string;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider = ({ children }: { children: React.ReactNode }) => {
  const [user, setUser] = useState<User | null>(null);
  const [userId, setUserId] = useState<number | null>(null);
  const [token, setToken] = useState<string | null>(null);
  const [adminToken, setAdminToken] = useState<string | null>(null);
  const [config, setConfig] = useState<any>({});
  const [isLoading, setIsLoading] = useState(true);
  const [isSuperAdminSetup, setIsSuperAdminSetup] = useState(true);
  const [isDatabaseConfigured, setIsDatabaseConfigured] = useState(true);
  const [isConnectionStringConfigured, setIsConnectionStringConfigured] =
    useState(true);

  const checkSuperAdminStatus = useCallback(async (): Promise<boolean> => {
    try {
      const response = await fetch(
        `${import.meta.env.VITE_API_URL}/setting/get-login-sa`,
        {
          method: "GET",
          headers: {
            "Content-Type": "application/json",
          },
        }
      );

      if (!response.ok) {
        setIsSuperAdminSetup(false);
        return false;
      }

      const result = await response.json();

      const login = result?.data?.login;

      // 🔎 cek apakah data login ada & lengkap
      if (!login || !login.superAdminUser || !login.superAdminPassword) {
        setIsSuperAdminSetup(false);
        return false;
      }

      setIsSuperAdminSetup(true);
      return true;
    } catch (error) {
      console.error("Check superadmin status error:", error);
      return false;
    }
  }, []);

  const checkDatabaseStatus = useCallback(
    async (token: string): Promise<boolean> => {
      try {
        const response = await fetch(
          `${import.meta.env.VITE_API_URL}/setting/config-db`,
          {
            method: "GET",
            headers: {
              "x-admin-token": token,
            },
          }
        );

        if (!response.ok) {
          setIsDatabaseConfigured(false);
          return false;
        }

        const result = await response.json();
        const connString = result?.data?.connectionString;

        const isConfigured = !!(connString && connString.host);
        setIsDatabaseConfigured(isConfigured);
        return isConfigured;
      } catch (error) {
        console.error("Check database status error:", error);
        setIsDatabaseConfigured(false);
        return false;
      }
    },
    []
  );

  const refreshStatus = useCallback(async () => {
    try {
      setIsLoading(true);
      const saSetup = await checkSuperAdminStatus();

      const storedAdminToken = localStorage.getItem("adminToken");
      if (storedAdminToken) {
        await checkDatabaseStatus(storedAdminToken);
      } else if (!saSetup) {
        setIsDatabaseConfigured(false);
      }

      const storedToken = localStorage.getItem("token");
      const storedUser = localStorage.getItem("user");
      const storedConfig = localStorage.getItem("config");
      if (
        storedToken &&
        storedUser &&
        (storedConfig || typeof storedConfig == "string")
      ) {
        const isValid = await verifyToken(storedToken);

        if (isValid) {
          setToken(storedToken);
          setUser(jwtDecode<User>(storedUser));
          setConfig(JSON.parse(storedConfig));
        } else {
          localStorage.removeItem("token");
          localStorage.removeItem("user");
          localStorage.removeItem("config");
        }
      }
    } catch (error) {
      console.error("Auth check failed:", error);
    } finally {
      setIsLoading(false);
    }
  }, [checkSuperAdminStatus, checkDatabaseStatus]);

  useEffect(() => {
    refreshStatus();
  }, [refreshStatus]);

  const login = async (
    username: string,
    password: string
  ): Promise<boolean> => {
    try {
      setIsLoading(true);

      const response = await fetch(
        `${import.meta.env.VITE_API_URL}/authentication/access-token`,
        {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
          },
          body: JSON.stringify({ username, password }),
        }
      );

      if (response.ok) {
        const data = await response.json();
        const token = data.data.accessToken;

        const responseConfig = await fetch(
          `${import.meta.env.VITE_API_URL}/setting`,
          {
            method: "GET",
            headers: {
              "Content-Type": "application/json",
              Authorization: `Bearer ${token}`,
            },
          }
        );

        const getConfig = await responseConfig.json();
        const {
          activeDirectory,
          email,
          fileOption,
          maintenance,
          connectionString,
          ...config
        } = getConfig.data;

        // Check if connection string is configured
        const isConnected =
          connectionString &&
          connectionString.host &&
          connectionString.host.trim() !== "";

        setUser(jwtDecode<User>(token));
        setToken(token);
        setConfig(config);
        setIsConnectionStringConfigured(isConnected);
        setIsDatabaseConfigured(isConnected);

        localStorage.setItem("token", token);
        localStorage.setItem("user", token);
        localStorage.setItem("config", JSON.stringify(config));

        return true;
      } else {
        const errorData = await response.json();
        throw new Error(errorData.message || "Login failed");
      }
    } catch (error) {
      console.error("Login error:", error);
      return false;
    } finally {
      setIsLoading(false);
    }
  };

  const logout = () => {
    setUser(null);
    setUserId(null);
    setToken(null);
    localStorage.removeItem("token");
    localStorage.removeItem("user");
  };

  const register = async (userData: RegisterData): Promise<boolean> => {
    try {
      setIsLoading(true);

      const response = await fetch("/api/auth/register", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(userData),
      });

      if (response.ok) {
        const data = await response.json();

        setUser(data.user);
        setToken(data.token);
        localStorage.setItem("token", data.token);
        localStorage.setItem("user", JSON.stringify(data.user));

        return true;
      } else {
        const errorData = await response.json();
        throw new Error(errorData.message || "Registration failed");
      }
    } catch (error) {
      console.error("Registration error:", error);
      return false;
    } finally {
      setIsLoading(false);
    }
  };

  const loginAdmin = async (
    username: string,
    password: string
  ): Promise<boolean> => {
    try {
      const response = await fetch(
        `${import.meta.env.VITE_API_URL}/setting/login-sa`,
        {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
          },
          body: JSON.stringify({ username, password }),
        }
      );

      if (response.ok) {
        const data = await response.json();
        const adminTokenValue = data.data?.token || data.data?.adminToken;

        if (adminTokenValue) {
          setAdminToken(adminTokenValue);
          localStorage.setItem("adminToken", adminTokenValue);
          return true;
        }

        throw new Error("Admin token not received");
      } else {
        const errorData = await response.json();
        throw new Error(errorData.message || "Admin login failed");
      }
    } catch (error) {
      console.error("Admin login error:", error);
      return false;
    }
  };

  const value: AuthContextType = {
    user,
    userId,
    token,
    adminToken,
    isAuthenticated: !!user && !!token,
    isLoading,
    config,
    isSuperAdminSetup,
    isDatabaseConfigured,
    isConnectionStringConfigured,
    checkSuperAdminStatus,
    loginAdmin,
    login,
    logout,
    register,
    refreshStatus,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error("useAuth must be used within an AuthProvider");
  }
  return context;
};

const verifyToken = async (token: string): Promise<boolean> => {
  try {
    const response = await fetch("/api/auth/verify", {
      method: "GET",
      headers: {
        Authorization: `Bearer ${token}`,
      },
    });
    return response.ok;
  } catch (error) {
    console.error("Token verification failed:", error);
    return false;
  }
};
