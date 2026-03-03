import "./style.css";
import {
  Avatar,
  Button,
  Card,
  Col,
  Divider,
  Flex,
  Modal,
  Row,
  Space,
  Table,
  Typography,
  Tag,
  Tooltip,
} from "antd";
import type { TableProps } from "antd";
import {
  ArrowLeftOutlined,
  ArrowRightOutlined,
  CheckOutlined,
  ClockCircleOutlined,
  CloseOutlined,
  ExclamationCircleOutlined,
  PartitionOutlined,
  UserOutlined,
} from "@ant-design/icons";
import { useEffect, useState } from "react";
import { ScanEye } from "lucide-react";
import type { PaginationProps } from "antd";
import apiClient from "../../../services/apiClient";
import { useAuth } from "../../../context/AuthContext";
import ShowDocumentOnly from "../../atom/document/ShowDocumentOnly";
import dayjs from "dayjs";
import customParseFormat from "dayjs/plugin/customParseFormat";
dayjs.extend(customParseFormat);
const { Text, Paragraph } = Typography;

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

interface DataType {
  key: string;
  req_id: string;
  status: string;
  req_date: string;
  approval_step: string;
  approver: string;
  next_approval: string;
  response_date: string;
}

const TableList = () => {
  const { config } = useAuth();
  const pageLengthConfig = config?.dataGrid?.rowPerPage || 5;
  const dateConfig =
    config?.dataGrid?.dateTimeFormat || "YYYY-MM-DD | HH:mm:ss";
  const [data, setData] = useState<DataType[]>([]);
  const [detail, setDetail] = useState<any>({});
  const [loading, setLoading] = useState(false);
  const [page, setPage] = useState<number>(1);
  const pageLength = pageLengthConfig;
  const [totalRow, setTotalRow] = useState<number>(0);

  const [open, setOpen] = useState(false);
  const [fileSelected, setFileSelected] = useState<any>({});
  const [openDocument, setOpenDocument] = useState(false);

  const columns: TableProps<DataType>["columns"] = [
    {
      key: "requestID",
      dataIndex: "requestID",
      title: "Request ID",
    },
    {
      key: "documentTitle",
      dataIndex: "documentTitle",
      title: "Document Name",
    },
    {
      key: "approvalStatus",
      dataIndex: "approvalStatus",
      title: "Status",
      render: (items: any, row: any) => {
        return (
          <Tag
            color={
              items == 1
                ? "orange"
                : items == 2
                ? "green"
                : items == 3
                ? "magenta"
                : "default"
            }
            style={{ height: "100%", paddingInline: 10 }}
          >
            {row.approvalStatusName}{" "}
            {items == 1 ? (
              <ClockCircleOutlined />
            ) : items == 2 ? (
              <CheckOutlined />
            ) : items == 3 ? (
              <ExclamationCircleOutlined />
            ) : (
              ""
            )}
          </Tag>
        );
      },
    },
    {
      dataIndex: "requestDate",
      key: "requestDate",
      title: "Request Date",
      render: (items: any) => (
        <Flex vertical>
          <span>{items ? dayjs.utc(items).format(dateConfig) : ""}</span>
        </Flex>
      ),
    },
    {
      dataIndex: "approvalStep",
      key: "approvalStep",
      title: "Approval Step",
    },
    {
      dataIndex: "approver",
      key: "approver",
      title: "Approver",
    },
    {
      dataIndex: "nextApprover",
      key: "nextApprover",
      title: "Next Approval",
    },
    {
      dataIndex: "responseDate",
      key: "responseDate",
      title: "Response Date",
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
        <Button
          icon={<ScanEye size={18} />}
          iconPosition="end"
          onClick={() => handleClickDetail(row)}
          loading={loadingDetail}
        />
      ),
    },
  ];
  const [loadingDetail, setLoadingDetail] = useState(false);

  const fileDetailColumns = [
    {
      key: "documentFileName",
      dataIndex: "documentFileName",
      title: "File",
      render: (item: any) => (
        <span className="text-blue-500 font-semibold">{item}</span>
      ),
    },
    {
      key: "updatedByFullName",
      dataIndex: "updatedByFullName",
      title: "Updated By",
    },
    {
      key: "documentFileSize",
      dataIndex: "documentFileSize",
      title: "File Size",
    },
    {
      title: "Action",
      key: "action",
      render: (items: any) => (
        <Tooltip placement="bottom" title="View" color="#595959">
          <Button
            type="text"
            icon={<ScanEye size={18} />}
            onClick={() => handleClickShow(items)}
          />
        </Tooltip>
      ),
    },
  ];

  const activityColumns = [
    {
      key: "userName",
      dataIndex: "userName",
      title: "Username",
      render: (item: any, row: any) => (
        <Flex gap="large" align="center">
          <Avatar size="large" icon={<UserOutlined />} />
          <span className="font-bold">{row.userName}</span>
        </Flex>
      ),
    },
    {
      key: "approvalActivity",
      dataIndex: "approvalActivityDesc",
      title: "Status",
      render: (item: any) => (
        <Tag
          color="default"
          className="rounded-md border-green-200 bg-green-50 text-green-600 px-3 py-0.5"
        >
          {item}
        </Tag>
      ),
    },
    {
      key: "reason",
      dataIndex: "reason",
      title: "Reason",
      render: (item: any) => (
        <Paragraph
          ellipsis={{ rows: 1, expandable: false }}
          className="mb-0! text-slate-500"
        >
          {item || "-"}
        </Paragraph>
      ),
    },
    {
      key: "approvalDate",
      dataIndex: "approvalDate",
      title: "Date",
      render: (item: any) => (
        <span className="text-slate-500">
          {item ? dayjs.utc(item).format("DD/MM/YY HH:mm:ss") : "-"}
        </span>
      ),
    },
  ];

  const handleClickShow = (value: any) => {
    setFileSelected(value);
    setOpenDocument(true);
  };

  const handleClickDetail = (item: any) => {
    setLoadingDetail(true);
    apiClient
      .get(`/approval/check-approval?approvalId=${item.requestID}`)
      .then(({ data }) => {
        setDetail(data.data);
        setOpen(true);
      })
      .catch((err) => {
        console.log(err);
      })
      .finally(() => {
        setLoadingDetail(false);
      });
  };

  useEffect(() => {
    getData();
  }, [page]);

  useEffect(() => {
    if (!open) {
      setDetail({});
    }
  }, [open]);

  const getData = () => {
    setLoading(true);
    apiClient
      .get(`/approval/get-approval-request?Page=${page}&Limit=${pageLength}`)
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

  const handleTableChange: TableProps<any>["onChange"] = (pagination) => {
    setPage(Number(pagination.current));
  };

  return (
    <>
      <Row>
        <Col span={24}>
          <Card
            styles={{
              body: {
                padding: 0,
              },
            }}
          >
            <Table<DataType>
              rowKey="requestID"
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
      {/* modal detail workflow */}
      <Modal
        title={
          <div
            className="space-align-block"
            style={{ paddingLeft: 24, paddingRight: 24, paddingTop: 24 }}
          >
            <Space align="center" size="middle">
              <PartitionOutlined
                style={{
                  border: "1px solid #d9d9d9",
                  padding: 7,
                  fontSize: 30,
                  color: "#434343",
                  borderRadius: 7,
                }}
              />
              <Space.Compact direction="vertical">
                <span className="text-xl font-bold">View Workflow</span>
                <Text type="secondary" style={{ fontWeight: "normal" }}>
                  Create a new user by filling in the form below.
                </Text>
              </Space.Compact>
            </Space>
          </div>
        }
        open={open}
        onCancel={() => setOpen(false)}
        maskClosable={false}
        footer={false}
        width={770}
        styles={{
          content: {
            padding: 0,
          },
        }}
        style={{ top: 20 }}
      >
        <Divider style={{ marginBottom: 0 }} />
        <div style={{ padding: 24 }}>
          <Space direction="vertical" size="large" style={{ width: "100%" }}>
            {/* Notes Section */}
            <div>
              <Text className="text-slate-400 block mb-2 font-semibold">
                Notes
              </Text>
              <Text className="text-slate-700">{detail?.notes || "-"}</Text>
            </div>

            <Divider style={{ margin: 0 }} />

            {/* File Section */}
            <div>
              <Text className="text-slate-400 block mb-3 font-semibold">
                File
              </Text>
              <Card styles={{ body: { padding: 0 } }}>
                <Table
                  columns={fileDetailColumns}
                  dataSource={detail?.documentFiles || []}
                  pagination={false}
                  rowKey="id"
                  scroll={{ x: 500 }}
                />
              </Card>
            </div>

            <Card styles={{ body: { padding: 0 } }}>
              <Table
                columns={activityColumns}
                dataSource={detail?.approvalActivities || []}
                pagination={false}
                scroll={{ x: 500 }}
              />
            </Card>
          </Space>
        </div>

        <Divider style={{ marginTop: 0, marginBottom: 0 }} />
        <div
          style={{
            padding: 20,
            paddingBottom: 20,
            display: "flex",
            justifyContent: "end",
          }}
        >
          <Button
            icon={<CloseOutlined />}
            iconPosition="end"
            onClick={() => setOpen(false)}
            color="danger"
            variant="filled"
          >
            Cancel
          </Button>
        </div>
      </Modal>

      <ShowDocumentOnly
        open={openDocument}
        setOpen={setOpenDocument}
        file={fileSelected}
      />
    </>
  );
};

export default TableList;
