// main.tsx
import React from "react";
import ReactDOM from "react-dom/client";
import { RouterProvider } from "react-router-dom";
import { ConfigProvider } from "antd";
import { router } from "./router";
import "antd/dist/reset.css";
import "./index.css";
import { AuthProvider } from "./context/AuthContext";

const validateMessages = {
  required: "Please fill in the ${label}.",
  types: {
    string: "${label} must contain valid text.",
    email: "Please enter a valid email address.",
    number: "${label} must be a valid number.",
  },
};

ReactDOM.createRoot(document.getElementById("root")!).render(
  <React.StrictMode>
    <ConfigProvider
      theme={{
        token: {
          fontFamily: "'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, 'Open Sans', 'Helvetica Neue', sans-serif",
        },
      }}
      form={{
        validateMessages,
      }}
    >
      <AuthProvider>
        <RouterProvider router={router} />
      </AuthProvider>
    </ConfigProvider>
  </React.StrictMode>
);
