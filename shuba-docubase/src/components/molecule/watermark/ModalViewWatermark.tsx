import { Modal, Space, Typography, Divider } from "antd";
import { CloseOutlined, EyeOutlined } from "@ant-design/icons";
import { Button } from "antd";
import dayjs from "dayjs";
import customParseFormat from "dayjs/plugin/customParseFormat";
import { useAuth } from "../../../context/AuthContext";
dayjs.extend(customParseFormat);

const { Text } = Typography;

export default function ModalViewWatermark({ open, setOpen, eventData }: any) {
  const { config } = useAuth();
  const dateConfig =
    config?.dataGrid?.dateTimeFormat || "YYYY-MM-DD | HH:mm:ss";

  return (
    <Modal
      title={
        <div
          className="space-align-block"
          style={{ paddingLeft: 24, paddingRight: 24, paddingTop: 24 }}
        >
          <Space align="center" size="middle">
            <EyeOutlined
              style={{
                border: "1px solid #d9d9d9",
                padding: 7,
                fontSize: 30,
                color: "#434343",
                borderRadius: 7,
              }}
            />
            <Space.Compact direction="vertical">
              <span>View Watermark</span>
              <Text type="secondary" style={{ fontWeight: "normal" }}>
                View watermark details
              </Text>
            </Space.Compact>
          </Space>
        </div>
      }
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
      <div style={{ padding: 24, paddingBottom: 10 }}>
        <div style={{ marginBottom: 16 }}>
          <p className="font-bold" style={{ marginBottom: 8 }}>
            Text
          </p>
          <p style={{ marginBottom: 0 }}>{eventData?.text || "-"}</p>
        </div>
        <Divider style={{ margin: "16px 0" }} />
        <div style={{ marginBottom: 16 }}>
          <p className="font-bold" style={{ marginBottom: 8 }}>
            Created By
          </p>
          <p style={{ marginBottom: 0 }}>
            {eventData?.insertedByFullName || "-"}
          </p>
        </div>
        <div style={{ marginBottom: 16 }}>
          <p className="font-bold" style={{ marginBottom: 8 }}>
            Created Date
          </p>
          <p style={{ marginBottom: 0 }}>
            {eventData?.insertedAt
              ? dayjs.utc(eventData.insertedAt).format(dateConfig)
              : "-"}
          </p>
        </div>
        <div style={{ marginBottom: 16 }}>
          <p className="font-bold" style={{ marginBottom: 8 }}>
            Updated By
          </p>
          <p style={{ marginBottom: 0 }}>
            {eventData?.updatedByFullName || "-"}
          </p>
        </div>
        <div style={{ marginBottom: 16 }}>
          <p className="font-bold" style={{ marginBottom: 8 }}>
            Last Update Date
          </p>
          <p style={{ marginBottom: 0 }}>
            {eventData?.updatedAt
              ? dayjs.utc(eventData.updatedAt).format(dateConfig)
              : "-"}
          </p>
        </div>
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
            Close
          </Button>
        </Space>
      </div>
    </Modal>
  );
}
