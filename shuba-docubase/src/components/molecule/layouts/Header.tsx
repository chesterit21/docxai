import { useState, useEffect } from "react";
import {
  DoubleLeftOutlined,
  DoubleRightOutlined,
  LogoutOutlined,
  OrderedListOutlined,
  SearchOutlined,
  UserOutlined,
} from "@ant-design/icons";
import { Button, Dropdown, Input, Layout, Space, Typography } from "antd";
import type { MenuProps } from "antd";
import PopListNotification from "./PopListNotification";
import { useNavigate, useLocation } from "react-router-dom";
import ModalItemList from "./ModalItemList";
import ModalAdvanceSearch from "./ModalAdvanceSearch";
import { useAuth } from "../../../context/AuthContext";
import dayjs from "dayjs";
import customParseFormat from "dayjs/plugin/customParseFormat";
import aiIcon from "../../../assets/ai_icon.png";
dayjs.extend(customParseFormat);

const { Header } = Layout;

export default function HeaderComponent({ collapsed, setCollapsed }: any) {
  const { logout, user, config } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [search, setSearch] = useState<string>("");
  const dateConfig =
    config?.dataGrid?.dateTimeFormat || "YYYY-MM-DD | HH:mm:ss";

  function convertIatToLocalDate(iat: number | string): string {
    const timestamp = typeof iat === "string" ? parseInt(iat, 10) : iat;
    return dayjs.unix(timestamp).format(dateConfig);
  }

  const handleLogout = () => {
    logout();
    navigate("/login");
  };
  const [open, setOpen] = useState(false);

  const avatarMenuItems: MenuProps["items"] = [
    {
      key: "grp",
      type: "group",
      label: (
        <div className="flex flex-col">
          <span className="font-bold text-slate-900">{user?.name || ""}</span>
          <span className="text-xs italic !text-slate-600">
            login: {user?.iat ? convertIatToLocalDate(user.iat) : ""}
          </span>
        </div>
      ),
    },
    { type: "divider" },
    {
      key: "profile",
      label: (
        <div
          onClick={() => navigate("/profiles")}
          className="flex justify-between gap-5"
        >
          <UserOutlined /> <div>Profile</div>{" "}
        </div>
      ),
    },
    {
      key: "logOut",
      label: (
        <div onClick={handleLogout} className="flex justify-between gap-5">
          <LogoutOutlined /> <div>Logout</div>{" "}
        </div>
      ),
    },
  ];

  useEffect(() => {
    if (location.pathname !== "/search") {
      setSearch("");
    }
  }, [location.pathname]);

  const handleKeyDown = (event: any) => {
    if (event.key === "Enter" || event.code === "Enter") {
      const str = search.length > 0 ? search : "";
      const queryParams = new URLSearchParams({
        textsearch: str,
        type: "global",
      });
      navigate(`/search?${queryParams}`);
    }
  };

  return (
    <>
      <ModalItemList open={open} setOpen={setOpen} />
      <Header
        style={{
          position: "sticky",
          top: 0,
          zIndex: 1,
          background: "#f5f5f5",
          padding: "0 16px",
          display: "flex",
          justifyContent: "space-between",
          alignItems: "center",
        }}
      >
        <Space>
          {collapsed ? (
            <DoubleRightOutlined
              style={{ fontSize: 18 }}
              onClick={() => setCollapsed(false)}
            />
          ) : (
            <DoubleLeftOutlined
              style={{ fontSize: 18 }}
              onClick={() => setCollapsed(true)}
            />
          )}
          <Typography.Title level={5} style={{ margin: 0 }}></Typography.Title>
        </Space>
        <Space size="small">
          <Input
            size="large"
            value={search}
            onChange={(e: any) => setSearch(e.target.value)}
            onKeyDown={handleKeyDown}
            placeholder="Search ..."
            prefix={<SearchOutlined />}
          />
          <ModalAdvanceSearch />
          {/* Added AI Assistant button */}
          <Button 
            size="large" 
            onClick={() => navigate("/chat-ai")}
            style={{ display: 'flex', alignItems: 'center' }}
          >
            <img src={aiIcon} alt="AI Assistant" style={{ width: 20, height: 20, marginRight: 8 }} />
            Assistant
          </Button>
          {/* Notification dropdown */}
          <PopListNotification />

          <Button
            size="large"
            icon={<OrderedListOutlined style={{ fontSize: 20 }} />}
            onClick={() => setOpen(!open)}
          />

          {/* Avatar dropdown */}
          <Dropdown
            menu={{ items: avatarMenuItems }}
            placement="bottomRight"
            trigger={["click"]}
          >
            <Button
              size="large"
              icon={<UserOutlined style={{ fontSize: 20 }} />}
            />
          </Dropdown>
        </Space>
      </Header>
    </>
  );
}
