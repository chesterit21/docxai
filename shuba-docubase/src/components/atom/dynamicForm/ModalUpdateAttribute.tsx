import { useEffect, useState } from "react";
import {
  Button,
  Col,
  Divider,
  Modal,
  Row,
  Space,
  Typography,
  Form,
  Select,
  Input,
  Checkbox,
  InputNumber,
  message,
} from "antd";
import {
  CheckOutlined,
  CloseOutlined,
  ExclamationCircleOutlined,
  MinusCircleOutlined,
  PlusOutlined,
} from "@ant-design/icons";
const { Text } = Typography;
const { Option } = Select;
import apiClient from "../../../services/apiClient";

const generateName = (label: string) =>
  label.toLowerCase().replace(/\s+/g, "_").replace(/[^\w]/g, "");
const generateLabel = (label: string) => {
  return label
    .toLowerCase()
    .split(" ")
    .map((word) => {
      return word.charAt(0).toUpperCase() + word.slice(1);
    })
    .join(" ");
};

export default function Index({
  open,
  setOpen,
  evenData,
  setEventData,
  rerenderData,
}: any) {
  const [openConfirmAdd, setOpenConfirmAdd] = useState(false);
  const [confirmAddLoading, setConfirmAddLoading] = useState(false);
  const [form] = Form.useForm();

  useEffect(() => {
    if (
      Object.keys(evenData).length > 0 &&
      Object.keys(JSON.parse(evenData.attributeElement)).length > 0
    ) {
      form.setFieldsValue(JSON.parse(evenData.attributeElement));
    }
  }, [evenData, form]);

  const handleSubmit = () => {
    setOpenConfirmAdd(true);
  };

  const handleOkAdd = async () => {
    setConfirmAddLoading(true);
    const values = await form.validateFields();
    const name = generateName(values.label);
    const label = generateLabel(values.label);
    const element = { ...values, name: name, label: label };
    const { id, attributeType } = evenData;
    const dataUpdate = {
      id: id,
      attributeType,
      attributeName: label,
      attributeElement: JSON.stringify(element),
    };
    console.log(dataUpdate);

    setConfirmAddLoading(true);
    apiClient
      .put("/attribute", dataUpdate)
      .then(({ data }) => {
        console.log(data);
        setOpen(false);
        setConfirmAddLoading(false);
        setOpenConfirmAdd(false);
        rerenderData();
        message.success("Update data is successful");
      })
      .catch((err) => {
        console.log(err);
        message.error(err.response.data.message);
        setConfirmAddLoading(false);
        setOpenConfirmAdd(false);
      });
  };

  const opt = [
    { value: "text-field", label: "Text Field" },
    { value: "text-area", label: "Text Area" },
    { value: "number", label: "Number" },
    { value: "date", label: "Date" },
    { value: "select", label: "Select" },
    { value: "checkbox", label: "Checkbox Group" },
    { value: "radio", label: "Radio Group" },
  ];

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
                <span>Edit Attribute</span>
                <Text type="secondary" style={{ fontWeight: "normal" }}>
                  Update Attribute by selecting and filling in the from below.
                </Text>
              </Space.Compact>
            </Space>
          </div>
        }
        open={open}
        onCancel={() => [setEventData({}), form.resetFields(), setOpen(false)]}
        maskClosable={false}
        destroyOnHidden
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
        <Form
          layout="vertical"
          onFinish={handleSubmit}
          form={form}
          name="dynamic_form_nest_item"
        >
          <div className="px-7 py-5">
            <Form.Item name="id" hidden>
              <Input />
            </Form.Item>

            <Form.Item label="Tipe Field" name="type">
              <Select options={opt} disabled />
            </Form.Item>

            <Form.Item label="Label" name="label" rules={[{ required: true }]}>
              {evenData.isSystem ? (
                <div className="rounded-md bg-gray-100 py-1 px-2 text-gray-500 border border-gray-200">
                  {evenData.attributeName}
                </div>
              ) : (
                <Input />
              )}
            </Form.Item>

            {evenData.attributeType !== "checkbox" &&
              evenData.attributeType !== "radio" && (
                <Form.Item label="Placeholder" name="placeholder">
                  <Input />
                </Form.Item>
              )}

            <Form.Item label="Help Text" name="helptext">
              <Input />
            </Form.Item>

            {(evenData.attributeType === "select" ||
              evenData.attributeType === "checkbox" ||
              evenData.attributeType === "radio") && (
              <Form.List name="options">
                {(fields, { add, remove }) => {
                  return (
                    <>
                      {fields.map(({ key, name, ...restField }) => (
                        <Space key={key} style={{ display: "flex" }}>
                          <Form.Item
                            {...restField}
                            name={[name, "opt"]}
                            label={`Option ${name + 1}`}
                            rules={[
                              { required: true, message: "Missing value" },
                            ]}
                          >
                            <Input placeholder={`enter option ${name + 1}`} />
                          </Form.Item>
                          {name >= 1 && (
                            <MinusCircleOutlined onClick={() => remove(name)} />
                          )}
                        </Space>
                      ))}
                      <Form.Item>
                        <Button
                          type="dashed"
                          onClick={() => add()}
                          iconPosition="end"
                          block
                          icon={<PlusOutlined />}
                        >
                          Add option
                        </Button>
                      </Form.Item>
                    </>
                  );
                }}
              </Form.List>
            )}

            {(evenData.attributeType === "text-field" ||
              evenData.attributeType === "text-area" ||
              evenData.attributeType === "number") && (
              <>
                {evenData.attributeType === "number" && (
                  <Form.Item
                    label="Min Length"
                    name="min"
                    rules={[{ required: true }]}
                  >
                    <InputNumber style={{ width: "100%" }} />
                  </Form.Item>
                )}
                <Form.Item
                  label="Max Length"
                  name="max"
                  rules={[{ required: true }]}
                >
                  <InputNumber style={{ width: "100%" }} min={1} />
                </Form.Item>
              </>
            )}

            {evenData.attributeType === "date" && (
              <Form.Item
                name="format"
                label="Format Tanggal"
                rules={[{ required: true }]}
              >
                <Select
                  style={{ width: "100%" }}
                  placeholder="Select format date"
                >
                  <Option value="DD/MM/YYYY">
                    DD/MM/YYYY{" "}
                    <span className="text-slate-500">(25/05/2025)</span>
                  </Option>
                  <Option value="MM-DD-YYYY">
                    MM-DD-YYYY{" "}
                    <span className="text-slate-500">(05-25-2025)</span>
                  </Option>
                  <Option value="D MMMM YYYY">
                    D MMMM YYYY{" "}
                    <span className="text-slate-500">(25 May 2025)</span>
                  </Option>
                </Select>
              </Form.Item>
            )}

            {evenData.attributeType === "file" && (
              <>
                <Form.Item
                  label="File Type"
                  name="allowedTypes"
                  rules={[{ required: true }]}
                >
                  <Select
                    style={{ width: "100%" }}
                    placeholder="pdf, doc, docx"
                  >
                    <Option value="pdf">PDF</Option>
                    <Option value="docx">Word</Option>
                    <Option value="xlsx">Excel</Option>
                  </Select>
                </Form.Item>

                <Form.Item
                  label="Max Size (MB)"
                  name="maxSizeMB"
                  rules={[{ required: true }]}
                >
                  <InputNumber min={1} max={5} style={{ width: "100%" }} />
                </Form.Item>
              </>
            )}
          </div>
          <Divider style={{ marginTop: 0, marginBottom: 0 }} />
          <div style={{ padding: 20, paddingBottom: 20, justifySelf: "end" }}>
            <Space>
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
                icon={<CheckOutlined />}
                iconPosition="end"
                htmlType="submit"
              >
                Save Change
              </Button>
            </Space>
          </div>
        </Form>
      </Modal>
      {/* modal confirm add */}
      <Modal
        title={
          <div style={{ paddingLeft: 24, paddingRight: 24, paddingTop: 24 }}>
            <ExclamationCircleOutlined
              style={{ fontSize: 30, color: "gold" }}
            />
          </div>
        }
        open={openConfirmAdd}
        onCancel={() => setOpenConfirmAdd(false)}
        maskClosable={false}
        destroyOnClose
        footer={(_) => (
          <>
            <Row gutter={12}>
              <Col span={12}>
                <Button
                  icon={<CloseOutlined />}
                  iconPosition="end"
                  onClick={() => setOpenConfirmAdd(false)}
                  variant="filled"
                  block
                >
                  Cancel
                </Button>
              </Col>
              <Col span={12}>
                <Button
                  type="primary"
                  icon={<CheckOutlined />}
                  iconPosition="end"
                  onClick={handleOkAdd}
                  block
                  loading={confirmAddLoading}
                >
                  Confirm
                </Button>
              </Col>
            </Row>
          </>
        )}
        width={400}
        styles={{
          content: {
            padding: 0,
          },
          footer: {
            padding: 20,
          },
        }}
      >
        <div style={{ padding: "10px 24px" }}>
          <p className="font-bold text-lg" style={{ marginBottom: 0 }}>
            Are you sure you want to update an Attribute?
          </p>
          <span className="text-gray-500">
            Make sure this Attribute suits your needs.
          </span>
        </div>
      </Modal>
    </>
  );
}
