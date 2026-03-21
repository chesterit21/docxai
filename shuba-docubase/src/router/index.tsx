import { Navigate, createBrowserRouter } from "react-router-dom";
import { ProtectedRoute, PublicRoute, SetupRoute } from "./ProtectedRoute";
import AppLayout from "../components/AppLayout";
import Favorite from "../pages/Favorite";
import Dashboard from "../pages/Dashboard";
import Users from "../pages/Users";
import Profiles from "../pages/Profiles";
import ProfilesEdit from "../components/molecule/profiles/edit";
import Shared from "../pages/Shared";
import Document from "../pages/Documents";
import DocumentHome from "../components/molecule/document";
import SummitCorp from "../components/molecule/document/SummitCorp";
import DocumentView from "../components/molecule/document/DocumentView";
import DocumentEdit from "../components/molecule/document/DocumentEdit";
import Approval from "../pages/Approval";
import Attributes from "../pages/Attributes";
import Notification from "../pages/Notofication";
import Search from "../pages/Search";
import Login from "../pages/auth/Login";
import ForgotPassword from "../pages/auth/ForgotPassword";
import CodeVerification from "../pages/auth/CodeVerification";
import SetNewPassword from "../pages/auth/SetNewPassword";
import SetupSuperAdmin from "../pages/auth/SetupSuperAdmin";
import UserGroup from "../components/molecule/userGroup";
import UserDetail from "../components/molecule/userGroup/UserDetail";
import GroupDetail from "../components/molecule/userGroup/GroupDetail";
import GroupEdit from "../components/molecule/userGroup/GroupEdit";
import CollectionDetail from "../components/molecule/attributes/CollectionDetail";
import CollectionAdd from "../components/molecule/attributes/CollectionAdd";
import CollectionEdit from "../components/molecule/attributes/CollectionEdit";
import AttributeCollection from "../components/molecule/attributes/index";
import Settings from "../pages/Settings";
import Page404 from "../pages/NotFoundGuest";
import Verification from "../pages/Verification";
import Test from "../pages/Test";
import TestWord from "../pages/TestWord";
import Preview from "../pages/preview";
import AutoLogOut from "../pages/auth/AutoLogOut";
import Recyclebin from "../pages/Recyclebin";
import RecycleContent from "../components/molecule/recyclebin";
import ApplicationLog from "../pages/ApplicationLog";
import TransactionLog from "../pages/TransactionLog";
import EmailLog from "../pages/EmailLog";
import Watermark from "../pages/Watermark";
import Reminders from "../pages/Reminders";
import ChatAI from "../pages/ChatAI";
import AiModel from "../pages/AiModel";

export const router = createBrowserRouter([
  {
    path: "/",
    element: <Navigate to="/document" replace />,
  },
  {
    path: "/",
    element: (
      <ProtectedRoute>
        <AppLayout />
        <AutoLogOut />
      </ProtectedRoute>
    ),
    children: [
      { index: true, element: <Navigate to="/login" /> },
      { path: "favorite", element: <Favorite /> },
      { path: "shared-workspace", element: <Shared /> },
      { path: "dashboard", element: <Dashboard /> },
      { path: "approval", element: <Approval /> },
      { path: "chat-ai", element: <ChatAI /> },
      {
        path: "document",
        element: <Document />,
        children: [
          { index: true, element: <DocumentHome /> },
          { path: "summit-corp/:uuid/:name?", element: <SummitCorp /> },
          { path: "document-view/:uuid/:name?", element: <DocumentView /> },
          { path: "document-edit/:uuid/:name?", element: <DocumentEdit /> },
        ],
      },
      {
        path: "attributes",
        element: <Attributes />,
        children: [
          { index: true, element: <AttributeCollection /> },
          { path: "collection-detail/:uuid", element: <CollectionDetail /> },
          { path: "collection-add", element: <CollectionAdd /> },
          { path: "collection-edit/:uuid", element: <CollectionEdit /> },
        ],
      },
      { path: "profiles", element: <Profiles /> },
      { path: "profiles/edit/:uuid", element: <ProfilesEdit /> },
      {
        path: "users-group",
        element: <Users />,
        children: [
          { index: true, element: <UserGroup /> },
          { path: "user-detail/:uuid", element: <UserDetail /> },
          { path: "group-detail/:uuid", element: <GroupDetail /> },
          { path: "group-edit/:uuid", element: <GroupEdit /> },
        ],
      },
      { path: "notification", element: <Notification /> },
      { path: "reminder", element: <Reminders /> },
      { path: "search", element: <Search /> },
      { path: "settings", element: <Settings /> },
      { path: "watermark", element: <Watermark /> },
      { path: "ai-model", element: <AiModel /> },
      { path: "application-log", element: <ApplicationLog /> },
      { path: "transaction-log", element: <TransactionLog /> },
      { path: "email-log", element: <EmailLog /> },
      {
        path: "recyclebin",
        element: <Recyclebin />,
        children: [{ index: true, element: <RecycleContent /> }],
      },
    ],
  },
  {
    path: "/login",
    element: (
      <PublicRoute>
        <Login />
      </PublicRoute>
    ),
  },
  {
    path: "/initial-setup",
    element: (
      <SetupRoute>
        <SetupSuperAdmin />
      </SetupRoute>
    ),
  },
  {
    path: "/forgot-password",
    element: (
      <PublicRoute>
        <ForgotPassword />
      </PublicRoute>
    ),
  },
  {
    path: "/code-verification",
    element: (
      <PublicRoute>
        <CodeVerification />
      </PublicRoute>
    ),
  },
  {
    path: "/set-new-password",
    element: (
      <PublicRoute>
        <SetNewPassword />
      </PublicRoute>
    ),
  },
  {
    path: "/verification/verify",
    element: (
      <PublicRoute>
        <Verification />
      </PublicRoute>
    ),
  },
  {
    path: "*",
    element: <Page404 />,
  },
  {
    path: "/test",
    element: <Test />,
  },
  {
    path: "/testword",
    element: <TestWord />,
  },
  {
    path: "/preview",
    element: <Preview />,
  },
]);
