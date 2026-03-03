import React, { useEffect } from "react";
import Background from "../../assets/auth-bg.jpg";
// import Image from '../../assets/img-auth.png'
import Logo from "../../assets/logo.png";

import { Button, Card, Form, Input, Space, message, Alert } from "antd";
import { LockOutlined, UserOutlined } from "@ant-design/icons";
import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "../../context/AuthContext";

interface LoginFormValues {
  userName: string;
  password: string;
}

const LoginPage: React.FC = () => {
  const navigate = useNavigate();
  const {
    login,
    isLoading,
    isSuperAdminSetup,
    isDatabaseConfigured,
    isConnectionStringConfigured,
  } = useAuth();
  const [form] = Form.useForm();

  useEffect(() => {
    if (!isLoading && (!isSuperAdminSetup || !isDatabaseConfigured)) {
      navigate("/initial-setup");
    }
  }, [isLoading, isSuperAdminSetup, isDatabaseConfigured, navigate]);

  const onFinish = async (values: LoginFormValues) => {
    try {
      const success = await login(values.userName, values.password);

      if (success) {
        message.success("Login success!");
        navigate("/document");
      } else {
        message.error("Username or password is wrong!");
      }
    } catch (error) {
      message.error("Something error, try again");
      console.error("Login error:", error);
    }
  };

  const onFinishFailed = (errorInfo: unknown) => {
    console.log("Failed:", errorInfo);
    message.error("Please fill field complately");
  };

  return (
    <div className="w-screen h-screen flex">
      <div className="w-full md:w-1/2 flex items-center justify-center bg-white p-8">
        <div className="max-w-md w-full">
          <Card style={{ border: "none" }}>
            <Space style={{ marginBottom: 50 }}>
              <img src={Logo} alt="" className="block h-13" />
              {/* <div
                        style={{
                        color: '#09090B',
                        fontWeight: 'bold',
                        fontSize: 22
                        }}
                    >
                        Docubase
                    </div> */}
            </Space>
            <div className="mb-8 flex flex-col gap-1">
              <span className="text-2xl font-bold">Log in</span>
              <span>Welcome back! Please enter your details.</span>
            </div>
            {!isConnectionStringConfigured && (
              <Alert
                message="Peringatan: Connection String Belum Dikonfigurasi"
                description="Connection String belum dikonfigurasi! Silakan hubungi administrator untuk men-setup connection string."
                type="warning"
                showIcon
                closable
                style={{ marginBottom: 16 }}
              />
            )}
            <Form
              form={form}
              name="login-form"
              // initialValues={{ userName: 'superadmin', password: 'P@ssw0rd' }}
              onFinish={onFinish}
              onFinishFailed={onFinishFailed}
              layout="vertical"
              disabled={isLoading}
            >
              <Form.Item
                label="Username"
                name="userName"
                rules={[
                  { required: true, message: "Please enter your username!" },
                  { type: "string", message: "Please enter a valid username!" },
                ]}
              >
                <Input
                  prefix={<UserOutlined />}
                  placeholder="Enter your username"
                  size="large"
                />
              </Form.Item>

              <Form.Item
                label="Password"
                name="password"
                rules={[
                  { required: true, message: "Please enter your password!" },
                  {
                    min: 6,
                    message: "Password must be at least 6 characters!",
                  },
                ]}
              >
                <Input.Password
                  prefix={<LockOutlined />}
                  placeholder="Enter your password"
                  size="large"
                />
              </Form.Item>

              <Form.Item>
                <Button
                  color="purple"
                  variant="solid"
                  size="large"
                  htmlType="submit"
                  block
                  style={{ marginTop: 20 }}
                  loading={isLoading}
                >
                  {isLoading ? "Signing in..." : "Sign in"}
                </Button>
              </Form.Item>
            </Form>
            <div style={{ textAlign: "center" }}>
              <span>
                Forgot your password?{" "}
                <Link to="/forgot-password">Reset password</Link>
              </span>
            </div>
          </Card>
        </div>
      </div>

      <div className="hidden md:flex w-1/2 relative items-center justify-end overflow-hidden">
        <img
          src={Background}
          alt="Background"
          className="absolute w-full h-full object-cover top-0 left-0 z-0"
        />

        <img
          // src={Image}
          alt="Illustration"
          className="z-10 max-w-[100%] max-h-[90%] -mr-[100px] rounded-l-lg"
        />
      </div>
    </div>
  );
};

export default LoginPage;
