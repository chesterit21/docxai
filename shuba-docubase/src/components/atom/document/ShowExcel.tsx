import { useEffect, useRef, useState } from "react";
import {
  Button,
  Col,
  Divider,
  Form,
  Modal,
  Row,
  Space,
  Typography,
  message,
  Checkbox,
} from "antd";
import {
  CheckOutlined,
  CloseOutlined,
  ExclamationCircleOutlined,
  FileExcelFilled,
  DownloadOutlined,
  SaveOutlined,
} from "@ant-design/icons";
import { SpreadsheetComponent } from "@syncfusion/ej2-react-spreadsheet";
import { Workbook } from '@syncfusion/ej2-excel-export';
import { type SaveType, type BlobSaveType } from '@syncfusion/ej2-excel-export';
import { registerLicense } from "@syncfusion/ej2-base";
import apiClient from "../../../services/apiClient";
import { useNavigate } from "react-router-dom";
registerLicense(
  "Ngo9BigBOggjHTQxAR8/V1NNaF5cXmBCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWXtdcHVVQ2FZWUBxXkFWYUA="
);

const { Text } = Typography;

export default function Index({ open, setOpen, file }: any) {
  const navigate = useNavigate();
  const [openConfirm, setOpenConfirm] = useState(false);
  const [confirmLoading, setConfirmLoading] = useState(false);
  const [showEditor, setShowEditor] = useState(false);
  const spreadsheetRef = useRef<SpreadsheetComponent>(null);
  const fileUrl = import.meta.env.VITE_API_URL + file.documentFilePath;
  const serviceUrl = import.meta.env.VITE_API_URL + "/ej2spreedsheet/";
  const [saveAsNew, setSaveAsNew] = useState<boolean>(false);

  useEffect(() => {
    if (!open) {
      setShowEditor(false);
      setSaveAsNew(false);
    }
  }, [open]);

  const handleSubmit = () => {
    setOpenConfirm(true);
  };

  const handleOkAdd = async () => {
    setConfirmLoading(true);
    try {
      // Get the file from spreadsheet
      if (spreadsheetRef.current) {
        // Save the spreadsheet as blob
        let filename = file.documentFileName + file.documentType;
        spreadsheetRef.current.save({ fileName: filename });
        const jsonData = spreadsheetRef.current.saveAsJson();
        // Step 2: Create a Workbook instance 
        const workbook = new Workbook(jsonData, 'xlsx' as SaveType);
        const blobResult = await workbook.saveAsBlob("xlsx" as BlobSaveType);
        const blob: Blob = blobResult.blobData;
        const newFile = new File([blob], file.documentFileName + file.documentType, { type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", });

        const formData = new FormData();
        formData.append("fileUpload", newFile);
        if (saveAsNew) {
          // Upload as new version
          formData.append("DocumentID", file.documentID);
          formData.append("fileUpload", newFile);
          apiClient
            .post(`/documents/upload-new-version`, formData, {
              headers: {
                "Content-Type": "multipart/form-data",
              },
            })
            .then(() => {
              message.success("Edit file is successful");
              setConfirmLoading(false);
              setOpenConfirm(false);
              setOpen(false);
              navigate(0);
            })
            .catch((err) => {
              console.error(err);
              message.error("Edit file is failed");
              setConfirmLoading(false);
            });
        } else {
          // Update existing file
          formData.append("file", newFile);
          apiClient
            .put(
              `/documentfiles?Id=${file.id}&DocumentID=${file.documentID}`,
              formData,
              {
                headers: {
                  "Content-Type": "multipart/form-data",
                },
              }
            )
            .then(() => {
              message.success("Edit file is successful");
              setConfirmLoading(false);
              setOpenConfirm(false);
              setOpen(false);
              navigate(0);
            })
            .catch((err) => {
              console.error(err);
              message.error("Edit file is failed");
              setConfirmLoading(false);
            });
        }
      }
    } catch (error) {
      console.error("Save error:", error);
      message.error("Failed to save file");
      setConfirmLoading(false);
    }
  };

  const handleSaveComplete = (args: any) => {
    if (args.status === "Success") {
      message.success("File saved successfully");
      setConfirmLoading(false);
      setOpenConfirm(false);
      setOpen(false);
      navigate(0);
    } else {
      message.error("Failed to save file");
      setConfirmLoading(false);
      setOpenConfirm(false);
    }
  };

  const handleDownload = async () => {
    try {
      const response = await fetch(fileUrl);
      if (!response.ok) {
        throw new Error("Download failed");
      }
      const blob = await response.blob();
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement("a");
      link.href = url;
      link.download = file?.documentFileName || "file.xlsx";
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      window.URL.revokeObjectURL(url);
      message.success("File downloaded successfully");
    } catch (error) {
      console.error("Download error:", error);
      message.error("Failed to download file");
    }
  };

  const handleAfterOpenChange = (visible: boolean) => {
    if (visible) {
      setShowEditor(true);
      setTimeout(async () => {
        const response = await fetch(fileUrl);
        const buffer = await response.arrayBuffer();
        const file = new File([buffer], "sample.xlsx", {
          type:
            response.headers.get("Content-Type") ||
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        });
        spreadsheetRef.current?.open({ file });
      }, 200);
    }
  };

  const handleBeforeOpen = (args: any) => {
    if (args.formData) {
      args.formData.append("documentFileID", file.id);
    }
  };

  const handleBeforeSave = (args: any) => {
    args.isFullPost = false;
    args.customParams = {
      documentFileID: file.id,
      isSaveAsNewVersion: saveAsNew,
    };
  };

  const handleBeforeSend = (args: any) => {
    // Add Authorization header
    args.ajaxSettings.headers = {
      Authorization: "Bearer " + localStorage.getItem("token"),
    };
  };

  return (
    <div>
      <Modal
        title={
          <div
            className="space-align-block"
            style={{ paddingLeft: 24, paddingRight: 24, paddingTop: 24 }}
          >
            <Space align="center" size="middle">
              <FileExcelFilled style={{ fontSize: 40, color: "#52c41a" }} />
              <Space.Compact direction="vertical">
                <span>{file?.documentFileName || "-"}</span>
                <Text type="secondary" style={{ fontWeight: "normal" }}>
                  Show Document
                </Text>
              </Space.Compact>
            </Space>
          </div>
        }
        open={open}
        onCancel={() => setOpen(false)}
        maskClosable={false}
        footer={false}
        width={1200}
        zIndex={1}
        style={{ top: 20 }}
        styles={{
          content: {
            padding: 0,
          },
        }}
        destroyOnHidden
        afterOpenChange={handleAfterOpenChange}
      >
        <Form layout="vertical" onFinish={handleSubmit}>
          <Divider style={{ marginBottom: 0 }} />
          {showEditor && (
            <SpreadsheetComponent
              height={"730px"}
              ref={spreadsheetRef}
              beforeOpen={handleBeforeOpen}
              beforeSave={handleBeforeSave}
              // @ts-expect-error: Missing prop definition in wrapper
              beforeSend={handleBeforeSend}
              allowOpen={true}
              // openUrl="https://services.syncfusion.com/react/production/api/spreadsheet/open"
              openUrl={serviceUrl + "open"}
              saveUrl={serviceUrl + "save"}
              saveComplete={handleSaveComplete}
            />
          )}
          <Divider style={{ marginTop: 0, marginBottom: 0 }} />
          <div style={{ padding: 20, paddingBottom: 20, justifySelf: "end" }}>
            <Space>
              <Checkbox
                onChange={(e) => setSaveAsNew(e.target.checked)}
                checked={saveAsNew}
              >
                Save as new version?
              </Checkbox>
              <Button
                icon={<DownloadOutlined />}
                iconPosition="end"
                onClick={handleDownload}
              >
                Download
              </Button>
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
                onClick={handleSubmit}
              >
                Save
              </Button>
            </Space>
          </div>
        </Form>
      </Modal>
      {/* modal confirm edit */}
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
        zIndex={5}
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
            Do you want to save this document?
          </p>
          <span className="text-gray-500">
            The file you uploaded will be saved as part of your document(s).
          </span>
        </div>
      </Modal>
    </div>
  );
}
