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
  Typography,
  Form,
  Tag,
  Spin,
  Select,
  message,
  Tooltip,
} from "antd";
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
import type { PaginationProps, TableProps } from "antd";
import apiClient from "../../../services/apiClient";
import { useAuth } from "../../../context/AuthContext";
// import debounce from "lodash/debounce";
import dayjs from "dayjs";
import customParseFormat from "dayjs/plugin/customParseFormat";
import ShowDocumentOnly from "../../atom/document/ShowDocumentOnly";
dayjs.extend(customParseFormat);
const { Text, Paragraph } = Typography;
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
  const { config } = useAuth();
  const pageLengthConfig = config?.dataGrid?.rowPerPage || 5;
  const dateConfig =
    config?.dataGrid?.dateTimeFormat || "YYYY-MM-DD | HH:mm:ss";
  const [data, setData] = useState<any[]>([]);
  const [loading, setLoading] = useState(false);
  const [page, setPage] = useState<number>(1);
  const pageLength = pageLengthConfig;
  const [totalRow, setTotalRow] = useState<number>(0);

  const [loadingCheck, setLoadingCheck] = useState(false);
  const [detail, setDetail] = useState<any>({});
  const [open, setOpen] = useState(false);
  const [appoval, setApproval] = useState<string>("");
  const [openConfirm, setOpenConfirm] = useState(false);
  const [confirmAddLoading, setConfirmAddLoading] = useState(false);
  const [form] = Form.useForm();

  const [optListDocument, setOptListDocument] = useState<any[]>([]);
  const [fetching] = useState(false);
  const [fileSelected, setFileSelected] = useState<any>({});
  const [openDocument, setOpenDocument] = useState(false);

  const columns = [
    {
      key: "requestID",
      dataIndex: "requestID",
      title: "Request ID",
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
      dataIndex: "initiator",
      key: "initiator",
      title: "Initiator",
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
      dataIndex: "approvalDate",
      key: "approvalDate",
      title: "Aproval Date",
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
          type="primary"
          icon={<ScanEye size={18} />}
          iconPosition="end"
          onClick={() => handleCheck(row.requestID)}
          disabled={row.approvalStatus == 2 ? true : false}
        >
          {" "}
          Check Approval
        </Button>
      ),
    },
  ];

  const userColumns = [
    {
      key: "fullName",
      dataIndex: "fullName",
      title: "Name",
      render: (_items: any, row: any) => {
        return (
          // <Flex gap="large" align='center'>
          //     <Badge dot offset={[0, 40]} color="green" style={{width: 13, height: 13}}><Avatar size="large" icon={<UserOutlined />} /></Badge>
          //     <span className='font-bold'>{item}</span>
          // </Flex>
          <Flex gap="large" align="center">
            <Avatar size="large" icon={<UserOutlined />} />
            <span className="font-bold">{row.userName}</span>
          </Flex>
        );
      },
    },

    {
      dataIndex: "status",
      key: "status",
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
            {row.approvalActivityDesc}{" "}
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
      dataIndex: "reason",
      key: "reason",
      title: "Reason",
      width: "40%",
      render: (item: any) => {
        return (
          <Paragraph ellipsis={{ rows: 2, expandable: true, symbol: "more" }}>
            {item}
          </Paragraph>
        );
      },
    },
    {
      dataIndex: "approvalDate",
      key: "approvalDate",
      title: "Date",
      render: (items: any) => (
        <Flex vertical>
          <span>{items ? dayjs.utc(items).format(dateConfig) : ""}</span>
        </Flex>
      ),
    },
  ];

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

  const handleClickShow = (value: any) => {
    setFileSelected(value);
    setOpenDocument(true);
  };

  useEffect(() => {
    getData();
  }, []);

  const getData = () => {
    setLoading(true);
    apiClient
      .get(`/approval/get-approval-task?Page=${page}&Limit=${pageLength}`)
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

  const handleCheck = (id: number) => {
    fetchCheck(id);
    setOpen(true);
  };
  const fetchCheck = async (id: number) => {
    setLoadingCheck(true);
    apiClient
      .get(`/approval/check-approval?approvalId=${id}`)
      .then(({ data }) => {
        setDetail(data?.data || {});

        const docs = data?.data?.documentFiles || [];
        const formatted = docs.map((item: any) => ({
          label: item.documentFileName,
          value: item.id,
        }));
        setOptListDocument(formatted);
      })
      .catch((err) => {
        console.log(err);
      })
      .finally(() => {
        setLoadingCheck(false);
      });
  };

  const handleActionApproval = async () => {
    const payload = form.getFieldsValue();
    payload.relatedDocumentId = payload?.relatedDocumentId?.value || null;
    payload.id = detail?.approvalID || null;
    payload.documentID = detail?.documentID || null;
    payload.categoryID = detail?.categoryID || null;

    if (appoval === "reject" && !payload.reason) {
      // close confirm modal and notify user
      setOpenConfirm(false);
      message.error("Please fill reason if reject");
      return;
    }

    setConfirmAddLoading(true);

    try {
      if (appoval === "reject") {
        const { data } = await apiClient.post(`/approval/reject`, payload);
        console.log(data);
        message.success("Successfully rejected");
        // close main modal on success
        setOpen(false);
      } else if (appoval === "approve") {
        const { data } = await apiClient.post(`/approval/approve`, payload);
        console.log(data);
        message.success("Successfully approved");
        // close main modal on success
        setOpen(false);
      }

      // refresh table data after action
      try {
        getData();
      } catch {
        /* ignore refresh errors */
      }
    } catch (err) {
      console.log(err);
      // @ts-expect-error - err typing comes from unknown axios error shape
      message.error(err?.response?.data?.message || "Something went wrong");
    } finally {
      // always close the confirm modal and reset loading/state
      setOpenConfirm(false);
      setConfirmAddLoading(false);
      setApproval("");
    }
  };

  const handleTableChange: TableProps<Record<string, unknown>>["onChange"] = (
    pagination
  ) => {
    setPage(Number(pagination.current));
  };

  // const getListdOC = async (search: string) => {
  //     setFetching(true);
  //     try {
  //         const res = await apiClient.get(`/dropdown/ddldocuments?DocumentTitle=${search}`);
  //         const data = await res?.data?.data || [];
  //         const formatted = data.map((item: any) => ({
  //             label: item.text,
  //             value: item.value,
  //         }));
  //         setOptListDocument(formatted);
  //     } catch (err) {
  //         console.error(err);
  //         setOptListDocument([]);
  //     }
  //     setFetching(false);
  // };

  // const debounceFetcher = useMemo(() => {
  //     return debounce((value: string) => {
  //         getListdOC(value);
  //     }, 500);
  // }, []);

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
            <Table
              rowKey="requestID"
              scroll={{ x: 1000 }}
              onChange={handleTableChange}
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
                <span>Add Workflow</span>
                <Text type="secondary" style={{ fontWeight: "normal" }}>
                  Create a new User by filling in the form bellow.
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
        {loadingCheck ? (
          <div className="flex justify-center py-10">
            <Spin />
          </div>
        ) : (
          <>
            <Divider style={{ marginBottom: 0 }} />
            <div style={{ padding: 24, paddingBottom: 10 }}>
              <p className="text-slate-500">Notes</p>
              <span>{detail.notes ?? "-"}</span>
            </div>
            <Form layout="vertical" form={form}>
              <div style={{ padding: 24, paddingBottom: 10 }}>
                <Form.Item
                  label="Reason"
                  name="reason"
                  rules={[{ type: "string" }]}
                >
                  <TextArea rows={4} placeholder="enter reason..." />
                </Form.Item>
                <Form.Item label="Related Document" name="relatedDocumentId">
                  <Select
                    showSearch
                    labelInValue
                    placeholder="Select document by search..."
                    filterOption={false}
                    notFoundContent={
                      fetching ? <Spin size="small" /> : "No results found"
                    }
                    options={optListDocument}
                  />
                </Form.Item>
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
                <br />
                <Card styles={{ body: { padding: 0 } }}>
                  <Table
                    rowKey="id"
                    pagination={false}
                    scroll={{ x: 500 }}
                    columns={userColumns}
                    dataSource={detail?.approvalActivities || []}
                  />
                </Card>
              </div>
              <Divider style={{ marginTop: 0, marginBottom: 0 }} />
              <div
                style={{ padding: 20, paddingBottom: 20, justifySelf: "end" }}
              >
                <Space>
                  <Button
                    onClick={() => {
                      setApproval("reject");
                      setOpenConfirm(true);
                    }}
                    icon={<CloseOutlined />}
                    iconPosition="end"
                    color="danger"
                    variant="filled"
                  >
                    Reject
                  </Button>
                  <Button
                    type="primary"
                    onClick={() => {
                      setApproval("approve");
                      setOpenConfirm(true);
                    }}
                    icon={<CheckOutlined />}
                    iconPosition="end"
                  >
                    Approve
                  </Button>
                </Space>
              </div>
            </Form>
          </>
        )}
      </Modal>
      {/* modal confirm */}
      <Modal
        title={
          <div style={{ paddingLeft: 24, paddingRight: 24, paddingTop: 24 }}>
            <ExclamationCircleOutlined
              style={{ fontSize: 30, color: "gold" }}
            />
          </div>
        }
        open={openConfirm}
        onCancel={() => {
          setOpenConfirm(false);
          setApproval("");
        }}
        maskClosable={false}
        footer={() => (
          <>
            <Row gutter={12}>
              <Col span={12}>
                <Button
                  icon={<CloseOutlined />}
                  iconPosition="end"
                  onClick={() => {
                    setOpenConfirm(false);
                    setApproval("");
                  }}
                  variant="filled"
                  loading={confirmAddLoading}
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
                  onClick={handleActionApproval}
                  loading={confirmAddLoading}
                  block
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
            Are you sure with your decision to{" "}
            {appoval == "reject" ? "Reject" : "Approve"}?
          </p>
          <span className="text-gray-500">
            Make sure this workflow decision on this document it's the right
            decisin.
          </span>
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
