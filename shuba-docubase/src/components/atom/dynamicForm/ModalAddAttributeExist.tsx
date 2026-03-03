import { useEffect, useState } from "react";
import {
  Button,
  Col,
  Divider,
  Modal,
  Row,
  Space,
  Typography,
  Input,
  Table,
} from "antd";
import {
  CloseOutlined,
  MenuFoldOutlined,
  PlusOutlined,
  SearchOutlined,
} from "@ant-design/icons";
const { Text } = Typography;
import apiClient from "../../../services/apiClient";

export default function Index({
  open,
  setOpen,
  attributeExisting,
  setAttributeExisting,
}: any) {
  const [listAttributeAll, setListAttributeAll] = useState<any>([]);
  const [listAttribute, setListAttribute] = useState<any>([]);
  const [loading, setLoading] = useState<boolean>(false);
  const [search, setSearch] = useState<string>("");
  const columns = [
    {
      key: "attributeName",
      dataIndex: "attributeName",
      title: "Attribute Name",
      render: (items: any) => (
        <Space>
          <MenuFoldOutlined />
          <span className="font-bold">{items}</span>
        </Space>
      ),
    },
    {
      key: "attributeType",
      dataIndex: "attributeType",
      title: "Attribute Type",
    },
  ];

  const getData = async () => {
    setLoading(true);
    try {
      const { data: allAttrRes } = await apiClient.get(
        `/attribute?Page=1&Limit=10000`
      );

      const filteredAttributes = allAttrRes.data.data.filter(
        (a: any) => !attributeExisting.some((b: any) => b.id === a.id)
      );

      setListAttributeAll(filteredAttributes);
      setLoading(false);
    } catch (err) {
      console.error(err);
      setLoading(false);
    }
  };

  useEffect(() => {
    if (open) {
      getData();
    }
  }, [open]);

  useEffect(() => {
    setListAttribute(listAttributeAll);
  }, [listAttributeAll]);

  const handleClickRow = (row: any) => {
    setSearch("");

    const attributeName = row.attributeName;

    // If Expired Date is selected, also add Days Of Reminder if it exists and isSystem = true
    if (
      attributeName === "Expired Date" ||
      attributeName === "Days Of Reminder"
    ) {
      const otherAttrName =
        attributeName === "Expired Date" ? "Days Of Reminder" : "Expired Date";
      const otherAttr = listAttributeAll.find(
        (attr: any) =>
          attr.attributeName === otherAttrName && attr.isSystem === true
      );

      if (otherAttr) {
        // Add both to existing attributes
        setAttributeExisting((prev: any) => [...prev, row, otherAttr]);

        // Remove both from available list
        setListAttributeAll((prev: any) =>
          prev.filter((i: any) => i.id !== row.id && i.id !== otherAttr.id)
        );
        return; // Exit early since we've handled both attributes
      }
    }

    // Normal flow for other attributes
    setListAttributeAll((prev: any) => prev.filter((i: any) => i.id != row.id));
    setAttributeExisting((prev: any) => [...prev, row]);
  };

  const handleKeyDown = (event: any) => {
    if (event.key === "Enter" || event.code === "Enter") {
      const str = search.length > 0 ? search.toLocaleLowerCase() : "";
      const dataFilter = listAttributeAll.filter((i: any) =>
        i.attributeName.toLocaleLowerCase().includes(str)
      );
      setListAttribute(dataFilter);
    }
  };

  return (
    <>
      <Modal
        title={
          <div
            className="space-align-block"
            style={{ paddingLeft: 24, paddingRight: 24, paddingTop: 24 }}
          >
            <Space align="center" size="middle">
              <PlusOutlined
                style={{
                  border: "1px solid #d9d9d9",
                  padding: 7,
                  fontSize: 30,
                  color: "#434343",
                  borderRadius: 7,
                }}
              />
              <Space.Compact direction="vertical">
                <span>Add Attribute Existing</span>
                <Text type="secondary" style={{ fontWeight: "normal" }}>
                  Select a attribute existing and append the attribute in
                  document automatically.
                </Text>
              </Space.Compact>
            </Space>
          </div>
        }
        open={open}
        onCancel={() => setOpen(false)}
        destroyOnHidden
        maskClosable={false}
        footer={false}
        width={600}
        styles={{
          content: {
            padding: 0,
          },
        }}
        style={{ top: 20 }}
      >
        <Divider style={{ marginBottom: 0 }} />
        <div className="px-7 py-5">
          <Row gutter={[12, 12]}>
            <Col span={24}>
              <Input
                onChange={(e: any) => setSearch(e.target.value)}
                onKeyDown={handleKeyDown}
                size="large"
                value={search}
                placeholder="Search ..."
                prefix={<SearchOutlined />}
              />
            </Col>
            <Col span={24}>
              <Table
                pagination={false}
                columns={columns}
                dataSource={listAttribute}
                rowKey={(record: any) => record.id}
                onRow={(record, _) => {
                  return {
                    onClick: () => {
                      handleClickRow(record);
                    },
                  };
                }}
                loading={loading}
                rowClassName="hover:cursor-pointer"
                className="!max-h-[450px] !overflow-y-auto"
              />
            </Col>
          </Row>
        </div>
        <Divider style={{ marginTop: 0, marginBottom: 0 }} />
        <div style={{ padding: 20, paddingBottom: 20, justifySelf: "end" }}>
          <Button
            icon={<CloseOutlined />}
            iconPosition="end"
            onClick={() => setOpen(false)}
            variant="filled"
            block
          >
            Close
          </Button>
        </div>
      </Modal>
    </>
  );
}
