import { useEffect, useRef, useState } from "react";
import { Button, Divider, Modal, Space, Typography, message } from "antd";
import {
  CloseOutlined,
  FileExcelFilled,
  DownloadOutlined,
} from "@ant-design/icons";
import { SpreadsheetComponent } from "@syncfusion/ej2-react-spreadsheet";
import { registerLicense } from "@syncfusion/ej2-base";
registerLicense(
  "Ngo9BigBOggjHTQxAR8/V1NNaF5cXmBCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWXtdcHVVQ2FZWUBxXkFWYUA="
);

const { Text } = Typography;

export default function Index({ open, setOpen, file }: any) {
  const [showEditor, setShowEditor] = useState(false);
  const spreadsheetRef = useRef<SpreadsheetComponent>(null);
  const fileUrl = import.meta.env.VITE_API_URL + file.documentFilePath;
  const serviceUrl = import.meta.env.VITE_API_URL + "/ej2spreedsheet/";

  useEffect(() => {
    if (!open) {
      setShowEditor(false);
    }
  }, [open]);

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
        style={{ top: 20 }}
        styles={{
          content: {
            padding: 0,
          },
        }}
        destroyOnHidden
        afterOpenChange={handleAfterOpenChange}
      >
        <Divider style={{ marginBottom: 0 }} />
        {showEditor && (
          <SpreadsheetComponent
            height={"730px"}
            ref={spreadsheetRef}
            allowOpen={true}
            openUrl={serviceUrl + "open"}
            saveUrl={serviceUrl + "save"}
          />
        )}
        <Divider style={{ marginTop: 0, marginBottom: 0 }} />
        <div style={{ padding: 20, paddingBottom: 20, justifySelf: "end" }}>
          <Space>
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
          </Space>
        </div>
      </Modal>
    </div>
  );
}
