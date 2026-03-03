import { useState, useEffect, useCallback } from "react";
import {
  Button,
  Card,
  Form,
  Input,
  Space,
  message,
  Spin,
  Alert,
  Steps,
  Result,
} from "antd";
import {
  UserOutlined,
  LockOutlined,
  DatabaseOutlined,
  CheckCircleOutlined,
  LoadingOutlined,
  SafetyCertificateOutlined,
} from "@ant-design/icons";
import Logo from "../../assets/logo.png";
import Background from "../../assets/auth-bg.jpg";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../../context/AuthContext";

interface SetupFormValues {
  username: string;
  password: string;
  password_confirmation: string;
}

interface DbConfigValues {
  host: string;
  port: number;
  database: string;
  username: string;
  password?: string;
}

const SetupSuperAdminPage: React.FC = () => {
  const navigate = useNavigate();
  const { refreshStatus } = useAuth();
  const [currentStep, setCurrentStep] = useState<number>(0);
  const [loading, setLoading] = useState(false);
  const [checkingStatus, setCheckingStatus] = useState(true);
  const [adminToken, setAdminToken] = useState<string | null>(null);
  const [migrationJobId, setMigrationJobId] = useState<string | null>(null);
  const [, setMigrationStatus] = useState<"running" | "success" | "failed">(
    "running"
  );

  const [saForm] = Form.useForm();
  const [loginForm] = Form.useForm();
  const [dbForm] = Form.useForm();

  const API_URL = import.meta.env.VITE_API_URL;

  const checkDbConfig = useCallback(
    async (token: string) => {
      try {
        const response = await fetch(`${API_URL}/setting/config-db`, {
          method: "GET",
          headers: {
            "x-admin-token": token,
          },
        });
        if (response.ok) {
          const data = await response.json();
          if (data.data?.connectionString) {
            dbForm.setFieldsValue(data.data.connectionString);
          }
        }
        setCurrentStep(2); // Go to DB Config
      } catch {
        // If check fails, just go to config step anyway
        setCurrentStep(2);
      }
    },
    [API_URL, dbForm]
  );

  const checkInitialStatus = useCallback(async () => {
    try {
      setCheckingStatus(true);
      const response = await fetch(`${API_URL}/setting/get-login-sa`, {
        method: "GET",
        headers: { "Content-Type": "application/json" },
      });

      // Parse JSON safely
      let data;
      try {
        data = await response.json();
      } catch {
        data = {};
      }

      console.log("Setup Check Response:", data);

      const loginData = data?.data?.login;
      const storedToken = localStorage.getItem("adminToken");

      // Check if SA is not set (superAdminUser is null)
      if (loginData && loginData.superAdminUser === null) {
        setCurrentStep(0); // Create SA
      } else if (storedToken) {
        // We have an admin token, skip to DB config
        setAdminToken(storedToken);
        await checkDbConfig(storedToken);
      } else if (
        data.status === "Unauthorized" ||
        (data.message && data.message.includes("Database is not configured"))
      ) {
        setCurrentStep(1); // Login SA
      } else if (loginData && loginData.superAdminUser) {
        // SA Exists
        message.info("Super Admin account found. Proceeding to login.");
        setCurrentStep(1);
      } else if (data.status === "SetupRequired") {
        // Fallback for previous spec just in case
        setCurrentStep(0);
      } else {
        // Default fallback or if everything is proper
        setCurrentStep(1);
      }
    } catch (error) {
      console.error("Status check failed:", error);
      message.error("Failed to check setup status. Is the server running?");
    } finally {
      setCheckingStatus(false);
    }
  }, [API_URL, checkDbConfig]);

  useEffect(() => {
    checkInitialStatus();
  }, [checkInitialStatus]);

  // Polling for Migration Status
  useEffect(() => {
    let intervalId: NodeJS.Timeout;

    if (currentStep === 3 && migrationJobId) {
      intervalId = setInterval(async () => {
        try {
          const response = await fetch(
            `${API_URL}/setting/migration-status/${migrationJobId}`,
            {
              method: "GET",
              headers: {
                "Content-Type": "application/json",
                ...(adminToken ? { "x-admin-token": adminToken } : {}),
              },
            }
          );

          if (response.ok) {
            const res = await response.json();
            if (res.code === 200 && res.data) {
              const status = res.data.status;
              if (status === 2) {
                // 2 = Completed
                setMigrationStatus("success");
                setCurrentStep(4);
                clearInterval(intervalId);
                message.success("Migration completed successfully.");
              } else if (status === 1) {
                // 1 = Running
                setMigrationStatus("running");
                message.info("Migration is running...");
              } else if (status === 3) {
                // 3 = Failed
                setMigrationStatus("failed");
                message.error("Migration failed. Please check server logs.");
                clearInterval(intervalId);
              }
            }
          }
        } catch (error) {
          console.error("Migration status check failed", error);
        }
      }, 3000);
    }

    return () => {
      if (intervalId) clearInterval(intervalId);
    };
  }, [currentStep, migrationJobId, API_URL, adminToken]);

  const handleCreateSA = async (values: SetupFormValues) => {
    if (values.password !== values.password_confirmation) {
      message.error("Password validation failed");
      return;
    }

    setLoading(true);
    try {
      const response = await fetch(`${API_URL}/setting/set-login-sa`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          login: {
            superAdminUser: values.username,
            superAdminPassword: values.password,
          },
        }),
      });

      if (response.ok) {
        message.success("Super Admin created successfully. Please login.");
        setCurrentStep(1); // Go to Login
        loginForm.setFieldsValue({ username: values.username });
      } else {
        const err = await response.json();
        message.error(err.message || "Failed to create Super Admin.");
      }
    } catch (error: any) {
      message.error(error.message || "An error occurred.");
    } finally {
      setLoading(false);
    }
  };

  const handleLoginSA = async (values: {
    username: string;
    password: string;
  }) => {
    setLoading(true);
    try {
      const response = await fetch(`${API_URL}/setting/login-sa`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(values),
      });

      const data = await response.json();

      if (response.ok) {
        const token = data.data?.token || data.data?.adminToken;
        if (token) {
          setAdminToken(token);
          localStorage.setItem("adminToken", token); // Backup
          message.success("Admin login successful.");

          // Check if DB is already configured by calling get config
          // If configured, just go to success? Or maybe verify?
          // The flow "Login -> DB Config" implies we should always show config or check it.
          // Let's check config first.
          await checkDbConfig(token);
        } else {
          message.error("Token not found in response.");
        }
      } else {
        message.error(data.message || "Login failed.");
      }
    } catch (error: any) {
      message.error(error.message || "Login request failed.");
    } finally {
      setLoading(false);
    }
  };

  const handleConfigDB = async (values: DbConfigValues) => {
    setLoading(true);
    try {
      const payload = {
        connectionString: {
          host: values.host,
          port: Number(values.port), // Ensure number
          database: values.database,
          username: values.username,
          password: values.password,
          timeout: 30,
          commandTimeout: 60,
        },
      };

      const response = await fetch(`${API_URL}/setting/set-config-db`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          "x-admin-token": adminToken || "",
        },
        body: JSON.stringify(payload),
      });

      const data = await response.json();

      if (response.status === 200) {
        const jobId = data.jobId;

        if (jobId) {
          setMigrationJobId(jobId);
          setCurrentStep(3);
          message.success(
            data.message || "Configuration saved. Migration running..."
          );
        } else {
          message.warning(
            "Configuration saved but Job ID missing. Please check manually."
          );
          setCurrentStep(4);
        }
      } else {
        message.error(data.message || "Configuration failed.");
      }
    } catch {
      message.error("Failed to connect/configure database.");
    } finally {
      setLoading(false);
    }
  };

  if (checkingStatus) {
    return (
      <div className="w-screen h-screen flex items-center justify-center bg-gray-50">
        <Spin size="large" tip="Checking System Status..." />
      </div>
    );
  }

  const stepItems = [
    { title: "Create Admin", icon: <UserOutlined /> },
    { title: "Login Admin", icon: <SafetyCertificateOutlined /> },
    { title: "Config DB", icon: <DatabaseOutlined /> },
    { title: "Migration", icon: <LoadingOutlined /> },
    { title: "Done", icon: <CheckCircleOutlined /> },
  ];

  return (
    <div
      className="w-screen h-screen flex"
      style={{
        backgroundImage: `url(${Background})`,
        backgroundSize: "cover",
        backgroundPosition: "center",
      }}
    >
      <div className="w-full md:w-1/2 flex flex-col items-center justify-center bg-white/95 p-8 overflow-y-auto">
        <div className="max-w-md w-full">
          <Space
            direction="vertical"
            style={{ width: "100%", marginBottom: 40 }}
            align="center"
          >
            <img src={Logo} alt="Logo" className="h-12 mb-4" />
            <Steps current={currentStep} items={stepItems} size="small" />
          </Space>

          {/* Step 0: Create Super Admin */}
          {currentStep === 0 && (
            <Card
              title="Create Super Admin"
              bordered={false}
              className="shadow-md"
            >
              <Form form={saForm} layout="vertical" onFinish={handleCreateSA}>
                <Form.Item
                  name="username"
                  label="Username"
                  rules={[
                    { required: true, message: "Please input username!" },
                  ]}
                >
                  <Input
                    prefix={<UserOutlined />}
                    placeholder="Username"
                    size="large"
                  />
                </Form.Item>
                <Form.Item
                  name="password"
                  label="Password"
                  rules={[{ required: true, min: 6, message: "Min 6 chars!" }]}
                >
                  <Input.Password
                    prefix={<LockOutlined />}
                    placeholder="Password"
                    size="large"
                  />
                </Form.Item>
                <Form.Item
                  name="password_confirmation"
                  label="Confirm Password"
                  rules={[
                    { required: true, message: "Please confirm password!" },
                  ]}
                >
                  <Input.Password
                    prefix={<LockOutlined />}
                    placeholder="Confirm Password"
                    size="large"
                  />
                </Form.Item>
                <Button
                  type="primary"
                  htmlType="submit"
                  block
                  loading={loading}
                  size="large"
                  color="purple"
                  variant="solid"
                >
                  Create Account
                </Button>
              </Form>
            </Card>
          )}

          {/* Step 1: Login Super Admin */}
          {currentStep === 1 && (
            <Card
              title="Login Super Admin"
              bordered={false}
              className="shadow-md"
            >
              <Alert
                message="Please login to configure the database."
                type="info"
                showIcon
                className="mb-4"
              />
              <Form form={loginForm} layout="vertical" onFinish={handleLoginSA}>
                <Form.Item
                  name="username"
                  label="Username"
                  rules={[{ required: true }]}
                >
                  <Input
                    prefix={<UserOutlined />}
                    placeholder="Username"
                    size="large"
                  />
                </Form.Item>
                <Form.Item
                  name="password"
                  label="Password"
                  rules={[{ required: true }]}
                >
                  <Input.Password
                    prefix={<LockOutlined />}
                    placeholder="Password"
                    size="large"
                  />
                </Form.Item>
                <Button
                  type="primary"
                  htmlType="submit"
                  block
                  loading={loading}
                  size="large"
                  color="purple"
                  variant="solid"
                >
                  Login & Continue
                </Button>
              </Form>
            </Card>
          )}

          {/* Step 2: Configure Database */}
          {currentStep === 2 && (
            <Card
              title="Database Configuration"
              bordered={false}
              className="shadow-md"
            >
              <Form
                form={dbForm}
                layout="vertical"
                onFinish={handleConfigDB}
                initialValues={{ port: 5432, host: "localhost" }}
              >
                <Space
                  direction="horizontal"
                  style={{ width: "100%" }}
                  size="middle"
                >
                  <Form.Item
                    name="host"
                    label="Host"
                    style={{ flex: 2 }}
                    rules={[{ required: true, message: "Required" }]}
                  >
                    <Input placeholder="localhost" />
                  </Form.Item>
                  <Form.Item
                    name="port"
                    label="Port"
                    style={{ flex: 1 }}
                    rules={[{ required: true, message: "Required" }]}
                  >
                    <Input type="number" placeholder="5432" />
                  </Form.Item>
                </Space>

                <Form.Item
                  name="database"
                  label="Database Name"
                  rules={[{ required: true, message: "Required" }]}
                >
                  <Input
                    placeholder="docubase_db"
                    prefix={<DatabaseOutlined />}
                  />
                </Form.Item>

                <Form.Item
                  name="username"
                  label="Username"
                  rules={[{ required: true, message: "Required" }]}
                >
                  <Input placeholder="postgres" prefix={<UserOutlined />} />
                </Form.Item>

                <Form.Item
                  name="password"
                  label="Password"
                  rules={[{ required: true, message: "Required" }]}
                >
                  <Input.Password
                    placeholder="Password"
                    prefix={<LockOutlined />}
                  />
                </Form.Item>

                <Button
                  type="primary"
                  htmlType="submit"
                  block
                  loading={loading}
                  size="large"
                  color="purple"
                  variant="solid"
                >
                  Save & Migrate
                </Button>
              </Form>
            </Card>
          )}

          {/* Step 3: Migration Running */}
          {currentStep === 3 && (
            <Card bordered={false} className="shadow-md text-center">
              <Spin size="large" />
              <h2 className="mt-4 text-lg font-semibold">
                Migrating Database...
              </h2>
              <p className="text-gray-500">
                Please check the migration status. Do not close this window.
              </p>
            </Card>
          )}

          {/* Step 4: Success */}
          {currentStep === 4 && (
            <Card bordered={false} className="shadow-md">
              <Result
                status="success"
                title="Setup Complete!"
                subTitle="Your application is ready to use."
                extra={[
                  <Button
                    type="primary"
                    key="login"
                    size="large"
                    onClick={async () => {
                      await refreshStatus();
                      navigate("/login");
                    }}
                  >
                    Go to Login
                  </Button>,
                ]}
              />
            </Card>
          )}
        </div>
      </div>

      <div className="hidden md:flex w-1/2 items-center justify-center">
        {/* Right side background managed by parent auth layout or handled via styling above */}
      </div>
    </div>
  );
};

export default SetupSuperAdminPage;
