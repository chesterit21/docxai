import { useEffect, useRef, useState } from "react";
import {
  Avatar,
  Button,
  Card,
  Col,
  ConfigProvider,
  Divider,
  Empty,
  Flex,
  Form,
  Input,
  List,
  message,
  Modal,
  Row,
  Select,
  Space,
  Table,
  Tag,
  Typography,
  type TableProps,
} from "antd";
import {
  CheckOutlined,
  CloseOutlined,
  ExclamationCircleOutlined,
  MailOutlined,
  UserOutlined,
} from "@ant-design/icons";
import { Trash2 } from "lucide-react";
import apiClient from "../../../services/apiClient";
const { Text } = Typography;
const { TextArea } = Input;
const { Option } = Select;

type TableRowSelection<T extends object = object> =
  TableProps<T>["rowSelection"];

export default function Index({ open, setOpen, document, fileList }: any) {
  const [userList, setUserList] = useState<any[]>([]);
  const [userAddList, setUserAddList] = useState<any[]>([]);
  const [userCCList, setUserCCList] = useState<any[]>([]);
  const [openConfirm, setOpenConfirm] = useState(false);
  const [confirmLoading, setConfirmLoading] = useState(false);
  const [optUserList, setOptUserList] = useState<any[]>([]);
  const [optUserCC, setOptUserCC] = useState<any[]>([]);
  const [selectedRowKeys, setSelectedRowKeys] = useState<React.Key[]>([]);
  const [form] = Form.useForm();
  const debounceRef = useRef<any>(null);

  const fileColumns = [
    // {
    //     dataIndex: 'id',
    //     key: 'id',
    //     title: 'File ID',
    // },
    {
      key: "documentFileName",
      dataIndex: "documentFileName",
      title: "File Name",
      render: (item: any) => {
        return <Tag style={{ color: "blue" }}>{item}</Tag>;
      },
    },
    {
      dataIndex: "owner",
      key: "owner",
      title: "Owner",
    },
    {
      dataIndex: "documentFileSize",
      key: "documentFileSize",
      title: "File Size",
    },
  ];

  useEffect(() => {
    if (open && userList.length == 0) {
      getDataUser();
      form.setFieldValue("subject", document.documentTitle);
      if (fileList.length > 0) {
        const defaultSelectedKeys = fileList.map((item: any) => item.id);
        setSelectedRowKeys(defaultSelectedKeys);
      }
    } else if (!open) {
      form.resetFields();
      setUserAddList([]);
      setUserCCList([]);
      setUserList([]);
    }
  }, [open]);

  useEffect(() => {
    if (userList.length > 0) {
      setOptUserList(
        userList.map((opt) => {
          return { value: opt.userId, label: opt.fullName };
        })
      );
      setOptUserCC(
        userList.map((opt) => {
          return { value: opt.userId, label: opt.fullName };
        })
      );
    } else {
      setOptUserList([]);
      setOptUserCC([]);
    }
  }, [userList]);

  const getDataUser = async () => {
    try {
      const { data: res } = await apiClient.get(`/dropdown/ddlusers`);
      setUserList(res.data);
    } catch (err) {
      console.error(err);
    }
  };

  const onSelectChange = (newSelectedRowKeys: React.Key[]) => {
    setSelectedRowKeys(newSelectedRowKeys);
  };

  const rowSelection: TableRowSelection = {
    selectedRowKeys,
    onChange: onSelectChange,
  };

  const onChange = (val: any) => {
    let userAdded = userList.find((i: any) => i.userId == val);
    if (userAdded) {
      setUserAddList((prev: any) => [...prev, userAdded]);
      setUserList((prev: any) => prev.filter((i: any) => i.userId != val));
    } else {
      const now = new Date();
      setUserAddList((prev: any) => [
        ...prev,
        { email: val, userId: now.getTime(), other: true },
      ]);
    }
    setOptUserList(
      userList.map((opt) => {
        return { value: opt.userId, label: opt.fullName };
      })
    );
  };

  const handleSearch = (value: string) => {
    if (debounceRef.current) clearTimeout(debounceRef.current);

    debounceRef.current = setTimeout(() => {
      const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
      const isExisting = optUserList.some((opt) => opt.value === value);

      if (emailRegex.test(value) && !isExisting) {
        setOptUserList((prev) => [
          ...prev,
          { label: `${value} (other email)`, value },
        ]);
      }
    }, 500);
  };

  const hanldeBlur = () => {
    setOptUserList(
      userList.map((opt) => {
        return { value: opt.userId, label: opt.fullName };
      })
    );
  };

  const handleDeleteUserAdded = (item: any) => {
    if (!item?.other) {
      setUserList((prev: any) => [...prev, item]);
      setUserAddList((prev: any) =>
        prev.filter((i: any) => i.userId !== item.userId)
      );
    } else {
      setUserAddList((prev: any) =>
        prev.filter((i: any) => i.userId !== item.userId)
      );
    }
  };

  const onChangeCC = (val: any) => {
    let userCC = userList.find((i: any) => i.userId == val);
    if (userCC) {
      setUserCCList((prev: any) => [...prev, userCC]);
      setUserList((prev: any) => prev.filter((i: any) => i.userId != val));
    } else {
      const now = new Date();
      setUserCCList((prev: any) => [
        ...prev,
        { email: val, userId: now.getTime(), other: true },
      ]);
    }

    setOptUserCC(
      userList.map((opt) => {
        return { value: opt.userId, label: opt.fullName };
      })
    );
  };

  const handleSearchCC = (value: string) => {
    if (debounceRef.current) clearTimeout(debounceRef.current);

    debounceRef.current = setTimeout(() => {
      const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
      const isExisting = optUserCC.some((opt) => opt.value === value);

      if (emailRegex.test(value) && !isExisting) {
        setOptUserCC((prev) => [
          ...prev,
          { label: `${value} (other email)`, value },
        ]);
      }
    }, 500);
  };

  const hanldeBlurCC = () => {
    setOptUserCC(
      userList.map((opt) => {
        return { value: opt.userId, label: opt.fullName };
      })
    );
  };

  const handleDeleteUserCC = (item: any) => {
    if (!item?.other) {
      setUserList((prev: any) => [...prev, item]);
      setUserCCList((prev: any) =>
        prev.filter((i: any) => i.userId !== item.userId)
      );
    } else {
      setUserCCList((prev: any) =>
        prev.filter((i: any) => i.userId !== item.userId)
      );
    }
  };

  const handleSubmit = () => {
    if (userAddList.length == 0) {
      message.error("Please select at least one user!");
      return;
    }
    if (selectedRowKeys.length == 0) {
      message.error("Please select at least one file!");
      return;
    }
    setOpenConfirm(true);
  };

  const handleConfrim = () => {
    const value = form.getFieldsValue();
    const user = userAddList.map((item) => item.email);
    const userCC =
      userCCList.length > 0 ? userCCList.map((item) => item.email) : [];
    const dataStore = {
      ...value,
      to: user,
      cc: userCC,
      documentID: Number(document.id),
      files: selectedRowKeys,
    };
    setConfirmLoading(true);
    apiClient
      .post("/documents/send-email", dataStore)
      .then(({ data }) => {
        console.log(data);
        form.resetFields();
        message.success("Send email is successful");
        setConfirmLoading(false);
        setOpenConfirm(false);
        setUserAddList([]);
        setUserCCList([]);
        setUserList([]);
        setOpen(false);
      })
      .catch((err) => {
        console.log(err);
        message.error(err.response.data.message);
        setConfirmLoading(false);
        setOpenConfirm(false);
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
              <MailOutlined
                style={{
                  border: "1px solid #d9d9d9",
                  padding: 7,
                  fontSize: 30,
                  color: "#434343",
                  borderRadius: 7,
                }}
              />
              <Space.Compact direction="vertical">
                <span>Email</span>
                <Text type="secondary" style={{ fontWeight: "normal" }}>
                  Fill in the user you want to give access to.
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
        zIndex={2}
        style={{ top: 20 }}
        styles={{
          content: {
            padding: 0,
          },
        }}
      >
        <Form layout="vertical" form={form} onFinish={handleSubmit}>
          <Divider style={{ marginBottom: 0 }} />
          <div className="grid grid-cols-1 gap-y-4 px-5 py-5">
            <div className="col-span-1">
              <div className="mb-3">Email</div>
              <Space.Compact style={{ width: "100%", marginBottom: 10 }}>
                <Select
                  showSearch
                  placeholder="Select a user"
                  style={{ width: "100%" }}
                  onSelect={onChange}
                  onSearch={handleSearch}
                  onBlur={hanldeBlur}
                  filterOption={(input, option) =>
                    (option?.label ?? "")
                      .toLowerCase()
                      .includes(input.toLowerCase())
                  }
                  options={optUserList}
                />
                <div
                  style={{
                    border: "1px solid #d9d9d9",
                    background: "#e7e7e7",
                    paddingLeft: 10,
                    paddingRight: 10,
                    fontSize: 20,
                    color: "#7c7c7c",
                    borderEndEndRadius: 5,
                    borderTopRightRadius: 5,
                  }}
                >
                  @
                </div>
              </Space.Compact>
              <ConfigProvider
                renderEmpty={() => (
                  <Empty
                    image={Empty.PRESENTED_IMAGE_SIMPLE}
                    description="No users added"
                    styles={{
                      root: {
                        marginBlock: 0,
                      },
                    }}
                  />
                )}
              >
                <List
                  itemLayout="horizontal"
                  dataSource={userAddList}
                  renderItem={(item) => (
                    <List.Item>
                      <div className="flex flex-row justify-between items-center w-full">
                        <div className="flex items-center gap-1">
                          <div className="flex gap-3 items-center">
                            <Avatar size="large" icon={<UserOutlined />} />
                            <Flex vertical>
                              <span className="font-bold">{item.fullName}</span>
                              <span style={{ textWrap: "nowrap" }}>
                                {item.email}
                              </span>
                            </Flex>
                          </div>
                        </div>
                        <Button
                          type="text"
                          icon={<Trash2 size={18} color="#595959" />}
                          iconPosition="start"
                          onClick={() => handleDeleteUserAdded(item)}
                        />
                      </div>
                    </List.Item>
                  )}
                />
              </ConfigProvider>
            </div>
            <div className="col-span-1">
              <div className="mb-3">CC</div>
              <Space.Compact style={{ width: "100%", marginBottom: 10 }}>
                <Select
                  showSearch
                  placeholder="Select a user"
                  style={{ width: "100%" }}
                  onSelect={onChangeCC}
                  onSearch={handleSearchCC}
                  onBlur={hanldeBlurCC}
                  filterOption={(input, option) =>
                    (option?.label ?? "")
                      .toLowerCase()
                      .includes(input.toLowerCase())
                  }
                  options={optUserCC}
                />
                <div
                  style={{
                    border: "1px solid #d9d9d9",
                    background: "#e7e7e7",
                    paddingLeft: 10,
                    paddingRight: 10,
                    fontSize: 20,
                    color: "#7c7c7c",
                    borderEndEndRadius: 5,
                    borderTopRightRadius: 5,
                  }}
                >
                  @
                </div>
              </Space.Compact>
              <ConfigProvider
                renderEmpty={() => (
                  <Empty
                    image={Empty.PRESENTED_IMAGE_SIMPLE}
                    description="No users added"
                    styles={{
                      root: {
                        marginBlock: 0,
                      },
                    }}
                  />
                )}
              >
                <List
                  itemLayout="horizontal"
                  dataSource={userCCList}
                  renderItem={(item) => (
                    <List.Item>
                      <div className="flex flex-row justify-between items-center w-full">
                        <div className="flex gap-3 items-center">
                          <Avatar size="large" icon={<UserOutlined />} />
                          <Flex vertical>
                            <span className="font-bold">{item.fullName}</span>
                            <span style={{ textWrap: "nowrap" }}>
                              {item.email}
                            </span>
                          </Flex>
                        </div>
                        <div className="flex justify-between items-center">
                          <Button
                            type="text"
                            icon={<Trash2 size={18} color="#595959" />}
                            iconPosition="start"
                            onClick={() => handleDeleteUserCC(item)}
                          />
                        </div>
                      </div>
                    </List.Item>
                  )}
                />
              </ConfigProvider>
            </div>
            <div className="col-span-1">
              <Form.Item
                name="subject"
                label="Subject"
                rules={[{ type: "string" }, { required: true }]}
              >
                <Input placeholder="enter subject" />
              </Form.Item>
            </div>
            <div className="col-span-1">
              <p className="mb-10">File</p>
              <Card styles={{ body: { padding: 0 } }}>
                <Table
                  rowKey="id"
                  scroll={{ x: 500 }}
                  rowSelection={rowSelection}
                  pagination={false}
                  columns={fileColumns}
                  dataSource={fileList}
                />
              </Card>
            </div>
            <div className="col-span-1">
              <Form.Item label="Watermark" name="watermark">
                <Select
                  style={{ width: "100%" }}
                  placeholder="select watermark"
                >
                  <Option value="text">Text</Option>
                  <Option value="logo">Logo</Option>
                </Select>
              </Form.Item>
            </div>
            <div className="col-span-1">
              <Form.Item
                label="Body"
                name="body"
                rules={[{ type: "string" }, { required: true }]}
              >
                <TextArea rows={4} placeholder="enter a body..." />
              </Form.Item>
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
                Cancel
              </Button>
              <Button
                type="primary"
                icon={<MailOutlined />}
                iconPosition="end"
                htmlType="submit"
              >
                Send Email
              </Button>
            </Space>
          </div>
        </Form>
      </Modal>
      {/* modal confirm share */}
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
                  Back
                </Button>
              </Col>
              <Col span={12}>
                <Button
                  type="primary"
                  icon={<CheckOutlined />}
                  iconPosition="end"
                  onClick={handleConfrim}
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
            Do you want to send Documents added someone to the list?
          </p>
          <span className="text-gray-500">
            The file you will be send to user on the list.
          </span>
        </div>
      </Modal>
    </>
  );
}
