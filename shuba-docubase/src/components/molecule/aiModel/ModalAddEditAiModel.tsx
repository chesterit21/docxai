import { useEffect, useState } from "react";
import {
  Button,
  Divider,
  Form,
  Input,
  InputNumber,
  message,
  Modal,
  Select,
  Space,
  Typography,
} from "antd";
import {
  CheckOutlined,
  CloseOutlined,
  PlusOutlined,
  SaveOutlined,
} from "@ant-design/icons";
import apiClient from "../../../services/apiClient";

const { Text } = Typography;

export default function ModalAddEditAiModel({
  open,
  setOpen,
  rerenderData,
  eventData,
  afterOpenChange,
}: any) {
  const [confirmLoading, setConfirmLoading] = useState(false);
  const [form] = Form.useForm();
  const isEdit = !!eventData?.id;

  useEffect(() => {
    if (open && isEdit && eventData) {
      form.setFieldsValue({
        modelName: eventData.modelName,
        urlApi: eventData.urlApi,
        apiKey: eventData.apiKey,
        provider: eventData.provider,
        maxToken: eventData.maxToken,
      });
    } else if (open && !isEdit) {
      form.resetFields();
    }
  }, [open, eventData, isEdit, form]);

  const handleSubmit = () => {
    form.validateFields().then((values) => {
      if (isEdit) {
        handleUpdate(values);
      } else {
        handleCreate(values);
      }
    });
  };

  const handleCreate = (values: any) => {
    setConfirmLoading(true);
    apiClient
      .post("/AiModel", values)
      .then(() => {
        setConfirmLoading(false);
        setOpen(false);
        message.success("Create AI Model is successful");
        form.resetFields();
        rerenderData();
      })
      .catch((err) => {
        console.error(err);
        message.error(err.response?.data?.message || "Create AI Model failed");
        setConfirmLoading(false);
      });
  };

  const handleUpdate = (values: any) => {
    setConfirmLoading(true);
    apiClient
      .put("/AiModel", { ...values, id: eventData.id })
      .then(() => {
        setConfirmLoading(false);
        setOpen(false);
        message.success("Update AI Model is successful");
        form.resetFields();
        rerenderData();
      })
      .catch((err) => {
        console.error(err);
        message.error(err.response?.data?.message || "Update AI Model failed");
        setConfirmLoading(false);
      });
  };

  const handleAfterOpenChange = (visible: boolean) => {
    if (!visible) {
      form.resetFields();
    }
    if (afterOpenChange) {
      afterOpenChange(visible);
    }
  };

  return (
    <Modal
      title={
        <div
          className="space-align-block"
          style={{ paddingLeft: 24, paddingRight: 24, paddingTop: 24 }}
        >
          <Space align="center" size="middle">
            <SaveOutlined
              style={{
                border: "1px solid #d9d9d9",
                padding: 7,
                fontSize: 30,
                color: "#434343",
                borderRadius: 7,
              }}
            />
            <Space.Compact direction="vertical">
              <span>{isEdit ? "Edit AI Model" : "Add AI Model"}</span>
              <Text type="secondary" style={{ fontWeight: "normal" }}>
                {isEdit
                  ? "Edit AI model by filling in the form below."
                  : "Add new AI model by filling in the form below."}
              </Text>
            </Space.Compact>
          </Space>
        </div>
      }
      destroyOnHidden
      afterOpenChange={handleAfterOpenChange}
      open={open}
      onCancel={() => setOpen(false)}
      maskClosable={false}
      footer={false}
      width={600}
      zIndex={10}
      styles={{
        content: {
          padding: 0,
        },
      }}
    >
      <Divider style={{ marginBottom: 0 }} />
      <Form form={form} layout="vertical" onFinish={handleSubmit}>
        <div style={{ padding: 24, paddingBottom: 10 }}>
          <Form.Item
            label="Model Name"
            name="modelName"
            rules={[{ required: true, message: "Model name is required" }]}
          >
            <Input placeholder="e.g. GPT-4o" />
          </Form.Item>
          
          <Form.Item
            label="API URL"
            name="urlApi"
            rules={[{ required: true, message: "API URL is required" }]}
          >
            <Input placeholder="https://api.openai.com/v1/chat/completions" />
          </Form.Item>

          <Form.Item
            label="API Key"
            name="apiKey"
            rules={[{ required: true, message: "API Key is required" }]}
          >
            <Input.Password placeholder="enter your API key..." />
          </Form.Item>

          <Form.Item
            label="Provider"
            name="provider"
            rules={[{ required: true, message: "Provider is required" }]}
          >
            <Select placeholder="Select provider">
              <Select.Option value="Gemini">Gemini</Select.Option>
              <Select.Option value="ZAI">ZAI</Select.Option>
              <Select.Option value="Qwen">Qwen</Select.Option>
              <Select.Option value="DeepSeek">DeepSeek</Select.Option>
              <Select.Option value="Claude">Claude</Select.Option>
              <Select.Option value="OpenAI-ChatGPT">OpenAI-ChatGPT</Select.Option>
              <Select.Option value="Other-OpenAI-Style">Other-OpenAI-Style</Select.Option>
              <Select.Option value="Mistral">Mistral</Select.Option>
              <Select.Option value="Cohere">Cohere</Select.Option>
              <Select.Option value="OpenRouter">OpenRouter</Select.Option>
            </Select>
          </Form.Item>

          <Form.Item
            label="Max Token"
            name="maxToken"
            rules={[{ required: true, message: "Max token is required" }]}
          >
            <InputNumber style={{ width: '100%' }} min={1} placeholder="4096" />
          </Form.Item>
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
              icon={isEdit ? <CheckOutlined /> : <PlusOutlined />}
              iconPosition="end"
              htmlType="submit"
              loading={confirmLoading}
            >
              {isEdit ? "Save Change" : "Create AI Model"}
            </Button>
          </Space>
        </div>
      </Form>
    </Modal>
  );
}
