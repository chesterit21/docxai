/**
 * Utility untuk validasi connection string di appsetting
 */

interface AppConfig {
  [key: string]: any;
}

export const isConnectionStringConfigured = (config: AppConfig): boolean => {
  if (!config || typeof config !== "object") {
    return false;
  }

  // Check for postgreeSqlConnectionString dalam connection section
  const connectionConfig = config.connection;

  if (!connectionConfig || typeof connectionConfig !== "object") {
    return false;
  }

  const connectionString = connectionConfig.postgreeSqlConnectionString;

  // Connection string harus ada dan tidak boleh kosong
  if (!connectionString || connectionString.trim() === "") {
    return false;
  }

  return true;
};

export const getConnectionStringWarningMessage = (): string => {
  return "Connection String belum dikonfigurasi! Silakan setup connection string di Settings terlebih dahulu agar aplikasi dapat berfungsi dengan normal.";
};
