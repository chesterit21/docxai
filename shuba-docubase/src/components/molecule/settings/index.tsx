import { useEffect, useState } from "react";
import {
  Space,
  Form,
  Input,
  Button,
  Tabs,
  InputNumber,
  Modal,
  Row,
  Col,
  message,
} from "antd";
import {
  CheckOutlined,
  CloseOutlined,
  ExclamationCircleOutlined,
  LockOutlined,
  SaveOutlined,
} from "@ant-design/icons";
import apiClient from "../../../services/apiClient";
import { useAuth } from "../../../context/AuthContext";
import { useNavigate } from "react-router-dom";

export default function Index() {
  const navigate = useNavigate();
  const { logout } = useAuth();
  const [form] = Form.useForm();
  const [dataSetting, setDataSetting] = useState<any>({});
  const [openConfirmAdd, setOpenConfirmAdd] = useState(false);
  const [confirmAddLoading, setConfirmAddLoading] = useState(false);

  useEffect(() => {
    apiClient
      .get("/setting")
      .then(({ data }) => {
        const filtered = Object.fromEntries(
          Object.entries(data?.data || {}).filter(
            ([key]) => !["appUploadFolder", "applicationUrl"].includes(key)
          )
        );
        setDataSetting(filtered);
      })
      .catch((err) => {
        console.log(err);
      });
  }, []);

  useEffect(() => {
    form.setFieldsValue(dataSetting);
  }, [dataSetting]);

  const formatLabel = (text: string): string => {
    return text.replace(/([a-z])([A-Z])/g, "$1 $2");
  };

  const handleSubmit = () => {
    setOpenConfirmAdd(true);
  };

  const handleClickOk = () => {
    console.log("submit", dataSetting);
    setConfirmAddLoading(true);
    apiClient
      .post("/setting", dataSetting)
      .then(({ data }) => {
        console.log(data?.data);
        setConfirmAddLoading(false);
        setOpenConfirmAdd(false);
        message.success("Change data is successful");
        setTimeout(() => {
          logout();
          navigate("/login");
        }, 1000);
      })
      .catch((err) => {
        console.log(err);
        setConfirmAddLoading(false);
        setOpenConfirmAdd(false);
        message.error(err.response.data.title);
      });
  };

  const handleChangeVal = (key1: any, key2: any, value: any) => {
    setDataSetting((prevState: any) => ({
      ...prevState,
      [key1]: { ...prevState[key1], [key2]: value },
    }));
  };

  return (
    <>
      <Form layout="vertical" onFinish={handleSubmit} form={form}>
        <div style={{ justifySelf: "end" }}>
          <Space>
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
        {/* <Divider style={{marginTop:0, marginBottom: 0}}/> */}
        <div style={{ padding: 24, paddingBottom: 10, minHeight: 250 }}>
          <Tabs
            tabPosition="left"
            items={Object.keys(dataSetting).map((groupKey, index) => {
              const groupData = dataSetting[groupKey];

              if (!groupData || typeof groupData !== "object") {
                return {
                  label: (
                    <span className="capitalize">{formatLabel(groupKey)}</span>
                  ),
                  key: `index-${index}`,
                  children: (
                    <div className="italic text-gray-500">
                      No data available
                    </div>
                  ),
                };
              }

              return {
                label: (
                  <span className="capitalize">{formatLabel(groupKey)}</span>
                ),
                key: `index-${index}`,
                children: (
                  <>
                    {Object.keys(groupData).map((fieldKey) => {
                      const value = groupData[fieldKey];
                      const isReadOnly = groupKey === "license" && fieldKey === "userCount";
                      const isNumberField = [
                        "rowPerPage",
                        "idleTimeoutAfterMinutes",
                        "attempts",
                        "port",
                        "commandTimeout",
                        "timeout",
                        "purgingPeriodInMonth",
                        "allowedReloginAfterMinutes",
                        "maxSize",
                      ].includes(fieldKey);
                      const labelTransform = formatLabel(fieldKey);
                      return (
                        <Form.Item
                          key={`${groupKey}-${fieldKey}`}
                          label={
                            <span className="capitalize">
                              {fieldKey === "maxSize"
                                ? "Max Size (MB)"
                                : labelTransform}
                            </span>
                          }
                          name={[groupKey, fieldKey]}
                          // rules={[{ required: true }]}
                        >
                          {isNumberField ? (
                            isReadOnly ? (
                              <Input
                                readOnly
                                value={value}
                                style={{ width: "100%" }}
                              />
                            ) : (
                              <InputNumber
                                onChange={(val) =>
                                  handleChangeVal(groupKey, fieldKey, val)
                                }
                                min={0}
                                style={{ width: "100%" }}
                                value={value}
                              />
                            )
                          ) : labelTransform
                              .toLocaleLowerCase()
                              .includes("password") ? (
                            <Input.Password
                              prefix={<LockOutlined />}
                              onChange={(e) =>
                                !isReadOnly &&
                                handleChangeVal(
                                  groupKey,
                                  fieldKey,
                                  e.target.value
                                )
                              }
                              placeholder="Enter value..."
                              value={value}
                              readOnly={isReadOnly}
                            />
                          ) : (
                            <Input
                              onChange={(e) =>
                                !isReadOnly &&
                                handleChangeVal(
                                  groupKey,
                                  fieldKey,
                                  e.target.value
                                )
                              }
                              placeholder="Enter value..."
                              value={value}
                              readOnly={isReadOnly}
                            />
                          )}
                        </Form.Item>
                      );
                    })}
                  </>
                ),
              };
            })}
          />
        </div>
      </Form>
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
                  onClick={handleClickOk}
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
          <p className="text-lg font-bold" style={{ marginBottom: 0 }}>
            Are you sure you want to save changes?
          </p>
          <span className="text-gray-500">
            Make sure this changes your needs and you logout automatically.
          </span>
        </div>
      </Modal>
    </>
  );
}
