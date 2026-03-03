import "./style.css";
import { useEffect, useState } from "react";
import apiClient from "../../../services/apiClient";
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
  PicLeftOutlined,
  SearchOutlined,
} from "@ant-design/icons";
import { Pencil, ScanEye, Trash2 } from "lucide-react";
import type { PaginationProps } from "antd";
import { useNavigate } from "react-router-dom";
import dayjs from "dayjs";
import customParseFormat from "dayjs/plugin/customParseFormat";
import { useAuth } from "../../../context/AuthContext";
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
        {/* Next */}
      </Button>
    );
  }
  return originalElement;
};

const TableList = () => {
  const navigate = useNavigate();
  const { config } = useAuth();
  const dateConfig =
    config?.dataGrid?.dateTimeFormat || "YYYY-MM-DD | HH:mm:ss";
  const pageLengthConfig = config?.dataGrid?.rowPerPage || 5;
  const [data, setData] = useState<any>([]);
  const [totolRow, setTotalRow] = useState<number>(0);
  const [page, setPage] = useState<number>(1);
  const pageLength = pageLengthConfig;
  const [loading, setLoading] = useState<boolean>(false);
  const [eventData, setEventData] = useState<any>({});
  const [openConfirmDelete, setOpenConfirmDelete] = useState(false);
  const [loadingDelete, setLoadingDelete] = useState(false);
  const [search, setSearch] = useState<string>("");

  const columns: TableProps<any>["columns"] = [
    {
      key: "collectionName",
      dataIndex: "collectionName",
      title: "Collection Name",
    },
    {
      key: "insertedByFullName",
      dataIndex: "insertedByFullName",
      title: "Owner",
      render: (items: any, row: any) => {
        return (
          <>
            <div>{items}</div>
            <div>{dayjs.utc(row.insertedAt).format(dateConfig)}</div>
          </>
        );
      },
    },
    {
      dataIndex: "updatedAt",
      key: "updatedAt",
      title: "Last Update",
      render: (items: any, row: any) => {
        return (
          <>
            <div>{row.updatedByFullName}</div>
            <div>{dayjs.utc(items).format(dateConfig)}</div>
          </>
        );
      },
    },
    {
      title: "Action",
      key: "action",
      render: (_, row) => (
        <Space size="middle">
          <Tooltip placement="bottom" title="Detail" color="#595959">
            <Button
              type="text"
              icon={<ScanEye size={18} color="#595959" />}
              onClick={() =>
                navigate(`/attributes/collection-detail/${row.id}`)
              }
            />
          </Tooltip>
          <Tooltip placement="bottom" title="Edit" color="#595959">
            <Button
              type="text"
              icon={<Pencil size={18} color="#595959" />}
              onClick={() => navigate(`/attributes/collection-edit/${row.id}`)}
            />
          </Tooltip>
          <Tooltip placement="bottom" title="Delete" color="#595959">
            <Button
              type="text"
              icon={<Trash2 size={18} color="#595959" />}
              onClick={() => handleDelete(row.id)}
            />
          </Tooltip>
        </Space>
      ),
    },
  ];

  const getDataTable = () => {
    setLoading(true);
    apiClient
      .get(
        `/attributecollections?search=${search}&Page=${page}&Limit=${pageLength}`
      )
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

  const handleTableChange: TableProps<any>["onChange"] = (pagination) => {
    setPage(Number(pagination.current));
  };

  const handleDelete = (value: any) => {
    setEventData(value);
    setOpenConfirmDelete(true);
  };

  const handleDeleteOk = () => {
    if (eventData) {
      setLoadingDelete(true);
      apiClient
        .put(`/attributecollections/delete?id=${eventData}`)
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
            type="primary"
            icon={<PicLeftOutlined />}
            iconPosition="end"
            onClick={() => navigate("/attributes/collection-add")}
          >
            Add Collection
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
            Are you sure you want to delete a Collection?
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
