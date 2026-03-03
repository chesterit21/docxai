import { useEffect, useState } from "react";
import {
  Button,
  Divider,
  Form,
  Input,
  message,
  Modal,
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
const { TextArea } = Input;

export default function ModalAddEditWatermark({
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
        text: eventData.text,
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
      .post("/watermarks", values)
      .then(({ data }) => {
        console.log(data);
        setConfirmLoading(false);
        setOpen(false);
        message.success("Create watermark is successful");
        form.resetFields();
        rerenderData();
      })
      .catch((err) => {
        console.log(err);
        message.error(err.response?.data?.message || "Create watermark failed");
        setConfirmLoading(false);
      });
  };

  const handleUpdate = (values: any) => {
    setConfirmLoading(true);
    apiClient
      .put("/watermarks", { ...values, id: eventData.id })
      .then(({ data }) => {
        console.log(data);
        setConfirmLoading(false);
        setOpen(false);
        message.success("Update watermark is successful");
        form.resetFields();
        rerenderData();
      })
      .catch((err) => {
        console.log(err);
        message.error(err.response?.data?.message || "Update watermark failed");
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
              <span>{isEdit ? "Edit Watermark" : "Add Watermark"}</span>
              <Text type="secondary" style={{ fontWeight: "normal" }}>
                {isEdit
                  ? "Edit watermark by filling in the form below."
                  : "Add new watermark by filling in the form below."}
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
            label="Text"
            name="text"
            rules={[{ type: "string" }, { required: true }]}
          >
            <TextArea rows={4} placeholder="enter watermark text..." />
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
              {isEdit ? "Save Change" : "Create Watermark"}
            </Button>
          </Space>
        </div>
      </Form>
    </Modal>
  );
}
