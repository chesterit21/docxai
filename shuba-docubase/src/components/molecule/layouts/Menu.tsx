import { ConfigProvider, Menu, type MenuProps } from "antd";
import { Link, useLocation } from "react-router-dom";
import {
  AppstoreOutlined,
  ContainerOutlined,
  FileDoneOutlined,
  FileTextOutlined,
  ProductOutlined,
  RestOutlined,
  SettingOutlined,
  ShareAltOutlined,
  SolutionOutlined,
  UsergroupAddOutlined,
  UserOutlined,
  FileProtectOutlined,
  RobotOutlined,
} from "@ant-design/icons";
import { useState, useMemo } from "react";
import { useAuth } from "../../../context/AuthContext";

type MenuItem = Required<MenuProps>["items"][number];

interface LevelKeysProps {
  key?: string;
  children?: LevelKeysProps[];
}

const getLevelKeys = (items1: LevelKeysProps[]) => {
  const key: Record<string, number> = {};
  const func = (items2: LevelKeysProps[], level = 1) => {
    items2.forEach((item) => {
      if (item.key) {
        key[item.key] = level;
      }
      if (item.children) {
        func(item.children, level + 1);
      }
    });
  };
  func(items1);
  return key;
};

export default function MenuItem({ collapsed }: { collapsed: boolean }) {
  const { user } = useAuth();
  const location = useLocation();

  const items: MenuItem[] = [
    {
      key: "favorite",
      icon: <AppstoreOutlined style={{ fontSize: 20 }} />,
      label: (
        <Link to="/favorite" style={{ fontSize: 15 }}>
          Favorite
        </Link>
      ),
    },
    {
      key: "shared-workspace",
      icon: <ShareAltOutlined style={{ fontSize: 20 }} />,
      label: (
        <Link to="/shared-workspace" style={{ fontSize: 15 }}>
          Shared Workspace
        </Link>
      ),
    },
    {
      key: "recyclebin",
      icon: <RestOutlined style={{ fontSize: 20 }} />,
      label: (
        <Link to="/recyclebin" style={{ fontSize: 15 }}>
          Recycle Bin
        </Link>
      ),
    },
    { type: "divider" },
    {
      key: "dashboard",
      icon: <ProductOutlined style={{ fontSize: 20 }} />,
      label: (
        <Link to="/dashboard" style={{ fontSize: 15 }}>
          Dashboard
        </Link>
      ),
    },
    {
      key: "document",
      icon: <FileTextOutlined style={{ fontSize: 20 }} />,
      label: (
        <Link to="/document" style={{ fontSize: 15 }}>
          Document
        </Link>
      ),
    },
    {
      key: "approval",
      icon: <FileDoneOutlined style={{ fontSize: 20 }} />,
      label: (
        <Link to="/approval" style={{ fontSize: 15 }}>
          Approval
        </Link>
      ),
    },
    // {
    //   key: "chat-ai",
    //   icon: <RobotOutlined style={{ fontSize: 20 }} />,
    //   label: (
    //     <Link to="/chat-ai" style={{ fontSize: 15 }}>
    //       AI Assistant
    //     </Link>
    //   ),
    //},
    // ...(user?.user_type === 'superadmin' ? [
    // ] : []),
    ...(user?.user_type === "admin" || user?.user_type === "superadmin"
      ? [{ type: "divider" as const }]
      : []),
    ...(user?.user_type === "admin" || user?.user_type === "superadmin"
      ? [
          {
            key: "administration",
            label: <div style={{ fontSize: 15 }}>Administration</div>,
            icon: collapsed ? (
              <SolutionOutlined style={{ fontSize: 20 }} />
            ) : undefined,
            style: { background: "" },
            children: [
              {
                key: "attributes",
                icon: <FileTextOutlined style={{ fontSize: 20 }} />,
                label: (
                  <Link to="/attributes" style={{ fontSize: 15 }}>
                    Attributes
                  </Link>
                ),
              },
              {
                key: "profiles",
                icon: <UserOutlined style={{ fontSize: 20 }} />,
                label: (
                  <Link to="/profiles" style={{ fontSize: 15 }}>
                    Profiles
                  </Link>
                ),
              },
              {
                key: "watermark",
                icon: <FileProtectOutlined style={{ fontSize: 20 }} />,
                label: (
                  <Link to="/watermark" style={{ fontSize: 15 }}>
                    Watermark
                  </Link>
                ),
              },
              ...(user?.user_type === "superadmin"
                ? [
                    {
                      key: "users-group",
                      icon: <UsergroupAddOutlined style={{ fontSize: 20 }} />,
                      label: (
                        <Link to="/users-group" style={{ fontSize: 15 }}>
                          Users & Group
                        </Link>
                      ),
                    },
                    {
                      key: "settings",
                      icon: <SettingOutlined style={{ fontSize: 20 }} />,
                      label: (
                        <Link to="/settings" style={{ fontSize: 15 }}>
                          Application Setting
                        </Link>
                      ),
                    },
                    {
                      key: "ai-model",
                      icon: <RobotOutlined style={{ fontSize: 20 }} />,
                      label: (
                        <Link to="/ai-model" style={{ fontSize: 15 }}>
                          AI Model Configuration
                        </Link>
                      ),
                    },
                    {
                      key: "system-log",
                      icon: <ContainerOutlined style={{ fontSize: 20 }} />,
                      label: <div style={{ fontSize: 15 }}>System Log</div>,
                      children: [
                        {
                          key: "application-log",
                          icon: <ContainerOutlined style={{ fontSize: 20 }} />,
                          label: (
                            <Link
                              to="/application-log"
                              style={{ fontSize: 15 }}
                            >
                              Application Log
                            </Link>
                          ),
                        },
                        {
                          key: "transaction-log",
                          icon: <ContainerOutlined style={{ fontSize: 20 }} />,
                          label: (
                            <Link
                              to="/transaction-log"
                              style={{ fontSize: 15 }}
                            >
                              Transaction Log
                            </Link>
                          ),
                        },
                        {
                          key: "email-log",
                          icon: <ContainerOutlined style={{ fontSize: 20 }} />,
                          label: (
                            <Link to="/email-log" style={{ fontSize: 15 }}>
                              Email Log
                            </Link>
                          ),
                        },
                      ],
                    },
                  ]
                : []),
            ],
          },
        ]
      : []),
  ];

  const levelKeys = getLevelKeys(items as LevelKeysProps[]);

  const currentKey = useMemo(() => {
    const path = location.pathname.split("/")[1];
    return path === "" ? "dashboard" : path;
  }, [location.pathname]);

  const [stateOpenKeys, setStateOpenKeys] = useState<string[]>([]);

  const onOpenChange: MenuProps["onOpenChange"] = (openKeys) => {
    const currentOpenKey = openKeys.find((key) => !stateOpenKeys.includes(key));
    if (currentOpenKey !== undefined) {
      const repeatIndex = openKeys
        .filter((key) => key !== currentOpenKey)
        .findIndex((key) => levelKeys[key] === levelKeys[currentOpenKey]);

      setStateOpenKeys(
        openKeys
          .filter((_, index) => index !== repeatIndex)
          .filter((key) => levelKeys[key] <= levelKeys[currentOpenKey])
      );
    } else {
      setStateOpenKeys(openKeys);
    }
  };

  return (
    <ConfigProvider
      theme={{
        components: {
          Menu: {
            subMenuItemBg: "#f5f5f5",
          },
        },
      }}
    >
      <Menu
        mode="inline"
        selectedKeys={[currentKey]}
        openKeys={stateOpenKeys}
        onOpenChange={onOpenChange}
        items={items}
        style={{ backgroundColor: "#f5f5f5", border: "none" }}
      />
    </ConfigProvider>
  );
}
