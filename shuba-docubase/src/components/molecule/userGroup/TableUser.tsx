import "./style.css";
import {
  Avatar,
  Button,
  Card,
  Col,
  Divider,
  Flex,
  Input,
  Modal,
  Row,
  Space,
  Table,
  Tooltip,
  Typography,
  Form,
  Select,
  Radio,
  Segmented,
  message,
} from "antd";
import type { TableProps } from "antd";
import {
  ArrowLeftOutlined,
  ArrowRightOutlined,
  CheckOutlined,
  CloseOutlined,
  DownloadOutlined,
  ExclamationCircleOutlined,
  PlusOutlined,
  SearchOutlined,
  UserOutlined,
} from "@ant-design/icons";
import { useEffect, useState } from "react";
import { Pencil, ScanEye, Trash2 } from "lucide-react";
import type { PaginationProps } from "antd";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../../../context/AuthContext";
import apiClient from "../../../services/apiClient";
import * as XLSX from "xlsx";
import { saveAs } from "file-saver";
import dayjs from "dayjs";
import customParseFormat from "dayjs/plugin/customParseFormat";

dayjs.extend(customParseFormat);
const { Text } = Typography;

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

type TableRowSelection<T extends object = object> =
  TableProps<T>["rowSelection"];

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
  const [eventData, setEventData] = useState<any>({});
  const [selectedRowKeys, setSelectedRowKeys] = useState<React.Key[]>([]);
  const [btnAddList, setBtnAddList] = useState<boolean>(true);
  const [openAdd, setOpenAdd] = useState(false);
  const [openEdit, setOpenEdit] = useState(false);
  const [openConfirmAdd, setOpenConfirmAdd] = useState(false);
  const [openConfirmEdit, setOpenConfirmEdit] = useState(false);
  const [confirmAddLoading, setConfirmAddLoading] = useState(false);
  const [confirmEditLoading, setConfirmEditLoading] = useState(false);
  const [openConfirmDelete, setOpenConfirmDelete] = useState(false);
  const [confirmDeleteLoading, setConfirmDeleteLoading] = useState(false);

  const [optList, setOptList] = useState<any[]>([]);
  const [optListCompany, setOptListCompany] = useState<any[]>([]);
  const [typeUser, setTypeUser] = useState("Email Address");
  const [search, setSearch] = useState<string>("");
  const [formCreate] = Form.useForm();
  const [formEdit] = Form.useForm();

  const columns = [
    {
      key: "fullName",
      dataIndex: "fullName",
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
      title: "Username",
      dataIndex: "userName",
      key: "userName",
    },
    {
      title: "Phone Number",
      dataIndex: "phoneNumber",
      key: "phoneNumber",
    },
    {
      key: "groups",
      dataIndex: "groups",
      title: "Group",
      render: (items: any) => {
        return (
          (items &&
            items
              .map((item: any) => {
                return item.groupName;
              })
              .join(" | ")) ||
          "-"
        );
      },
    },
    {
      title: "User Type",
      dataIndex: "userType",
      key: "userType",
    },
    {
      title: "Status",
      dataIndex: "userStatus",
      key: "userStatus",
    },
    {
      title: "Last login",
      dataIndex: "lastLogin",
      key: "lastLogin",
      render: (items: any) => (
        <Flex vertical>
          <span>{items ? dayjs.utc(items).format(dateConfig) : ""}</span>
        </Flex>
      ),
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
      render: (_: any, row: any) => (
        <Space size="middle">
          <Tooltip placement="bottom" title="Detail" color="#595959">
            <Button
              type="text"
              icon={<ScanEye size={18} color="#595959" />}
              onClick={() => navigate(`/users-group/user-detail/${row.userId}`)}
            />
          </Tooltip>
          <Tooltip placement="bottom" title="Edit" color="#595959">
            <Button
              type="text"
              icon={<Pencil size={18} color="#595959" />}
              onClick={() => handleClickUpdate(row)}
            />
          </Tooltip>
          <Tooltip placement="bottom" title="Delete" color="#595959">
            <Button
              type="text"
              icon={<Trash2 size={18} color="#595959" />}
              onClick={() => handleDeleteDocument(row)}
            />
          </Tooltip>
        </Space>
      ),
    },
  ];

  useEffect(() => {
    getData();
  }, [page, pageLength]);

  useEffect(() => {
    if (openAdd || openEdit) {
      getListGroup();
      getListCompany();
    }
  }, [openAdd, openEdit]);

  useEffect(() => {
    if (Object.keys(eventData).length > 0) {
      formEdit.setFieldsValue(eventData);
    }
  }, [eventData, formEdit]);

  useEffect(() => {
    if (selectedRowKeys.length > 0) {
      setBtnAddList(false);
    } else {
      setBtnAddList(true);
    }
  }, [selectedRowKeys]);

  const getData = () => {
    setLoading(true);
    apiClient
      .get(`/user/filter?Username=${search}&Page=${page}&Limit=${pageLength}`)
      .then(({ data }) => {
        console.log(data.data);
        setData(data?.data?.data || []);
        setTotalRow(data.data.totalRecords);
        setLoading(false);
      })
      .catch((err) => {
        console.log(err);
        setLoading(false);
      });
  };

  const getListGroup = () => {
    apiClient
      .get(`/dropdown/ddlgroups`)
      .then(({ data }) => {
        setOptList(
          data?.data?.map((opt: any) => {
            return { value: opt.value, label: opt.text };
          }) || []
        );
      })
      .catch((err) => {
        console.log(err);
      });
  };

  const getListCompany = () => {
    apiClient
      .get(`/dropdown/ddlcompany`)
      .then(({ data }) => {
        setOptListCompany(
          data?.data?.map((opt: any) => {
            return { value: opt.value, label: opt.text };
          }) || []
        );
      })
      .catch((err) => {
        console.log(err);
      });
  };

  const onSelectChange = (newSelectedRowKeys: React.Key[]) => {
    console.log("selectedRowKeys changed: ", newSelectedRowKeys);
    setSelectedRowKeys(newSelectedRowKeys);
  };

  const rowSelection: TableRowSelection<any> = {
    selectedRowKeys,
    onChange: onSelectChange,
  };

  const handleSubmit = () => {
    setOpenConfirmAdd(true);
  };

  const handleSubmitEdit = () => {
    setOpenConfirmEdit(true);
  };

  const handleOkAdd = () => {
    const value = formCreate.getFieldsValue();
    const dataStore = {
      ...value,
      isADUser: value.user_type == "Active Directory" ? true : false,
      phoneNumber: value.phoneNumber
        ? value.phoneNumber.toString().startsWith("0")
          ? "62" + value.phoneNumber.toString().slice(1)
          : value.phoneNumber.toString()
        : "",
    };
    setConfirmAddLoading(true);
    apiClient
      .post("/user", dataStore)
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

  const handleClickUpdate = (value: any) => {
    const changeOpt = value.groups.map((items: any) => items.groupId);
    setEventData({ ...value, groups: changeOpt });
    setOpenEdit(true);
  };

  const handleOkEdit = () => {
    const value = formEdit.getFieldsValue();
    const dataUpdate = {
      ...value,
      phoneNumber: value.phoneNumber
        ? value.phoneNumber.toString().startsWith("0")
          ? "62" + value.phoneNumber.toString().slice(1)
          : value.phoneNumber.toString()
        : "",
    };
    setConfirmEditLoading(true);
    apiClient
      .put("/user", dataUpdate)
      .then(({ data }) => {
        console.log(data);
        setConfirmEditLoading(false);
        setOpenConfirmEdit(false);
        setOpenEdit(false);
        message.success("Edit data is successful");
        getData();
      })
      .catch((err) => {
        console.log(err);
        message.error(err.response.data.message);
        setConfirmEditLoading(false);
        setOpenConfirmEdit(false);
      });
  };

  const handleDeleteDocument = (item: any) => {
    setEventData(item);
    setOpenConfirmDelete(true);
  };

  const handleOkDelete = () => {
    if (Object.keys(eventData).length == 0)
      return message.error("User ID not found!");
    setConfirmDeleteLoading(true);
    apiClient
      .put(`/user/delete?userId=${eventData.userId}`)
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
        message.error(err.response.data.message);
      });
  };

  const handleTableChange: TableProps<any>["onChange"] = (pagination) => {
    setPage(Number(pagination.current));
  };

  const handleAfterOpenChange = (visible: boolean) => {
    if (!visible) {
      formCreate.resetFields();
    }
  };

  const handleAfterOpenChangeEdit = (visible: boolean) => {
    if (!visible) {
      formEdit.resetFields();
      setEventData({});
    }
  };

  const handleKeyDownDocument = (event: any) => {
    if (event.key === "Enter" || event.code === "Enter") {
      const str = search.length > 0 ? search : "";
      setSearch(str);
      getData();
    }
  };

  const handleClickDownload = async () => {
    const dd =
      data?.filter((a: any) =>
        selectedRowKeys.some((id: any) => id === a.userId)
      ) || [];
    // Create a new workbook and worksheet
    const worksheet = XLSX.utils.json_to_sheet(dd);
    const workbook = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(workbook, worksheet, "Sheet1");

    // Create a binary Excel file
    const excelBuffer = XLSX.write(workbook, {
      bookType: "xlsx",
      type: "array",
    });

    // Use FileSaver to save the file
    const blob = new Blob([excelBuffer], { type: "application/octet-stream" });
    saveAs(blob, `user-list.xlsx`);
  };

  return (
    <>
      <div className="grid grid-cols-2 lg:grid-cols-12 gap-4 mb-5">
        <div className="col-span-2 lg:col-span-3">
          <Input
            size="middle"
            onChange={(e: any) => setSearch(e.target.value)}
            onKeyDown={handleKeyDownDocument}
            placeholder="Search ..."
            prefix={<SearchOutlined />}
          />
        </div>
        <div className="col-span-2 lg:col-span-4 lg:col-end-13 flex justify-start lg:justify-end gap-4">
          <Button
            type="primary"
            icon={<PlusOutlined />}
            iconPosition="end"
            onClick={() => setOpenAdd(true)}
          >
            Create User
          </Button>
          <Button
            icon={<DownloadOutlined />}
            iconPosition="end"
            onClick={handleClickDownload}
            disabled={btnAddList}
          >
            Download
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
              rowSelection={rowSelection}
              scroll={{ x: 1100 }}
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
                <span>Create User</span>
                <Text type="secondary" style={{ fontWeight: "normal" }}>
                  Add new User by filling in the form bellow.
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
        zIndex={10}
        width={600}
        styles={{
          content: {
            padding: 0,
          },
        }}
      >
        <Divider style={{ marginBottom: 0 }} />
        <Form
          form={formCreate}
          layout="vertical"
          onFinish={handleSubmit}
          initialValues={{ userType: "user" }}
        >
          <div style={{ padding: 24, paddingBottom: 10 }}>
            <Form.Item
              label="Full Name"
              name="fullName"
              rules={[{ type: "string" }, { required: true }]}
            >
              <Input placeholder="enter full name..." />
            </Form.Item>
            <Form.Item
              label="Username"
              name="userName"
              rules={[{ type: "string" }, { required: true }]}
            >
              <Input placeholder="enter username..." />
            </Form.Item>
            <Form.Item name="user_type">
              <Segmented
                defaultValue="Email Address"
                block
                options={["Active Directory", "Email Address"]}
                onChange={(value) => setTypeUser(value)}
              />
            </Form.Item>
            <Form.Item
              label={typeUser == "Email Address" ? "Email" : "User AD"}
              name="emailAddress"
              rules={[
                { type: typeUser == "Email Address" ? "email" : "string" },
                { required: true },
              ]}
            >
              <Input
                placeholder={`enter ${
                  typeUser == "Email Address" ? "email" : "user AD"
                } ...`}
              />
            </Form.Item>
            <Form.Item
              label="Phone Number"
              name="phoneNumber"
              rules={[{ type: "string" }, { required: true }]}
            >
              <Input type="number" placeholder="enter phone number..." />
            </Form.Item>
            <Form.Item
              label="Role"
              name="userType"
              rules={[{ required: true }]}
            >
              <Select
                placeholder="Select Role"
                options={[
                  { value: "user", label: "User" },
                  { value: "admin", label: "Admin" },
                  { value: "superadmin", label: "Super Admin" },
                ]}
              />
            </Form.Item>
            <Form.Item
              label="Company"
              name="companyId"
              rules={[{ required: true }]}
            >
              <Select placeholder="Select Company" options={optListCompany} />
            </Form.Item>
            <Form.Item
              label="Group"
              name="groups"
              rules={[{ type: "array" }, { required: true }]}
            >
              <Select
                placeholder="Group"
                mode="multiple"
                options={optList}
              ></Select>
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
                Create User
              </Button>
            </Space>
          </div>
        </Form>
      </Modal>
      {/* modal edit */}
      <Modal
        title={
          <div
            className="space-align-block"
            style={{ paddingLeft: 24, paddingRight: 24, paddingTop: 24 }}
          >
            <Space align="center" size="middle">
              <Pencil
                size={44}
                color="#434343"
                className="mock-block"
                style={{
                  border: "1px solid #d9d9d9",
                  padding: 7,
                  borderRadius: 7,
                }}
              />
              <Space.Compact direction="vertical">
                <span>Edit User</span>
                <Text type="secondary" style={{ fontWeight: "normal" }}>
                  Edit User by filling in the form bellow.
                </Text>
              </Space.Compact>
            </Space>
          </div>
        }
        destroyOnHidden
        afterOpenChange={handleAfterOpenChangeEdit}
        open={openEdit}
        onCancel={() => setOpenEdit(false)}
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
        <Form
          layout="vertical"
          onFinish={handleSubmitEdit}
          form={formEdit}
          initialValues={{ isActive: true }}
        >
          <div style={{ padding: 24, paddingBottom: 10 }}>
            <Form.Item name="userId" hidden>
              <Input />
            </Form.Item>
            <Form.Item
              label="Full Name"
              name="fullName"
              rules={[{ type: "string" }, { required: true }]}
            >
              <Input placeholder="enter full name..." />
            </Form.Item>
            <Form.Item
              label="Username"
              name="userName"
              rules={[{ type: "string" }, { required: true }]}
            >
              <Input placeholder="enter username..." />
            </Form.Item>
            <Form.Item
              label="Email"
              name="emailAddress"
              rules={[{ type: "string" }, { required: true }]}
            >
              <Input placeholder="enter email..." />
            </Form.Item>
            <Form.Item
              label="Phone Number"
              name="phoneNumber"
              rules={[{ type: "string" }, { required: true }]}
            >
              <Input type="number" placeholder="enter phone number..." />
            </Form.Item>
            <Form.Item
              label="User Status"
              name="isActive"
              rules={[{ required: true }]}
            >
              <Radio.Group>
                <Radio value={true}> Active </Radio>
                <Radio value={false}> Inactive </Radio>
              </Radio.Group>
            </Form.Item>
            <Form.Item
              label="Role"
              name="userType"
              rules={[{ required: true }]}
            >
              <Select
                defaultValue="user"
                placeholder="Select Role"
                options={[
                  { value: "user", label: "User" },
                  { value: "admin", label: "Admin" },
                  { value: "superadmin", label: "Super Admin" },
                ]}
              />
            </Form.Item>
            <Form.Item
              label="Company"
              name="companyId"
              rules={[{ required: true }]}
            >
              <Select placeholder="Select Company" options={optListCompany} />
            </Form.Item>
            <Form.Item
              label="Group"
              name="groups"
              rules={[{ type: "array" }, { required: true }]}
            >
              <Select placeholder="Group" mode="multiple" options={optList} />
            </Form.Item>
          </div>
          <Divider style={{ marginTop: 0, marginBottom: 0 }} />
          <div style={{ padding: 20, paddingBottom: 20, justifySelf: "end" }}>
            <Space>
              <Button
                icon={<CloseOutlined />}
                iconPosition="end"
                onClick={() => setOpenEdit(false)}
                color="danger"
                variant="filled"
              >
                Cancel
              </Button>
              <Button
                type="primary"
                icon={<CheckOutlined />}
                iconPosition="end"
                htmlType="submit"
              >
                Save Change
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
        zIndex={20}
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
            Do you want to add a new user?
          </p>
          <span className="text-gray-500">
            The data set you fill in now will automatically become a new user
          </span>
        </div>
      </Modal>
      {/* modal confirm eidt */}
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
        zIndex={20}
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
            Do you want to update user data?
          </p>
          <span className="text-gray-500">
            The data you update now will automatically change in the user list.
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
          <p className="font-bold text-lg" style={{ marginBottom: 0 }}>
            Are you sure you want to delete this user?
          </p>
          <span className="text-gray-500">
            Deleting this user will delete all items within it. Make sure this
            action before continuing..
          </span>
        </div>
      </Modal>
    </>
  );
};

export default TableList;
