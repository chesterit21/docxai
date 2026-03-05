import "./style.css";
import ModalAttribute from "../../atom/dynamicForm/ModalAddAttribute";
import ModalEditAttribute from "../../atom/dynamicForm/ModalUpdateAttribute";
import {
  Button,
  Card,
  Col,
  Input,
  Modal,
  Row,
  Space,
  Table,
  Tooltip,
  message,
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
import { useEffect, useState } from "react";
import { Pencil, Trash2 } from "lucide-react";
import type { PaginationProps } from "antd";
import apiClient from "../../../services/apiClient";
import dayjs from "dayjs";
import customParseFormat from "dayjs/plugin/customParseFormat";
dayjs.extend(customParseFormat);
import { useAuth } from "../../../context/AuthContext";

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

const TableList = () => {
  const { config } = useAuth();
  const dateConfig =
    config?.dataGrid?.dateTimeFormat || "YYYY-MM-DD | HH:mm:ss";
  const pageLengthConfig = config?.dataGrid?.rowPerPage || 5;
  const [data, setData] = useState<any>([]);
  const [totolRow, setTotalRow] = useState<number>(0);
  const [page, setPage] = useState<number>(1);
  const pageLength = pageLengthConfig;
  const [openAdd, setOpenAdd] = useState(false);
  const [openEdit, setOpenEdit] = useState(false);
  const [eventData, setEventData] = useState<any>({});
  const [loading, setLoading] = useState<boolean>(false);
  const [openConfirmDelete, setOpenConfirmDelete] = useState(false);
  const [loadingDelete, setLoadingDelete] = useState(false);
  const [search, setSearch] = useState<string>("");

  const columns: TableProps<any>["columns"] = [
    {
      key: "attributeName",
      dataIndex: "attributeName",
      title: "Attribute Name",
    },
    {
      key: "attributeType",
      dataIndex: "attributeType",
      title: "Group Member",
    },
    {
      key: "insertedByFullName",
      dataIndex: "insertedByFullName",
      title: "Owner",
      render: (_items: any, row: any) => {
        return (
          <>
            <div>{row.insertedByByFullName}</div>
            <div>{dayjs.utc(row.insertedAt).format(dateConfig)}</div>
          </>
        );
      },
    },
    {
      dataIndex: "updatedAt",
      key: "updatedAt",
      title: "Last Update",
      render: (_: any, row: any) => {
        return (
          <>
            <div>{row.updatedByFullName}</div>
            <div>{dayjs.utc(_).format(dateConfig)}</div>
          </>
        );
      },
    },
    {
      title: "Action",
      key: "action",
      render: (_, row) => (
        <Space size="middle">
          <Tooltip placement="bottom" title="Edit" color="#595959">
            <Button
              type="text"
              icon={
                <Pencil
                  size={18}
                  color="#595959"
                  onClick={() => [
                    console.log("row edit", row),
                    handleUpdateClick(row),
                  ]}
                />
              }
            />
          </Tooltip>
          {!row.isSystem && (
            <Tooltip placement="bottom" title="Delete" color="#595959">
              <Button
                type="text"
                icon={<Trash2 size={18} color="#595959" />}
                onClick={() => handleDelete(row.id)}
              />
            </Tooltip>
          )}
        </Space>
      ),
    },
  ];

  const getDataTable = () => {
    setLoading(true);
    apiClient
      .get(`/attribute?search=${search}&Page=${page}&Limit=${pageLength}`)
      .then(({ data }) => {
        setData(data.data.data);
        setTotalRow(data.data.totalRecords);
        setLoading(false);
      })
      .catch((err) => {
        console.log(err);
        setLoading(false);
      });
  };

  useEffect(() => {
    getDataTable();
  }, [page, pageLength]);

  const handleUpdateClick = (item: any) => {
    setEventData(item);
    setOpenEdit(true);
  };

  const handleDelete = (value: any) => {
    setEventData(value);
    setOpenConfirmDelete(true);
  };

  const handleDeleteOk = () => {
    if (eventData) {
      setLoadingDelete(true);
      apiClient
        .put(`/attribute/delete?id=${eventData}`)
        .then(({ data }) => {
          setLoadingDelete(false);
          console.log(data);
          setOpenConfirmDelete(false);
          getDataTable();
          message.success("Delete data is successful");
        })
        .catch((err) => {
          console.log(err, "error delete");
          setLoadingDelete(false);
          setOpenConfirmDelete(false);
          message.error(err?.response?.data?.message || "Failed to delete");
        });
    }
    setEventData({});
  };

  const handleTableChange: TableProps<any>["onChange"] = (pagination) => {
    setPage(Number(pagination.current));
  };

  const handleKeyDownDocument = (event: any) => {
    if (event.key === "Enter" || event.code === "Enter") {
      const str = search.length > 0 ? search : "";
      setSearch(str);
      getDataTable();
    }
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
            icon={<PlusOutlined />}
            iconPosition="end"
            onClick={() => setOpenAdd(true)}
          >
            Add Attribute
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
              rowKey="id"
              onChange={handleTableChange}
              scroll={{ x: 1000 }}
              pagination={{
                position: ["bottomCenter"],
                itemRender: itemRender,
                pageSize: pageLength,
                total: totolRow,
              }}
              columns={columns}
              dataSource={data}
              loading={loading}
            />
          </Card>
        </Col>
      </Row>
      {/* modal add */}
      <ModalAttribute
        open={openAdd}
        setOpen={setOpenAdd}
        rerenderData={getDataTable}
      />
      {/* modal edit */}
      <ModalEditAttribute
        evenData={eventData}
        setEventData={setEventData}
        open={openEdit}
        setOpen={setOpenEdit}
        rerenderData={getDataTable}
      />
      {/* modal confirm delete */}
      <Modal
        title={
          <div style={{ paddingLeft: 24, paddingRight: 24, paddingTop: 24 }}>
            <ExclamationCircleOutlined style={{ fontSize: 30, color: "red" }} />
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
                  variant="filled"
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
                  onClick={handleDeleteOk}
                  block
                  loading={loadingDelete}
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
            Are you sure you want to delete an Attribute?
          </p>
          <span className="text-gray-500">
            Make sure this action will delete the data.
          </span>
        </div>
      </Modal>
    </>
  );
};

export default TableList;
