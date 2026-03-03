import { useEffect, useState } from "react";
import {
  Button,
  Col,
  DatePicker,
  Divider,
  Form,
  Input,
  message,
  Modal,
  Row,
  Space,
  Typography,
} from "antd";
import {
  CheckOutlined,
  ClockCircleOutlined,
  CloseOutlined,
  ExclamationCircleOutlined,
} from "@ant-design/icons";
import apiClient from "../../services/apiClient";
import dayjs from "dayjs";
import customParseFormat from "dayjs/plugin/customParseFormat";
import { useNavigate } from "react-router-dom";
dayjs.extend(customParseFormat);

const { Text } = Typography;
const { TextArea } = Input;

export function AddRemainder({ open, setOpen, documentId }: any) {
  const navigate = useNavigate();
  const [openConfirm, setOpenConfirm] = useState(false);
  const [confirmLoading, setConfirmLoading] = useState(false);
  const [form] = Form.useForm();

  const handleSubmit = () => {
    setOpenConfirm(true);
  };

  const handleOkAdd = () => {
    setConfirmLoading(true);
    const values = form.getFieldsValue();
    const dataStore = {
      ...values,
      reminderDateTime: values["reminderDateTime"].format(
        "YYYY-MM-DD HH:mm:ss"
      ),
      documentID: documentId,
    };
    apiClient
      .post("/documentreminder", dataStore)
      .then(({ data }) => {
        console.log(data);
        setConfirmLoading(false);
        setOpenConfirm(false);
        message.success("Create data is successful");
        form.resetFields();
        setOpen(false);
        navigate(0);
      })
      .catch((err) => {
        console.log(err);
        message.error(err.response.data.message);
        setConfirmLoading(false);
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
              <ClockCircleOutlined
                style={{
                  border: "1px solid #d9d9d9",
                  padding: 7,
                  fontSize: 30,
                  color: "#434343",
                  borderRadius: 7,
                }}
              />
              <Space.Compact direction="vertical">
                <span>Add Reminder</span>
                <Text type="secondary" style={{ fontWeight: "normal" }}>
                  The time specified below serves as a reminder.
                </Text>
              </Space.Compact>
            </Space>
          </div>
        }
        destroyOnHidden
        open={open}
        onCancel={() => setOpen(false)}
        maskClosable={false}
        footer={false}
        width={600}
        centered
        styles={{
          content: {
            padding: 0,
          },
        }}
      >
        <Divider style={{ marginBottom: 0 }} />
        <Form layout="vertical" form={form} onFinish={handleSubmit}>
          <div style={{ padding: 24, paddingBottom: 10 }}>
            <Form.Item
              label="Datetime"
              name="reminderDateTime"
              rules={[
                { type: "date" },
                { required: true, message: "required" },
              ]}
            >
              <DatePicker
                showTime
                format="DD/MM/YYYY HH:mm:ss"
                placeholder="DD/MM/YYYY HH:mm:ss"
                getPopupContainer={(trigger) =>
                  trigger.parentElement as HTMLElement
                }
              />
            </Form.Item>
            <Form.Item
              label="Description"
              name="reminderDesc"
              rules={[
                { type: "string" },
                { required: true, message: "required" },
              ]}
            >
              <TextArea rows={4} placeholder="enter description..." />
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
                icon={<ClockCircleOutlined />}
                iconPosition="end"
                htmlType="submit"
              >
                Set Reminder
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
        open={openConfirm}
        onCancel={() => setOpenConfirm(false)}
        maskClosable={false}
        footer={(_) => (
          <>
            <Row gutter={12}>
              <Col span={12}>
                <Button
                  icon={<CloseOutlined />}
                  iconPosition="end"
                  onClick={() => setOpenConfirm(false)}
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
                  loading={confirmLoading}
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
            Are you sure you want to add Reminder?
          </p>
          <span className="text-gray-500">
            Reminder automaticali set on document.
          </span>
        </div>
      </Modal>
    </>
  );
}

export function EditRemainder({
  open,
  setOpen,
  eventData,
  documentId,
  setEventDataRemainder,
}: any) {
  const navigate = useNavigate();
  const [openConfirm, setOpenConfirm] = useState(false);
  const [confirmLoading, setConfirmLoading] = useState(false);
  const [form] = Form.useForm();

  useEffect(() => {
    if (Object.keys(eventData).length > 0 && open) {
      const data = {
        ...eventData,
        reminderDateTime: dayjs.utc(eventData.reminderDateTime),
      };
      form.setFieldsValue(data);
    }
  }, [open, eventData]);

  useEffect(() => {
    if (!open) setEventDataRemainder({});
  }, [open]);

  const handleSubmit = () => {
    setOpenConfirm(true);
  };

  const handleOkAdd = () => {
    setConfirmLoading(true);
    const values = form.getFieldsValue();
    const dataUpdate = {
      ...values,
      reminderDateTime: values["reminderDateTime"].format(
        "YYYY-MM-DD HH:mm:ss"
      ),
      documentID: documentId,
    };
    apiClient
      .put("/documentreminder", dataUpdate)
      .then(({ data }) => {
        console.log(data);
        setConfirmLoading(false);
        setOpenConfirm(false);
        message.success("Update data is successful");
        form.resetFields();
        setOpen(false);
        navigate(0);
      })
      .catch((err) => {
        console.log(err);
        message.error(err.response.data.message);
        setConfirmLoading(false);
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
              <ClockCircleOutlined
                style={{
                  border: "1px solid #d9d9d9",
                  padding: 7,
                  fontSize: 30,
                  color: "#434343",
                  borderRadius: 7,
                }}
              />
              <Space.Compact direction="vertical">
                <span>Edit Reminder</span>
                <Text type="secondary" style={{ fontWeight: "normal" }}>
                  The time specified below serves as a reminder.
                </Text>
              </Space.Compact>
            </Space>
          </div>
        }
        destroyOnHidden
        open={open}
        onCancel={() => setOpen(false)}
        maskClosable={false}
        footer={false}
        width={600}
        centered
        styles={{
          content: {
            padding: 0,
          },
        }}
      >
        <Divider style={{ marginBottom: 0 }} />
        <Form layout="vertical" form={form} onFinish={handleSubmit}>
          <div style={{ padding: 24, paddingBottom: 10 }}>
            <Form.Item name="id" hidden>
              <Input />
            </Form.Item>
            <Form.Item
              label="Datetime"
              name="reminderDateTime"
              rules={[
                { type: "date" },
                { required: true, message: "required" },
              ]}
            >
              <DatePicker
                showTime
                format="DD/MM/YYYY HH:mm:ss"
                placeholder="DD/MM/YYYY HH:mm:ss"
                getPopupContainer={(trigger) =>
                  trigger.parentElement as HTMLElement
                }
              />
            </Form.Item>
            <Form.Item
              label="Description"
              name="reminderDesc"
              rules={[
                { type: "string" },
                { required: true, message: "required" },
              ]}
            >
              <TextArea rows={4} placeholder="enter description..." />
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
                icon={<ClockCircleOutlined />}
                iconPosition="end"
                htmlType="submit"
              >
                Edit Reminder
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
        open={openConfirm}
        onCancel={() => setOpenConfirm(false)}
        maskClosable={false}
        footer={(_) => (
          <>
            <Row gutter={12}>
              <Col span={12}>
                <Button
                  icon={<CloseOutlined />}
                  iconPosition="end"
                  onClick={() => setOpenConfirm(false)}
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
                  loading={confirmLoading}
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
            Are you sure you want to edit Reminder?
          </p>
          <span className="text-gray-500">
            Reminder automaticali set on document.
          </span>
        </div>
      </Modal>
    </>
  );
}
