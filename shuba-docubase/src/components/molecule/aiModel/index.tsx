import { useEffect, useState } from "react";
import {
  Button,
  Card,
  Col,
  Input,
  message,
  Modal,
  Row,
  Space,
  Table,
  Tooltip,
} from "antd";
import type { TableProps } from "antd";
import {
  ArrowLeftOutlined,
  ArrowRightOutlined,
  CloseOutlined,
  ExclamationCircleOutlined,
  PlusOutlined,
  SearchOutlined,
} from "@ant-design/icons";
import { Pencil, ScanEye, Trash2 } from "lucide-react";
import type { PaginationProps } from "antd";
import { useAuth } from "../../../context/AuthContext";
import apiClient from "../../../services/apiClient";
import dayjs from "dayjs";
import customParseFormat from "dayjs/plugin/customParseFormat";
import Breadcrumb from "../../atom/Breadcrumb";
import ModalAddEditAiModel from "./ModalAddEditAiModel";
import ModalViewAiModel from "./ModalViewAiModel";
dayjs.extend(customParseFormat);

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
      </Button>
    );
  }
  return originalElement;
};

const AiModelList = () => {
  const { config } = useAuth();
  const dateConfig =
    config?.dataGrid?.dateTimeFormat || "YYYY-MM-DD | HH:mm:ss";
  const pageLengthConfig = config?.dataGrid?.rowPerPage || 5;
  const [data, setData] = useState<any[]>([]);
  const [totalRow, setTotalRow] = useState<number>(0);
  const [page, setPage] = useState<number>(1);
  const pageLength = pageLengthConfig;
  const [loading, setLoading] = useState<boolean>(false);
  const [search, setSearch] = useState<string>("");
  const [openAdd, setOpenAdd] = useState(false);
  const [openEdit, setOpenEdit] = useState(false);
  const [openView, setOpenView] = useState(false);
  const [openConfirmDelete, setOpenConfirmDelete] = useState(false);
  const [confirmDeleteLoading, setConfirmDeleteLoading] = useState(false);
  const [eventData, setEventData] = useState<any>({});

  const columns: TableProps<any>["columns"] = [
    {
      key: "modelName",
      dataIndex: "modelName",
      title: "Model Name",
    },
    {
      key: "provider",
      dataIndex: "provider",
      title: "Provider",
    },
    {
      key: "maxToken",
      dataIndex: "maxToken",
      title: "Max Token",
    },
    {
      key: "lastUpdate",
      dataIndex: "updatedAt",
      title: "Last Update",
      render: (items: any, row: any) => {
        return (
          <>
            <div>{row.updatedByFullName || "-"}</div>
            <div>{items ? dayjs.utc(items).format(dateConfig) : "-"}</div>
          </>
        );
      },
    },
    {
      title: "Action",
      key: "action",
      render: (_: any, row: any) => (
        <Space size="middle">
          <Tooltip placement="bottom" title="View" color="#595959">
            <Button
              type="text"
              icon={<ScanEye size={18} color="#595959" />}
              onClick={() => handleView(row)}
            />
          </Tooltip>
          <Tooltip placement="bottom" title="Edit" color="#595959">
            <Button
              type="text"
              icon={<Pencil size={18} color="#595959" />}
              onClick={() => handleEdit(row)}
            />
          </Tooltip>
          <Tooltip placement="bottom" title="Delete" color="#595959">
            <Button
              type="text"
              icon={<Trash2 size={18} color="#595959" />}
              onClick={() => handleDelete(row)}
            />
          </Tooltip>
        </Space>
      ),
    },
  ];

  useEffect(() => {
    getData();
  }, [page, pageLength]);

  const getData = () => {
    setLoading(true);
    const searchParam = search ? `&search=${search}` : "";
    apiClient
      .get(`/AiModel/filter?page=${page}&limit=${pageLength}${searchParam}`)
      .then(({ data }) => {
        setData(data?.data?.data || []);
        setTotalRow(data?.data?.totalRecords || 0);
        setLoading(false);
      })
      .catch((err) => {
        console.error(err);
        setLoading(false);
      });
  };

  const handleKeyDown = (event: any) => {
    if (event.key === "Enter" || event.code === "Enter") {
      setPage(1);
      getData();
    }
  };

  const handleTableChange: TableProps<any>["onChange"] = (pagination) => {
    setPage(Number(pagination.current));
  };

  const handleView = (row: any) => {
    setEventData(row);
    setOpenView(true);
  };

  const handleEdit = (row: any) => {
    setEventData(row);
    setOpenEdit(true);
  };

  const handleDelete = (row: any) => {
    setEventData(row);
    setOpenConfirmDelete(true);
  };

  const handleOkDelete = () => {
    if (!eventData || !eventData.id)
      return message.error("AI Model ID not found!");
    setConfirmDeleteLoading(true);
    apiClient
      .put(`/AiModel/delete?id=${eventData.id}`)
      .then(() => {
        setConfirmDeleteLoading(false);
        setOpenConfirmDelete(false);
        setEventData({});
        getData();
        message.success("Delete AI Model is successful");
      })
      .catch((err) => {
        console.error(err);
        message.error(err.response?.data?.message || "Delete AI Model failed");
        setConfirmDeleteLoading(false);
        setOpenConfirmDelete(false);
      });
  };

  const handleAfterOpenChange = (visible: boolean) => {
    if (!visible) {
      setEventData({});
    }
  };

  return (
    <>
      <Breadcrumb
        item={[
          { title: <span style={{ fontWeight: "bold" }}>AI Model</span> },
        ]}
      />
      <Row style={{ marginBottom: 20 }}>
        <Col span={24}>
          <div className="grid grid-cols-2 lg:grid-cols-12 gap-4 mb-5">
            <div className="col-span-2 lg:col-span-3">
              <Input
                size="middle"
                onChange={(e: any) => setSearch(e.target.value)}
                onKeyDown={handleKeyDown}
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
                Add AI Model
              </Button>
            </div>
          </div>
        </Col>
      </Row>
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
              rowKey="id"
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
      {/* Modal Add */}
      <ModalAddEditAiModel
        open={openAdd}
        setOpen={setOpenAdd}
        rerenderData={getData}
        afterOpenChange={handleAfterOpenChange}
      />
      {/* Modal Edit */}
      <ModalAddEditAiModel
        open={openEdit}
        setOpen={setOpenEdit}
        rerenderData={getData}
        eventData={eventData}
        afterOpenChange={handleAfterOpenChange}
      />
      {/* Modal View */}
      <ModalViewAiModel
        open={openView}
        setOpen={setOpenView}
        eventData={eventData}
      />
      {/* Modal Confirm Delete */}
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
            Are you sure you want to delete this AI Model?
          </p>
          <span className="text-gray-500">
            Deleting this AI Model will permanently remove it. Make sure you
            have backed up important data before continuing.
          </span>
        </div>
      </Modal>
    </>
  );
};

export default AiModelList;
