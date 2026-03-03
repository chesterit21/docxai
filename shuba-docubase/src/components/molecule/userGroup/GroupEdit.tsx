import { useEffect, useState } from "react";
import Breadcrumb from "../../atom/Breadcrumb";
import {
  Avatar,
  Button,
  Card,
  Col,
  Divider,
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
  Tooltip,
  Typography,
} from "antd";
import {
  CheckOutlined,
  CloseOutlined,
  ExclamationCircleOutlined,
  PlusOutlined,
  SaveOutlined,
  UsergroupAddOutlined,
  UserOutlined,
} from "@ant-design/icons";
import { useNavigate, useParams } from "react-router-dom";
import { Trash2, UserPlus } from "lucide-react";
import apiClient from "../../../services/apiClient";
import dayjs from "dayjs";
import customParseFormat from "dayjs/plugin/customParseFormat";
import { useAuth } from "../../../context/AuthContext";

dayjs.extend(customParseFormat);

const { Text } = Typography;
const { TextArea } = Input;

export default function Index() {
  const navigate = useNavigate();
  const { config } = useAuth();
  const dateConfig =
    config?.dataGrid?.dateTimeFormat || "YYYY-MM-DD | HH:mm:ss";
  const { uuid: documentId } = useParams();
  const [dataDetail, setDataDetail] = useState<any>({});
  const [userList, setUserList] = useState<any[]>([]);
  const [optUserList, setOptUserList] = useState<any[]>([]);
  const [userAddList, setUserAddList] = useState<any[]>([]);
  const [openConfirmEdit, setOpenConfirmEdit] = useState(false);
  const [confirmEditLoading, setConfirmEditLoading] = useState(false);
  const [groupMember, setGroupMember] = useState<any[]>([]);
  const [memberSearch, setMemberSearch] = useState<string>("");
  const [deletedRows, setDeletedRows] = useState<string[]>([]);

  const [openAdd, setOpenAdd] = useState(false);
  const [form] = Form.useForm();
  const columns = [
    {
      dataIndex: "fullName",
      key: "fullName",
      title: "User Name",
      render: (items: any, row: any) => (
        <Flex gap="small" align="center">
          <Avatar size="large" icon={<UserOutlined />} />
          <Flex vertical>
            <span className="font-bold">{items}</span>
            <span style={{ textWrap: "nowrap" }}>{row.emailAddress}</span>
          </Flex>
        </Flex>
      ),
    },
    {
      dataIndex: "insertedByFullName",
      key: "insertedByFullName",
      title: "Inserted By",
    },
    {
      dataIndex: "insertedAt",
      key: "insertedAt",
      title: "Inserted At",
      render: (items: any) => (
        <Flex vertical>
          <span>{items ? dayjs.utc(items).format(dateConfig) : ""}</span>
        </Flex>
      ),
    },
    {
      dataIndex: "updatedByFullName",
      key: "updatedByFullName",
      title: "Updated By",
    },
    {
      title: "Updated At",
      dataIndex: "updatedAt",
      key: "updatedAt",
      render: (items: any) => (
        <Flex vertical>
          <span>{items ? dayjs.utc(items).format(dateConfig) : ""}</span>
        </Flex>
      ),
    },
    {
      title: "Action",
      key: "action",
      render: (_: any, record: any) => (
        <Space size="middle">
          <Tooltip placement="bottom" title="Delete" color="#595959">
            <Button
              type="text"
              onClick={() => handleDelete(record)}
              icon={<Trash2 size={18} color="#595959" />}
            />
          </Tooltip>
        </Space>
      ),
    },
  ];

  useEffect(() => {
    getDataDetail();
    getDataMember();
  }, [documentId]);

  useEffect(() => {
    if (Object.keys(dataDetail).length > 0) {
      form.setFieldsValue(dataDetail);
    }
  }, [dataDetail, form]);

  useEffect(() => {
    if (openAdd) {
      getDataUser();
    }
  }, [openAdd]);

  useEffect(() => {
    if (userList.length > 0) {
      const filterData = userList
        .filter(
          (item: any) => !groupMember.some((b: any) => b.userId === item.userId)
        )
        .map((opt) => {
          return { value: opt.userId, label: opt.fullName };
        });
      setOptUserList(filterData);
    } else {
      setOptUserList([]);
    }
  }, [userList, groupMember]);

  const getDataDetail = () => {
    apiClient
      .get(`/group/get-byid?id=${documentId}`)
      .then(({ data }) => {
        setDataDetail(data?.data || {});
      })
      .catch((err) => {
        console.log(err);
      });
  };

  const getDataMember = () => {
    apiClient
      .get(`/group/get-group-members?id=${documentId}`)
      .then(({ data }) => {
        setGroupMember(data?.data?.[0]?.userGroup || []);
      })
      .catch((err) => {
        console.log(err);
      });
  };

  const getDataUser = async () => {
    try {
      const { data: res } = await apiClient.get(`/dropdown/ddlusers`);
      setUserList(res.data);
    } catch (err) {
      console.error(err);
    }
  };

  const handleSubmit = () => {
    setOpenConfirmEdit(true);
  };

  const handleOkEdit = () => {
    const value = form.getFieldsValue();
    const userAdd = groupMember
      .filter((a: any) => !deletedRows.some((b: any) => a.userId === b))
      .map((item: any) => {
        return { userId: item.userId };
      });
    const dataUpdate = { ...value, users: userAdd };
    setConfirmEditLoading(true);
    apiClient
      .put("/group", dataUpdate)
      .then(({ data }) => {
        console.log(data);
        setConfirmEditLoading(false);
        setOpenConfirmEdit(false);
        setOpenAdd(false);
        message.success("Edit data is successful");
        navigate("/users-group?tab=group");
      })
      .catch((err) => {
        console.log(err);
        message.error(err.response.data.message);
        setConfirmEditLoading(false);
        setOpenConfirmEdit(false);
      });
  };

  const onChange = (val: any) => {
    let userAdded = userList
      .filter((i: any) => i.userId == val)
      .reduce((_, item) => {
        return item;
      }, {});
    const userAddToGroup = { ...userAdded, emailAddress: userAdded.email };
    setUserAddList((prev: any) => [...prev, userAddToGroup]);
    setUserList((prev: any) => prev.filter((i: any) => i.userId != val));
  };

  const handleAfterOpenChange = (visible: boolean) => {
    if (!visible) {
      setUserAddList([]);
    }
  };

  const handleClickAddUser = () => {
    if (userAddList.length == 0)
      return message.error("Please select at least one user!");
    setGroupMember((prev: any) => [...prev, ...userAddList]);
    setOpenAdd(false);
  };

  const handleDelete = (record: any) => {
    setDeletedRows((prev) => {
      const isAlreadyDeleted = prev.includes(record.userId);
      if (isAlreadyDeleted) {
        return prev.filter((key) => key !== record.userId);
      } else {
        return [...prev, record.userId];
      }
    });
  };

  const filteredGroupMembers = groupMember.filter((member) => {
    if (!memberSearch) return true;
    return member?.fullName?.toLowerCase().includes(memberSearch.toLowerCase());
  });

  return (
    <>
      <Breadcrumb
        item={[
          { title: <span style={{ fontWeight: "bold" }}>Group Edit</span> },
        ]}
      />
      <Form layout="vertical" form={form} onFinish={handleSubmit}>
        <Row gutter={[16, 16]} style={{ marginTop: 30 }}>
          <Col span={24}>
            <div style={{ padding: 24, paddingBottom: 10 }}>
              <Form.Item name="groupId" hidden>
                <Input />
              </Form.Item>
              <Form.Item
                label="Group Name"
                name="groupName"
                rules={[{ type: "string" }, { required: true }]}
              >
                <Input
                  placeholder="enter group name..."
                  disabled={dataDetail.isSystem}
                />
              </Form.Item>
              <Form.Item
                label="Description"
                name="groupDescription"
                rules={[{ type: "string" }]}
              >
                <TextArea rows={4} placeholder="enter description..." />
              </Form.Item>
            </div>
          </Col>
          <Col span={24}>
            <div className="grid grid-cols-2 md:grid-cols-12 gap-4 mb-5 items-center">
              <div className="col-span-2 md:col-span-3">
                <span className="font-bold">Member List</span>
              </div>
              <div className="col-span-2 md:col-span-2 md:col-end-13 flex justify-start md:justify-end gap-4">
                <Button
                  color="purple"
                  variant="solid"
                  icon={<PlusOutlined />}
                  iconPosition="end"
                  onClick={() => setOpenAdd(true)}
                >
                  Add User
                </Button>
              </div>
            </div>
            <Card
              styles={{
                body: {
                  padding: 0,
                },
              }}
            >
              <Input
                placeholder="Search member"
                style={{ marginBottom: 12 }}
                value={memberSearch}
                onChange={(e) => setMemberSearch(e.target.value)}
              />
              <Table
                rowKey="userId"
                scroll={{ x: 1000 }}
                pagination={false}
                columns={columns}
                dataSource={filteredGroupMembers}
                rowClassName={(record) =>
                  deletedRows.includes(record.userId) ? "deleted-row" : ""
                }
              />
            </Card>
          </Col>
          <Col span={24} style={{ display: "flex", justifyContent: "end" }}>
            <Space size="large">
              <Button
                icon={<CloseOutlined />}
                iconPosition="end"
                onClick={() => navigate("/users-group?tab=group")}
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
                Save Changed
              </Button>
            </Space>
          </Col>
        </Row>
      </Form>
      {/* modal add user */}
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
                <span>Add User</span>
                <Text type="secondary" style={{ fontWeight: "normal" }}>
                  The users you add will automatically be added to the group.
                </Text>
              </Space.Compact>
            </Space>
          </div>
        }
        destroyOnHidden
        afterOpenChange={handleAfterOpenChange}
        open={openAdd}
        onCancel={() => setOpenAdd(false)}
        maskClosable={false}
        footer={false}
        width={600}
        zIndex={2}
        styles={{
          content: {
            padding: 0,
          },
        }}
      >
        <Divider style={{ marginBottom: 0 }} />
        <div className="px-7 py-5">
          <Row gutter={12}>
            <Col span={24}>
              <Space.Compact style={{ width: "100%", marginBottom: 10 }}>
                <Select
                  showSearch
                  placeholder="Select a user"
                  style={{ width: "100%" }}
                  onSelect={onChange}
                  filterOption={(input, option) =>
                    (option?.label ?? "")
                      .toLowerCase()
                      .includes(input.toLowerCase())
                  }
                  options={optUserList}
                />
                <UsergroupAddOutlined
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
                />
              </Space.Compact>
            </Col>
            <Col span={24}>
              <div className="min-h-[200px] max-h-[410px] overflow-y-auto">
                <List
                  itemLayout="horizontal"
                  dataSource={userAddList}
                  renderItem={(item) => (
                    <List.Item>
                      <Flex gap="small" align="center">
                        <UserPlus
                          size={30}
                          color="#434343"
                          style={{
                            border: "1px solid #d9d9d9",
                            padding: 5,
                            borderRadius: 7,
                          }}
                        />
                        <Avatar size="large" icon={<UserOutlined />} />
                        <Flex vertical>
                          <span className="font-bold">{item.fullName}</span>
                          <span style={{ textWrap: "nowrap" }}>
                            {item.email}
                          </span>
                        </Flex>
                      </Flex>
                    </List.Item>
                  )}
                />
              </div>
            </Col>
          </Row>
        </div>
        <Divider style={{ marginTop: 0, marginBottom: 0 }} />
        <div style={{ padding: 20, paddingBottom: 20, justifySelf: "end" }}>
          <Space>
            <Button
              icon={<CloseOutlined />}
              iconPosition="end"
              onClick={() => setOpenAdd(false)}
              color="danger"
              variant="filled"
            >
              Cancel
            </Button>
            <Button
              type="primary"
              icon={<PlusOutlined />}
              iconPosition="end"
              onClick={handleClickAddUser}
            >
              Add
            </Button>
          </Space>
        </div>
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
        open={openConfirmEdit}
        onCancel={() => setOpenConfirmEdit(false)}
        maskClosable={false}
        zIndex={5}
        footer={(_) => (
          <>
            <Row gutter={12}>
              <Col span={12}>
                <Button
                  icon={<CloseOutlined />}
                  iconPosition="end"
                  onClick={() => setOpenConfirmEdit(false)}
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
                  onClick={handleOkEdit}
                  block
                  loading={confirmEditLoading}
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
            Do you want to update group data?
          </p>
          <span className="text-gray-500">
            The data you update now will automatically change in the group list.
          </span>
        </div>
      </Modal>
    </>
  );
}
