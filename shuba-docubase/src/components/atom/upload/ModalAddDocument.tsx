import { useState, useEffect } from "react";
import {
  Button,
  Divider,
  Form,
  Input,
  Modal,
  Space,
  message,
  Upload,
  Typography,
  Row,
  Col,
  Select,
} from "antd";
import {
  CheckOutlined,
  CloseOutlined,
  CloudUploadOutlined,
  ExclamationCircleOutlined,
  PlusOutlined,
  SaveOutlined,
} from "@ant-design/icons";

import type { UploadProps } from "antd";
import apiClient from "../../../services/apiClient";
const { Dragger } = Upload;
const { Text } = Typography;
const { TextArea } = Input;

export default function Index({
  open,
  setOpen,
  categoryId,
  rerenderData,
}: any) {
  const [fileList, setFileList] = useState<any[]>([]);
  const [openConfirm, setOpenConfirm] = useState(false);
  const [confirmLoading, setConfirmLoading] = useState(false);
  const [form] = Form.useForm();
  const [watermarkOptions, setWatermarkOptions] = useState<any[]>([]);

  const allowedTypes = [
    "image/jpeg",
    "image/png",
    "image/jpg",
    "application/pdf",
    "application/msword", // .doc
    "application/vnd.openxmlformats-officedocument.wordprocessingml.document", // .docx
    "application/vnd.ms-excel", // .xls
    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", // .xlsx
    "text/plain",
    "application/vnd.ms-powerpoint", // .ppt
    "application/vnd.openxmlformats-officedocument.presentationml.presentation", // .pptx
  ];

  useEffect(() => {
    if (open) {
      fetchWatermarks();
    }
  }, [open]);

  const fetchWatermarks = async () => {
    try {
      const { data } = await apiClient.get("/dropdown/ddlwatermarks");
      setWatermarkOptions(data?.data || []);
    } catch (err) {
      console.error("Error fetching watermarks:", err);
    }
  };

  const handleSubmit = () => {
    if (fileList.length === 0) {
      return message.warning("Please select a file");
    }
    setOpenConfirm(true);
  };

  const handleOkAdd = async () => {
    try {
      const values = await form.validateFields();

      if (fileList.length === 0) {
        message.warning("Please select a file first!");
        return;
      }

      const { DocumentTitle, DocumentDesc, watermarkID } = values;
      const formData = new FormData();
      formData.append("DocFile", fileList[0]);

      const queryParams = new URLSearchParams({
        CategoryID: categoryId || "",
        DocumentTitle: DocumentTitle || "",
        DocumentDesc: DocumentDesc || "",
      });
      if (watermarkID) {
        queryParams.append("watermarkID", watermarkID);
      }

      setConfirmLoading(true);

      await apiClient.post(
        `/documents/add-initial-document?${queryParams.toString()}`,
        formData,
        {
          headers: {
            "Content-Type": "multipart/form-data",
          },
        }
      );
      message.success("Upload successful");
      setFileList([]);
      form.resetFields();
      setOpenConfirm(false);
      setOpen(false);
      rerenderData();
    } catch (err) {
      message.error("Upload is failed");
    } finally {
      setConfirmLoading(false);
      setOpenConfirm(false);
    }
  };

  const onModalClose = () => {
    form.resetFields();
    setFileList([]);
  };

  const props: UploadProps = {
    multiple: false,
    beforeUpload: (file) => {
      if (!allowedTypes.includes(file.type)) {
        message.error("File extension not allowed!");
        return Upload.LIST_IGNORE;
      }

      const isLt2MB = file.size / 1024 / 1024 < 20;
      if (!isLt2MB) {
        message.error("File size cannot be more than 20 MB!");
        return Upload.LIST_IGNORE;
      }

      setFileList([file]);
      return false;
    },
    onRemove: () => {
      setFileList([]);
    },
    fileList,
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
                <span>Add Document</span>
                <Text type="secondary" style={{ fontWeight: "normal" }}>
                  Upload Document with input field below.
                </Text>
              </Space.Compact>
            </Space>
          </div>
        }
        open={open}
        onCancel={() => setOpen(false)}
        afterClose={onModalClose}
        maskClosable={false}
        footer={false}
        width={600}
        style={{ top: 20 }}
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
              label="Title"
              name="DocumentTitle"
              rules={[{ type: "string" }, { required: true }]}
            >
              <Input placeholder="enter title..." />
            </Form.Item>
            <Form.Item
              label="File Upload"
              name="file"
              rules={[{ required: true }]}
            >
              <Dragger {...props}>
                <p className="">
                  <CloudUploadOutlined style={{ fontSize: 40 }} />
                </p>
                <p className="ant-upload-text">
                  <span className="font-bold text-blue-700">
                    Click to upload
                  </span>{" "}
                  or drag and drop
                </p>
                <p className="ant-upload-hint">
                  jpg, jpeg, png, pdf, doc, docx, xls, xlsx, txt, ppt, pptx
                  (max. 20MB)
                </p>
              </Dragger>
            </Form.Item>
            <Form.Item
              label="Description"
              name="DocumentDesc"
              rules={[{ type: "string" }, { required: false }]}
            >
              <TextArea rows={4} placeholder="enter a description..." />
            </Form.Item>
            <Form.Item label="Watermark" name="watermarkID">
              <Select
                placeholder="Select watermark (optional)"
                allowClear
                options={watermarkOptions.map((opt: any) => ({
                  value: opt.value,
                  label: opt.text,
                }))}
              />
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
                icon={<SaveOutlined />}
                iconPosition="end"
                htmlType="submit"
              >
                Save
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
            Are you sure you want to add a new document?
          </p>
          <span className="text-gray-500">
            The file you uploaded will be saved as a new document.
          </span>
        </div>
      </Modal>
    </>
  );
}
