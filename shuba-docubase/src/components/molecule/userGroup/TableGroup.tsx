import "./style.css";
import {
  Button,
  Card,
  Col,
  Divider,
  Flex,
  Form,
  Input,
  message,
  Modal,
  Row,
  Space,
  Table,
  Tooltip,
  Typography,
} from "antd";
import type { TableProps } from "antd";
import {
  ArrowLeftOutlined,
  ArrowRightOutlined,
  CheckOutlined,
  CloseOutlined,
  ExclamationCircleOutlined,
  PlusOutlined,
  SearchOutlined,
  UsergroupAddOutlined,
} from "@ant-design/icons";
import { useEffect, useState } from "react";
import { Pencil, ScanEye, Trash2 } from "lucide-react";
import type { PaginationProps } from "antd";
import { useNavigate } from "react-router-dom";
import apiClient from "../../../services/apiClient";
import { useAuth } from "../../../context/AuthContext";
import dayjs from "dayjs";
import customParseFormat from "dayjs/plugin/customParseFormat";
dayjs.extend(customParseFormat);
const { Text } = Typography;
const { TextArea } = Input;

const itemRender: PaginationProps["itemRender"] = (
  _,
  type,
  originalElement
) => {
  if (type === "prev") {
    return (
      <Button
        type="link"
        icon={<ArrowLeftOutlined />}
        iconPosition="start"
        style={{
          border: "1px solid #d9d9d9",
          fontWeight: "bold",
          alignItems: "baseline",
        }}
      >
        <span style={{ alignSelf: "center" }}>Prev</span>
      </Button>
    );
  }
  if (type === "next") {
    return (
      <Button
        type="link"
        icon={<ArrowRightOutlined />}
        iconPosition="end"
        style={{
          border: "1px solid #d9d9d9",
          fontWeight: "bold",
          alignItems: "baseline",
        }}
      >
        <span style={{ alignSelf: "center" }}>Next</span>
        {/* Next */}
      </Button>
    );
  }
  return originalElement;
};

const TableList = () => {
  const navigate = useNavigate();
  const { config } = useAuth();
  const pageLengthConfig = config?.dataGrid?.rowPerPage || 5;
  const dateConfig =
    config?.dataGrid?.dateTimeFormat || "YYYY-MM-DD | HH:mm:ss";
  const [data, setData] = useState<any[]>([]);
  const [loading, setLoading] = useState(false);
  const [page, setPage] = useState<number>(1);
  const pageLength = pageLengthConfig;
  const [totalRow, setTotalRow] = useState<number>(0);
  const [openAdd, setOpenAdd] = useState(false);
  const [openConfirmAdd, setOpenConfirmAdd] = useState(false);
  const [confirmAddLoading, setConfirmAddLoading] = useState(false);
  const [eventData, setEventData] = useState<any>({});
  const [openConfirmDelete, setOpenConfirmDelete] = useState(false);
  const [confirmDeleteLoading, setConfirmDeleteLoading] = useState(false);
  const [search, setSearch] = useState<string>("");
  const [form] = Form.useForm();
  const columns = [
    {
      key: "groupName",
      dataIndex: "groupName",
      title: "Group Name",
      render: (items: any) => <span className="font-bold">{items}</span>,
    },
    {
      key: "group_member",
      dataIndex: "group_member",
      title: "Group Member",
      render: (items: any) => <span>{items} User</span>,
    },
    {
      title: "Inserted By",
      dataIndex: "insertedByByFullName",
      key: "insertedByByFullName",
    },
    {
      title: "Inserted At",
      dataIndex: "insertedAt",
      key: "insertedAt",
      render: (items: any) => (
        <Flex vertical>
          <span>{items ? dayjs.utc(items).format(dateConfig) : ""}</span>
        </Flex>
      ),
    },
    {
      title: "Updated By",
      dataIndex: "updatedByFullName",
      key: "updatedByFullName",
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
      render: (row: any) => (
        <Space size="middle">
          <Tooltip placement="bottom" title="Detail" color="#595959">
            <Button
              type="text"
              icon={<ScanEye size={18} color="#595959" />}
              onClick={() => navigate(`/users-group/group-detail/${row.id}`)}
            />
          </Tooltip>
          <Tooltip
            placement="bottom"
            title={row.isSystem ? "Cannot edit system group" : "Edit"}
            color="#595959"
          >
            <Button
              type="text"
              icon={<Pencil size={18} color="#595959" />}
              onClick={() => navigate(`/users-group/group-edit/${row.id}`)}
            />
          </Tooltip>
          {!row.isSystem && (
            <Tooltip placement="bottom" title="Delete" color="#595959">
              <Button
                type="text"
                icon={<Trash2 size={18} color="#595959" />}
                onClick={() => handleDeleteDocument(row)}
              />
            </Tooltip>
          )}
        </Space>
      ),
    },
  ];

  useEffect(() => {
    getData();
  }, [page, pageLength]);

  const getData = () => {
    setLoading(true);
    apiClient
      .get(`/group/filter?search=${search}&Page=${page}&Limit=${pageLength}`)
      .then(({ data }) => {
        setData(data?.data?.data || []);
        setTotalRow(data.data.totalRecords);
        setLoading(false);
      })
      .catch((err) => {
        console.log(err);
        setLoading(false);
      });
  };

  const handleSubmit = () => {
    setOpenConfirmAdd(true);
  };

  const handleOkAdd = () => {
    const dataStore = form.getFieldsValue();
    setConfirmAddLoading(true);
    apiClient
      .post("/group", dataStore)
      .then(({ data }) => {
        console.log(data);
        setConfirmAddLoading(false);
        setOpenConfirmAdd(false);
        setOpenAdd(false);
        message.success("Add data is successful");
        getData();
      })
      .catch((err) => {
        console.log(err);
        message.error(err.response.data.message);
        setConfirmAddLoading(false);
        setOpenConfirmAdd(false);
      });
  };

  const handleDeleteDocument = (item: any) => {
    setEventData(item);
    setOpenConfirmDelete(true);
  };

  const handleOkDelete = () => {
    if (Object.keys(eventData).length == 0)
      return message.error("Group ID not found!");
    setConfirmDeleteLoading(true);
    apiClient
      .put(`/group/delete?groupId=${eventData.id}`)
      .then(({ data }) => {
        console.log(data);
        setEventData({});
        setConfirmDeleteLoading(false);
        setOpenConfirmDelete(false);
        message.success("Delete data is successful");
        getData();
      })
      .catch((err) => {
        console.log(err);
        setConfirmDeleteLoading(false);
        setOpenConfirmDelete(false);
        message.error(err?.response?.data?.message || "Failed to delete");
      });
  };

  const handleTableChange: TableProps<any>["onChange"] = (pagination) => {
    setPage(Number(pagination.current));
  };

  const handleKeyDownDocument = (event: any) => {
    if (event.key === "Enter" || event.code === "Enter") {
      const str = search.length > 0 ? search : "";
      setSearch(str);
      getData();
    }
  };

  const handleAfterOpenChange = (visible: boolean) => {
    if (!visible) {
      form.resetFields();
    }
  };

  return (
    <>
      <div className="grid grid-cols-2 gap-4 mb-5 lg:grid-cols-12">
        <div className="col-span-2 lg:col-span-3">
          <Input
            size="middle"
            onChange={(e: any) => setSearch(e.target.value)}
            onKeyDown={handleKeyDownDocument}
            placeholder="Search ..."
            prefix={<SearchOutlined />}
          />
        </div>
        <div className="flex justify-start col-span-2 gap-4 lg:col-span-4 lg:col-end-13 lg:justify-end">
          <Button
            type="primary"
            icon={<UsergroupAddOutlined />}
            iconPosition="end"
            onClick={() => setOpenAdd(true)}
          >
            Create Group
          </Button>
        </div>
      </div>
      <Row>
        <Col span={24}>
          <Card
            styles={{
              body: {
                padding: 0,
              },
            }}
          >
            <Table
              rowKey="userId"
              onChange={handleTableChange}
              scroll={{ x: 1000 }}
              pagination={{
                position: ["bottomCenter"],
                itemRender: itemRender,
                pageSize: pageLength,
                total: totalRow,
              }}
              columns={columns}
              dataSource={data}
              loading={loading}
            />
          </Card>
        </Col>
      </Row>
      {/* modal create */}
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
                <span>Create Group</span>
                <Text type="secondary" style={{ fontWeight: "normal" }}>
                  Create a group by filling in the group name and make sure the
                  user list in correct.
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
        <Form layout="vertical" form={form} onFinish={handleSubmit}>
          <div style={{ padding: 24, paddingBottom: 10 }}>
            <Form.Item
              label="Group Name"
              name="groupName"
              rules={[{ type: "string" }, { required: true }]}
            >
              <Input placeholder="enter group name..." />
            </Form.Item>
            <Form.Item
              label="Description"
              name="groupDescription"
              rules={[{ type: "string" }]}
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
                htmlType="submit"
              >
                Create Group
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
        open={openConfirmAdd}
        onCancel={() => setOpenConfirmAdd(false)}
        maskClosable={false}
        zIndex={5}
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
                  onClick={handleOkAdd}
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
            Do you want to add a new Group?
          </p>
          <span className="text-gray-500">
            The users you enter into the group list and group name will
            automatically be created into a group
          </span>
        </div>
      </Modal>
      {/* modal confirm delete */}
      <Modal
        title={
          <div style={{ paddingLeft: 24, paddingRight: 24, paddingTop: 24 }}>
            <ExclamationCircleOutlined
              style={{ fontSize: 30, color: "#f5222d" }}
            />
          </div>
        }
        open={openConfirmDelete}
        onCancel={() => setOpenConfirmDelete(false)}
        maskClosable={false}
        footer={(_) => (
          <>
            <Row gutter={12}>
              <Col span={12}>
                <Button
                  icon={<CloseOutlined />}
                  iconPosition="end"
                  onClick={() => setOpenConfirmDelete(false)}
                  variant="outlined"
                  color="danger"
                  block
                >
                  Cancel
                </Button>
              </Col>
              <Col span={12}>
                <Button
                  variant="solid"
                  color="danger"
                  icon={<Trash2 size={18} />}
                  iconPosition="end"
                  onClick={handleOkDelete}
                  loading={confirmDeleteLoading}
                  block
                >
                  Delete
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
            Are you sure you want to delete this group?
          </p>
          <span className="text-gray-500">
            Deleting this group will delete all items within it. Make sure this
            action before continuing..
          </span>
        </div>
      </Modal>
    </>
  );
};

export default TableList;
