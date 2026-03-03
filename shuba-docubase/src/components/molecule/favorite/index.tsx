import type { TabsProps } from "antd";
import { Tabs } from "antd";
import TableListDocument from "./TableListDocument";
import TableListCategory from "./TableListCategory";
import Breadcrumb from "../../atom/Breadcrumb";
import { useEffect, useState } from "react";
import { useSearchParams } from "react-router-dom";

export default function Index() {
  const [searchParams, setSearchParams] = useSearchParams();
  const tabParam = searchParams.get("tab");
  const [activeTab, setActiveTab] = useState(
    tabParam === "category" ? "2" : "1"
  );

  useEffect(() => {
    const currentTabParam = searchParams.get("tab");
    setActiveTab(currentTabParam === "category" ? "2" : "1");
  }, [searchParams]);

  const handleTabChange = (key: string) => {
    setActiveTab(key);
    if (key === "2") {
      setSearchParams({ tab: "category" });
    } else {
      searchParams.delete("tab");
      setSearchParams(searchParams);
    }
  };

  const items: TabsProps["items"] = [
    {
      label: "Document",
      key: "1",
      children: <TableListDocument />,
    },
    {
      label: "Category",
      key: "2",
      children: <TableListCategory />,
    },
  ];

  return (
    <>
      <Breadcrumb
        item={[
          {
            title: (
              <span style={{ fontWeight: "bold" }}>
                Favorite Document & Category
              </span>
            ),
          },
        ]}
      />
      <Tabs
        defaultActiveKey="1"
        type="card"
        size="middle"
        style={{ marginBottom: 32, marginTop: 39 }}
        items={items}
        activeKey={activeTab}
        onChange={handleTabChange}
      />
    </>
  );
}
