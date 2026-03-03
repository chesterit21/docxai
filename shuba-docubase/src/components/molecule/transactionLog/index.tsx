import {
  Card,
  Table,
  type PaginationProps,
  Button,
  type TableProps,
  DatePicker,
  Modal,
  Tabs,
  Empty,
} from "antd";
import {
  ArrowLeftOutlined,
  ArrowRightOutlined,
  DownloadOutlined,
  SearchOutlined,
  FileTextOutlined,
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
  const [totalRow, setTotalRow] = useState<number>(0);
  const [loading, setLoading] = useState(false);
  const [loadingDownload, setLoadingDownload] = useState(false);
  const [startDate, setStartDate] = useState<string | null>(() =>
    dayjs.utc().subtract(1, "day").format("YYYY-MM-DD")
  );
  const [endDate, setEndDate] = useState<string | null>(() =>
    dayjs.utc().format("YYYY-MM-DD")
  );
  const [auditModalOpen, setAuditModalOpen] = useState(false);
  const [selectedAudit, setSelectedAudit] = useState<any>(null);

  const columns = [
    {
      dataIndex: "insertedAt",
      key: "insertedAt",
      title: "Time",
      onCell: () => ({
        style: { verticalAlign: "top" },
      }),
      render: (items: any) => <span>{dayjs.utc(items).format(dateConfig)}</span>,
    },
    {
      key: "insertedByFullName",
      dataIndex: "insertedByFullName",
      title: "User",
      onCell: () => ({
        style: { verticalAlign: "top" },
      }),
    },
    {
      dataIndex: "ipAddress",
      key: "ipAddress",
      title: "IP Address",
      onCell: () => ({
        style: { verticalAlign: "top" },
      }),
    },
    {
      dataIndex: "userAgent",
      key: "userAgent",
      title: "User Agent",
      onCell: () => ({
        style: { verticalAlign: "top" },
      }),
    },
    // {
    //     dataIndex: 'action',
    //     key: 'action',
    //     title: 'Action',
    //     onCell: () => ({
    //         style: { verticalAlign: "top" }
    //     })
    // },
    {
      dataIndex: "description",
      key: "description",
      title: "Description",
      onCell: () => ({
        style: { verticalAlign: "top" },
      }),
    },
    {
      dataIndex: "path",
      key: "path",
      title: "Path",
      onCell: () => ({
        style: { verticalAlign: "top" },
      }),
    },
    {
      dataIndex: "parameter",
      key: "parameter",
      title: "Parameter",
      onCell: () => ({
        style: { verticalAlign: "top" },
      }),
    },
    {
      key: "audit",
      title: "Audit",
      width: 100,
      align: "center" as const,
      render: (_: unknown, record: any) =>
        record.audit ? (
          <Button
            type="primary"
            size="small"
            icon={<FileTextOutlined />}
            onClick={() => {
              setSelectedAudit(record.audit);
              setAuditModalOpen(true);
            }}
          >
            View
          </Button>
        ) : (
          <span style={{ color: "#ccc" }}>-</span>
        ),
    },
  ];

  useEffect(() => {
    getData();
  }, [page]);

  const getData = () => {
    setLoading(true);
    let url = `/transactionlog/filter?Page=${page}&Limit=${pageLengthConfig}`;
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

    const parsed = JSON.parse(data);

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
  };

  const handleClickDownload = async () => {
    setLoadingDownload(true);
    let url = `/transactionlog/filter?Page=${page}&Limit=${pageLengthConfig}`;
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
      <div className="flex flex-col justify-between gap-4 py-3 mb-3 md:flex-row md:items-center">
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
        <div>
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
          onChange={handleTableChange}
          scroll={{ y: 510 }}
          pagination={{
            position: ["bottomCenter"],
            itemRender: itemRender,
            current: page,
            pageSize: pageLengthConfig,
            total: totalRow,
            showSizeChanger: false,
          }}
          columns={columns}
          dataSource={data}
          loading={loading}
        />
      </Card>

      <Modal
        title="Audit Details"
        open={auditModalOpen}
        onCancel={() => {
          setAuditModalOpen(false);
          setSelectedAudit(null);
        }}
        footer={[
          <Button
            key="close"
            onClick={() => {
              setAuditModalOpen(false);
              setSelectedAudit(null);
            }}
          >
            Close
          </Button>,
        ]}
        width={900}
        bodyStyle={{ maxHeight: "600px", overflowY: "auto" }}
      >
        {selectedAudit ? (
          <Tabs
            items={[
              {
                key: "1",
                label: "Table Name",
                children: (
                  <div>
                    <p>
                      <strong>Table Name:</strong>{" "}
                      {selectedAudit.tableName || "-"}
                    </p>
                  </div>
                ),
              },
              {
                key: "2",
                label: "Column Name",
                children: (
                  <div>
                    <p>
                      <strong>Column Name:</strong>{" "}
                      {selectedAudit.columnName || "-"}
                    </p>
                  </div>
                ),
              },
              {
                key: "3",
                label: "Before",
                children: (
                  <div>
                    {selectedAudit.before ? (
                      <pre
                        style={{
                          whiteSpace: "pre-wrap",
                          backgroundColor: "#f5f5f5",
                          padding: "12px",
                          borderRadius: "4px",
                          fontSize: "12px",
                        }}
                      >
                        {JSON.stringify(
                          JSON.parse(selectedAudit.before),
                          null,
                          2
                        )}
                      </pre>
                    ) : (
                      <Empty description="No data" />
                    )}
                  </div>
                ),
              },
              {
                key: "4",
                label: "After",
                children: (
                  <div>
                    {selectedAudit.after ? (
                      <pre
                        style={{
                          whiteSpace: "pre-wrap",
                          backgroundColor: "#f5f5f5",
                          padding: "12px",
                          borderRadius: "4px",
                          fontSize: "12px",
                        }}
                      >
                        {JSON.stringify(
                          JSON.parse(selectedAudit.after),
                          null,
                          2
                        )}
                      </pre>
                    ) : (
                      <Empty description="No data" />
                    )}
                  </div>
                ),
              },
            ]}
          />
        ) : (
          <Empty description="No audit data" />
        )}
      </Modal>
    </div>
  );
}
