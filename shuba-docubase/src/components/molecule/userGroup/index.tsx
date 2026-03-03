import type { TabsProps } from "antd";
import { Tabs } from "antd";
import TableListUser from "./TableUser";
import TableListGroup from "./TableGroup";
import { Col, Flex, Row, Space } from "antd";
import Breadcrumb from "../../atom/Breadcrumb";
import { useEffect, useState } from "react";
import { useSearchParams } from "react-router-dom";

export default function Index() {
  const [searchParams, setSearchParams] = useSearchParams();
  const tabParam = searchParams.get("tab");
  const [activeTab, setActiveTab] = useState(tabParam === "group" ? "2" : "1");

  useEffect(() => {
    const currentTabParam = searchParams.get("tab");
    setActiveTab(currentTabParam === "group" ? "2" : "1");
  }, [searchParams]);

  const handleTabChange = (key: string) => {
    setActiveTab(key);
    if (key === "2") {
      setSearchParams({ tab: "group" });
    } else {
      searchParams.delete("tab");
      setSearchParams(searchParams);
    }
  };

  const items: TabsProps["items"] = [
    {
      label: "User",
      key: "1",
      children: <TableListUser />,
    },
    {
      label: "Group",
      key: "2",
      children: <TableListGroup />,
    },
  ];

  return (
    <>
      <Breadcrumb
        item={[
          { title: <span style={{ fontWeight: "bold" }}>Users & Group</span> },
        ]}
      />
      <Row style={{ marginBottom: 20 }}>
        <Col span={24}>
          <Flex justify="space-between">
            <span className="text-2xl font-bold">User & Group Management</span>
            <Space size="large"></Space>
          </Flex>
        </Col>
      </Row>
      <Tabs
        defaultActiveKey="1"
        type="card"
        size="middle"
        style={{ marginBottom: 32 }}
        activeKey={activeTab}
        onChange={handleTabChange}
        items={items}
      />
    </>
  );
}
