import { useEffect, useState } from "react";
import {
  Button,
  Divider,
  Modal,
  Space,
  Typography,
  Form,
  DatePicker,
  Input,
  Select,
  Flex,
} from "antd";
import {
  CloseOutlined,
  FunnelPlotOutlined,
  SearchOutlined,
} from "@ant-design/icons";
import { useNavigate } from "react-router-dom";
import dayjs from "dayjs";
import customParseFormat from "dayjs/plugin/customParseFormat";
import apiClient from "../../../services/apiClient";
dayjs.extend(customParseFormat);

const { Text } = Typography;

const ModalAdvanceSearch = () => {
  const navigate = useNavigate();
  const [open, setOpen] = useState(false);
  const [form] = Form.useForm();
  const [categories, setCategories] = useState<
    Array<{ label: string; value: string }>
  >([]);
  const [loadingCategories, setLoadingCategories] = useState(false);

  useEffect(() => {
    if (open) {
      fetchCategories();
    }
  }, [open]);

  const fetchCategories = async () => {
    setLoadingCategories(true);
    try {
      const { data } = await apiClient.get("/dropdown/ddlcategories");
      const categoriesData = data?.data || [];
      const formattedCategories = categoriesData.map(
        (item: { text: string; value: string }) => ({
          label: item.text,
          value: item.value,
        })
      );
      // Add "All Category" option at the beginning
      setCategories([
        { label: "All Category", value: "" },
        ...formattedCategories,
      ]);
    } catch (err) {
      console.error("Error fetching categories:", err);
    } finally {
      setLoadingCategories(false);
    }
  };

  const handleSearch = () => {
    form
      .validateFields()
      .then((values) => {
        const queryParams = new URLSearchParams({
          DocumentTitle: values.document,
          DtFrom: values.date_from.format("YYYY-MM-DD"),
          DtTo: values.date_to.format("YYYY-MM-DD"),
          CategoryID: values.category || "",
          type: "advance",
        });
        // Clear form after search
        form.resetFields();
        setOpen(false);
        navigate(`/search?${queryParams}`);
      })
      .catch((errorInfo) => {
        console.log("error submit", errorInfo);
      });
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
              <FunnelPlotOutlined
                style={{
                  border: "1px solid #d9d9d9",
                  padding: 7,
                  fontSize: 30,
                  color: "#434343",
                  borderRadius: 7,
                }}
              />
              <Space.Compact direction="vertical">
                <span>Advance Search</span>
                <Text type="secondary" style={{ fontWeight: "normal" }}>
                  Fill in the form bellow according to the data you want to get
                </Text>
              </Space.Compact>
            </Space>
          </div>
        }
        open={open}
        onCancel={() => setOpen(false)}
        maskClosable={false}
        footer={() => (
          <>
            <Button
              icon={<CloseOutlined />}
              iconPosition="end"
              onClick={() => setOpen(false)}
              color="danger"
              variant="filled"
            >
              Cancel
            </Button>
            <Button
              type="primary"
              icon={<SearchOutlined />}
              iconPosition="end"
              onClick={handleSearch}
            >
              Search
            </Button>
          </>
        )}
        width={600}
        styles={{
          content: {
            padding: 0,
          },
          footer: {
            padding: 24,
          },
        }}
      >
        <Divider style={{ marginBottom: 0 }} />
        <div style={{ padding: 24, paddingBottom: 10 }}>
          <Form layout="vertical" form={form}>
            <Form.Item
              name="document"
              label="Document Text"
              rules={[{ type: "string" }, { required: true }]}
            >
              <Input placeholder="input title document..." />
            </Form.Item>
            <Flex gap="small" vertical>
              <span>Date Modified</span>
              <Space size="large">
                <Form.Item
                  name="date_from"
                  label="From"
                  rules={[{ required: true }]}
                >
                  <DatePicker format="YYYY-MM-DD" placeholder="YYYY-MM-DD" />
                </Form.Item>
                <Form.Item
                  name="date_to"
                  label="To"
                  rules={[{ required: true }]}
                >
                  <DatePicker format="YYYY-MM-DD" placeholder="YYYY-MM-DD" />
                </Form.Item>
              </Space>
            </Flex>
            <Form.Item name="category" label="Categories">
              <Select
                placeholder="Select categories"
                loading={loadingCategories}
                options={categories}
              />
            </Form.Item>
          </Form>
        </div>
        <Divider style={{ marginTop: 0, marginBottom: 0 }} />
      </Modal>
      <Button
        size="large"
        icon={<FunnelPlotOutlined style={{ fontSize: 20 }} />}
        onClick={() => setOpen(true)}
      />
    </>
  );
};

export default ModalAdvanceSearch;
