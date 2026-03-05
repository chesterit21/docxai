import {
  Card,
  Table,
  type PaginationProps,
  Button,
  type TableProps,
  DatePicker,
  Tag,
  message,
  Modal,
} from "antd";
import {
  ArrowLeftOutlined,
  ArrowRightOutlined,
  DownloadOutlined,
  SearchOutlined,
  SendOutlined,
} from "@ant-design/icons";
import { useEffect, useState } from "react";
import { useAuth } from "../../../context/AuthContext";
import apiClient from "../../../services/apiClient";
import * as XLSX from "xlsx";
import { saveAs } from "file-saver";
import dayjs from "dayjs";
import customParseFormat from "dayjs/plugin/customParseFormat";
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

export default function Index() {
  const { config } = useAuth();
  const dateConfig =
    config?.dataGrid?.dateTimeFormat || "YYYY-MM-DD | HH:mm:ss";
  const pageLengthConfig = config?.dataGrid?.rowPerPage || 5;

  const [data, setData] = useState<any[]>([]);
  const [page, setPage] = useState<number>(1);
  const pageLengthDocument = pageLengthConfig;

  const [totalRow, setTotalRow] = useState<number>(0);
  const [loading, setLoading] = useState(false);
  const [loadingDownload, setLoadingDownload] = useState(false);
  const [startDate, setStartDate] = useState<string | null>(() =>
    dayjs.utc().subtract(1, "day").format("YYYY-MM-DD")
  );
  const [endDate, setEndDate] = useState<string | null>(() =>
    dayjs.utc().format("YYYY-MM-DD")
  );
  const [selectedRowKeys, setSelectedRowKeys] = useState<React.Key[]>([]);
  const [loadingResend, setLoadingResend] = useState(false);

  const handleResendSingle = (id: string) => {
    setLoadingResend(true);
    apiClient
      .put(`/email/resend?id=${id}`)
      .then(() => {
        message.success("Email resend success");
        getData();
      })
      .catch((err) => {
        message.error(err?.response?.data?.message || "Failed to resend email");
        console.log(err);
      })
      .finally(() => setLoadingResend(false));
  };

  const handleResendMultiple = () => {
    Modal.confirm({
      title: "Resend Email",
      content: `Are you sure you want to resend ${selectedRowKeys.length} emails?`,
      onOk: () => {
        setLoadingResend(true);
        apiClient
          .post(`/email/multi-resend`, { ids: selectedRowKeys })
          .then(() => {
            message.success("Multi resend email success");
            setSelectedRowKeys([]);
            getData();
          })
          .catch((err) => {
            message.error(
              err?.response?.data?.message || "Failed to multi resend email"
            );
            console.log(err);
          })
          .finally(() => setLoadingResend(false));
      },
    });
  };

  const onSelectChange = (newSelectedRowKeys: React.Key[]) => {
    console.log("selectedRowKeys changed: ", newSelectedRowKeys);
    setSelectedRowKeys(newSelectedRowKeys);
  };

  const rowSelection = {
    selectedRowKeys,
    onChange: onSelectChange,
  };

  const columns = [
    {
      key: "subject",
      dataIndex: "subject",
      title: "Subject",
      onCell: () => ({
        style: { verticalAlign: "top" },
      }),
    },
    {
      dataIndex: "to",
      key: "to",
      title: "To",
      onCell: () => ({
        style: { verticalAlign: "top" },
      }),
      render: (items: any) => transformData(items),
    },
    {
      dataIndex: "cc",
      key: "cc",
      title: "CC",
      onCell: () => ({
        style: { verticalAlign: "top" },
      }),
      render: (items: any) => transformData(items),
    },
    {
      dataIndex: "body",
      key: "body",
      title: "Body",
      onCell: () => ({
        style: { verticalAlign: "top" },
      }),
      render: (items: any) => transformData(items),
    },
    {
      dataIndex: "sentStatus",
      key: "sentStatus",
      title: "Status",
      onCell: () => ({
        style: { verticalAlign: "top" },
      }),
      render: (status: any) => {
        if (status == 0) return <Tag color="warning">Waiting</Tag>;
        if (status == 1) return <Tag color="success">Sent</Tag>;
        if (status == 2) return <Tag color="error">Failed</Tag>;
        return transformData(status);
      },
    },
    {
      dataIndex: "insertedAt",
      key: "insertedAt",
      title: "Time",
      onCell: () => ({
        style: { verticalAlign: "top" },
      }),
      render: (items: any) => (
        <span>{dayjs.utc(items).format(dateConfig)}</span>
      ),
    },
    {
      dataIndex: "inserteByFullName",
      key: "inserteByFullName",
      title: "User",
      onCell: () => ({
        style: { verticalAlign: "top" },
      }),
      render: (items: any) => transformData(items),
    },
    {
      dataIndex: "actionLogDocument",
      key: "actionLogDocument",
      title: "Action",
      onCell: () => ({
        style: { verticalAlign: "top" },
      }),
      render: (_: any, record: any) => (
        <Button
          icon={<SendOutlined />}
          onClick={() => handleResendSingle(record.id)}
          loading={loadingResend}
        >
          Resend
        </Button>
      ),
    },
  ];

  useEffect(() => {
    getData();
  }, [page]);

  const getData = () => {
    setLoading(true);
    let url = `/email/filter?Page=${page}&Limit=${pageLengthConfig}`;
    if (startDate) url += `&StartDate=${startDate}`;
    if (endDate) url += `&EndDate=${endDate}`;

    apiClient
      .get(url)
      .then(({ data }) => {
        console.log(data);
        let res = data?.data?.data || [];
        setData(res);
        setTotalRow(data?.data?.totalRecords || 0);
        setLoading(false);
      })
      .catch((err) => {
        console.log(err);
        setLoading(false);
      });
  };

  const handleClickFilter = () => {
    setPage(1);
    getData();
  };

  const handleTableChange: TableProps<any>["onChange"] = (pagination) => {
    setPage(Number(pagination.current));
  };

  const transformData = (data: any) => {
    if (!data) return null;

    try {
      const parsed = JSON.parse(data);

      // If parsed is an array, process it
      if (Array.isArray(parsed)) {
        return (
          <pre style={{ whiteSpace: "pre-wrap", margin: 0 }}>
            {parsed
              .map((item: any) =>
                Object.entries(item)
                  .map(([key, value]) => `${key}: ${value ?? "null"}`)
                  .join("\n")
              )
              .join("\n")}
          </pre>
        );
      }

      // If parsed is an object, display it
      if (typeof parsed === "object" && parsed !== null) {
        return (
          <pre style={{ whiteSpace: "pre-wrap", margin: 0 }}>
            {Object.entries(parsed)
              .map(([key, value]) => `${key}: ${value ?? "null"}`)
              .join("\n")}
          </pre>
        );
      }

      // If parsed is a primitive value (string, number, boolean), display it directly
      return <span>{String(parsed)}</span>;
    } catch (error) {
      // If JSON.parse fails, it's probably a plain string
      return <span>{data}</span>;
    }
  };

  const handleClickDownload = async () => {
    setLoadingDownload(true);
    let url = `/email/filter?Page=${page}&Limit=${pageLengthConfig}`;
    if (startDate) url += `&StartDate=${startDate}`;
    if (endDate) url += `&EndDate=${endDate}`;
    apiClient
      .get(url)
      .then(({ data }) => {
        let dd = data?.data?.data || [];
        // Create a new workbook and worksheet
        const worksheet = XLSX.utils.json_to_sheet(transformDataExport(dd));
        const workbook = XLSX.utils.book_new();
        XLSX.utils.book_append_sheet(workbook, worksheet, "Sheet1");

        // Create a binary Excel file
        const excelBuffer = XLSX.write(workbook, {
          bookType: "xlsx",
          type: "array",
        });

        // Use FileSaver to save the file
        const blob = new Blob([excelBuffer], {
          type: "application/octet-stream",
        });
        saveAs(blob, `log-audit.xlsx`);
        setLoadingDownload(false);
      })
      .catch((err) => {
        console.log(err);
        setLoadingDownload(false);
      });
  };

  const transformDataExport = (data: any) => {
    return data.map((entry: any) => ({
      ...entry,
      before: entry.before
        ? JSON.parse(entry.before)
          .map((item: any) =>
            Object.entries(item)
              .map(([key, value]) => `${key}: ${value ?? "null"}`)
              .join("\n")
          )
          .join("\n")
        : null,
      after: entry.after
        ? JSON.parse(entry.after)
          .map((item: any) =>
            Object.entries(item)
              .map(([key, value]) => `${key}: ${value ?? "null"}`)
              .join("\n")
          )
          .join("\n")
        : null,
    }));
  };

  return (
    <div className="pt-0">
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4 py-3 mb-3">
        <div className="flex items-end gap-3">
          <div className="flex flex-col gap-1">
            <span>From</span>
            <DatePicker
              format="YYYY-MM-DD"
              placeholder="YYYY-MM-DD"
              value={startDate ? dayjs.utc(startDate, "YYYY-MM-DD") : null}
              onChange={(val) =>
                setStartDate(val ? dayjs.utc(val).format("YYYY-MM-DD") : null)
              }
            />
          </div>
          <div className="flex flex-col gap-1">
            <span>To</span>
            <DatePicker
              format="YYYY-MM-DD"
              placeholder="YYYY-MM-DD"
              value={endDate ? dayjs.utc(endDate, "YYYY-MM-DD") : null}
              onChange={(val) =>
                setEndDate(val ? dayjs.utc(val).format("YYYY-MM-DD") : null)
              }
            />
          </div>
          <Button
            icon={<SearchOutlined />}
            type="primary"
            iconPosition="start"
            onClick={handleClickFilter}
            loading={loading}
          >
            Filter
          </Button>
        </div>
        <div className="flex gap-2">
          {selectedRowKeys.length > 0 && (
            <Button
              icon={<SendOutlined />}
              type="primary"
              onClick={handleResendMultiple}
              loading={loadingResend}
            >
              Resend Selected ({selectedRowKeys.length})
            </Button>
          )}
          <Button
            icon={<DownloadOutlined />}
            iconPosition="end"
            onClick={handleClickDownload}
            loading={loadingDownload}
          >
            Download
          </Button>
        </div>
      </div>
      <Card styles={{ body: { padding: 0, height: 650 } }}>
        <Table
          rowKey="id"
          rowSelection={rowSelection}
          onChange={handleTableChange}
          scroll={{ y: 510 }}
          pagination={{
            position: ["bottomCenter"],
            itemRender: itemRender,
            current: page,
            pageSize: pageLengthDocument,
            total: totalRow,
            showSizeChanger: false,
          }}
          columns={columns}
          dataSource={data}
          loading={loading}
        />
      </Card>
    </div>
  );
}
