// main.tsx
import React from "react";
import ReactDOM from "react-dom/client";
import { RouterProvider } from "react-router-dom";
import { ConfigProvider } from "antd";
import { router } from "./router";
import "antd/dist/reset.css";
import "./index.css";
import "@syncfusion/ej2-base/styles/material.css";
import "@syncfusion/ej2-buttons/styles/material.css";
import "@syncfusion/ej2-dropdowns/styles/material.css";
import "@syncfusion/ej2-inputs/styles/material.css";
import "@syncfusion/ej2-navigations/styles/material.css";
import "@syncfusion/ej2-popups/styles/material.css";
import "@syncfusion/ej2-splitbuttons/styles/material.css";
import "@syncfusion/ej2-pdfviewer/styles/material.css";
import "@syncfusion/ej2-documenteditor/styles/material.css";
import "@syncfusion/ej2-lists/styles/material.css";
import "@syncfusion/ej2-grids/styles/material.css";
import "@syncfusion/ej2-react-spreadsheet/styles/material.css";
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
