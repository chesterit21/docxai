import { useState, useEffect } from "react";
import {
  Card,
  Col,
  Row,
  Statistic,
  Spin,
  Select,
  List,
  Typography,
  Space,
  DatePicker,
  Empty,
  Alert,
  Pagination,
} from "antd";
import {
  FileOutlined,
  ClockCircleOutlined,
  CloseCircleOutlined,
  CheckCircleOutlined,
  HourglassOutlined,
  ExclamationCircleOutlined,
  DatabaseOutlined,
  UserOutlined,
} from "@ant-design/icons";
import {
  LineChart,
  Line,
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer,
  Cell,
} from "recharts";
import dayjs, { Dayjs } from "dayjs";
import apiClient from "../services/apiClient";
import { useAuth } from "../context/AuthContext";

const { RangePicker } = DatePicker;
const { Text } = Typography;

interface DashboardData {
  totalDocuments: number;
  expiringDocuments: number;
  expiredDocuments: number;
  pendingApprovalDocuments: number;
  rejectedDocuments: number;
  approvedDocuments: number;
  totalSizeDocuments: string;
  pendingMyApproval: number;
}

interface RecentActivity {
  id: string;
  documentID: number;
  documentTitle: string;
  activity: string;
  activityDate: string;
  activityBy: string;
}

interface StorageUser {
  userID: number;
  userName: string;
  totalSize: string;
}

interface UserOption {
  photo?: string | null;
  fullName: string;
  email: string;
  userId: number;
}

interface DocumentGrowthData {
  monthNumber: number;
  monthName: string;
  totalDocuments: number;
}

interface CategoryDocumentData {
  categoryId: number;
  categoryName: string;
  documentCount: number;
}

interface StatusDocumentData {
  status: number;
  statusName: string;
  documentCount: number;
}

interface ExpiringDocumentData {
  groupKey: number;
  groupLabel: string;
  expiringDocumentCount: number;
}

interface ActiveUserData {
  userId: number;
  userName: string;
  loginCount: number;
}

interface ViewedDocumentData {
  documentId: number;
  documentTitle: string;
  viewCount: number;
}

interface DownloadedDocumentData {
  documentId: number;
  documentTitle: string;
  categoryName: string;
  downloadCount: number;
}

const Dashboard = () => {
  const { user, config, isConnectionStringConfigured } = useAuth();
  const isSuperAdmin = user?.user_type?.toLowerCase() === "superadmin";
  const isSuperAdminOrAdmin = ["superadmin", "admin"].includes(
    user?.user_type?.toLowerCase() || ""
  );
  const dateConfig =
    config?.dataGrid?.dateTimeFormat || "YYYY-MM-DD | HH:mm:ss";
  const [dashboardData, setDashboardData] = useState<DashboardData | null>(
    null
  );
  const [loading, setLoading] = useState<boolean>(true);
  const [userOptions, setUserOptions] = useState<UserOption[]>([]);
  const [usersLoading, setUsersLoading] = useState<boolean>(false);
  const [selectedUserId, setSelectedUserId] = useState<number | undefined>(
    undefined
  );
  // Date ranges for each chart (default: last 1 month)
  const [recentActivityDateRange, setRecentActivityDateRange] = useState<
    [Dayjs | null, Dayjs | null]
  >([dayjs().subtract(1, "month"), dayjs()]);
  const [categoryDateRange, setCategoryDateRange] = useState<
    [Dayjs | null, Dayjs | null]
  >([dayjs().subtract(1, "month"), dayjs()]);
  const [statusDateRange, setStatusDateRange] = useState<
    [Dayjs | null, Dayjs | null]
  >([dayjs().subtract(1, "month"), dayjs()]);
  const [activeUsersDateRange, setActiveUsersDateRange] = useState<
    [Dayjs | null, Dayjs | null]
  >([dayjs().subtract(1, "month"), dayjs()]);
  const [viewedDocsDateRange, setViewedDocsDateRange] = useState<
    [Dayjs | null, Dayjs | null]
  >([dayjs().subtract(1, "month"), dayjs()]);
  const [downloadedDocsDateRange, setDownloadedDocsDateRange] = useState<
    [Dayjs | null, Dayjs | null]
  >([dayjs().subtract(1, "month"), dayjs()]);
  const [storageUsersDateRange, _setStorageUsersDateRange] = useState<
    [Dayjs | null, Dayjs | null]
  >([dayjs().subtract(1, "month"), dayjs()]);
  const [recentActivities, setRecentActivities] = useState<RecentActivity[]>(
    []
  );
  const [recentLoading, setRecentLoading] = useState<boolean>(false);
  const [recentActivityPage, setRecentActivityPage] = useState<number>(1);
  const [storageUsers, setStorageUsers] = useState<StorageUser[]>([]);
  const [storageLoading, setStorageLoading] = useState<boolean>(false);
  const [selectedYear, setSelectedYear] = useState<number>(dayjs().year());
  const [documentGrowthData, setDocumentGrowthData] = useState<
    DocumentGrowthData[]
  >([]);
  const [growthLoading, setGrowthLoading] = useState<boolean>(false);
  const [categoryData, setCategoryData] = useState<CategoryDocumentData[]>([]);
  const [categoryLoading, setCategoryLoading] = useState<boolean>(false);
  const [statusData, setStatusData] = useState<StatusDocumentData[]>([]);
  const [statusLoading, setStatusLoading] = useState<boolean>(false);
  const [expiringData, setExpiringData] = useState<ExpiringDocumentData[]>([]);
  const [expiringLoading, setExpiringLoading] = useState<boolean>(false);
  const [expiringPeriod, setExpiringPeriod] = useState<number>(0);
  /* eslint-disable @typescript-eslint/no-unused-vars */
  const [expiringDateRange, _setExpiringDateRange] = useState<
    [Dayjs | null, Dayjs | null]
  >([null, null]);
  const [activeUsersData, setActiveUsersData] = useState<ActiveUserData[]>([]);
  const [activeUsersLoading, setActiveUsersLoading] = useState<boolean>(false);
  const [viewedDocsData, setViewedDocsData] = useState<ViewedDocumentData[]>(
    []
  );
  const [viewedDocsLoading, setViewedDocsLoading] = useState<boolean>(false);
  const [downloadedDocsData, setDownloadedDocsData] = useState<
    DownloadedDocumentData[]
  >([]);
  const [downloadedDocsLoading, setDownloadedDocsLoading] =
    useState<boolean>(false);

  useEffect(() => {
    getDashboardData(selectedUserId);
  }, [selectedUserId]);

  useEffect(() => {
    if (isSuperAdmin) {
      getUserOptions();
    }
  }, [isSuperAdmin]);

  useEffect(() => {
    getRecentActivities(
      selectedUserId,
      recentActivityDateRange[0],
      recentActivityDateRange[1]
    );
  }, [selectedUserId, recentActivityDateRange]);

  useEffect(() => {
    getDocumentGrowthData(selectedYear, selectedUserId);
  }, [selectedYear, selectedUserId]);

  useEffect(() => {
    getCategoryDocumentData(
      selectedUserId,
      categoryDateRange[0],
      categoryDateRange[1]
    );
  }, [selectedUserId, categoryDateRange]);

  useEffect(() => {
    getStatusDocumentData(
      selectedUserId,
      statusDateRange[0],
      statusDateRange[1]
    );
  }, [selectedUserId, statusDateRange]);

  useEffect(() => {
    getExpiringDocumentsData(
      selectedUserId,
      expiringPeriod,
      expiringDateRange[0],
      expiringDateRange[1]
    );
  }, [selectedUserId, expiringPeriod, expiringDateRange]);

  useEffect(() => {
    getTopActiveUsers(activeUsersDateRange[0], activeUsersDateRange[1]);
  }, [activeUsersDateRange]);

  useEffect(() => {
    getMostViewedDocuments(viewedDocsDateRange[0], viewedDocsDateRange[1]);
  }, [viewedDocsDateRange]);

  useEffect(() => {
    getMostDownloadedDocuments(
      downloadedDocsDateRange[0],
      downloadedDocsDateRange[1]
    );
  }, [downloadedDocsDateRange]);

  useEffect(() => {
    getMostStorageUsers(storageUsersDateRange[0], storageUsersDateRange[1]);
  }, [storageUsersDateRange]);

  const getDashboardData = (userId?: number) => {
    setLoading(true);
    const query = userId ? `?userId=${userId}` : "";
    apiClient
      .get(`/dashboard/get-dashboard-counter${query}`)
      .then(({ data }) => {
        setDashboardData(data?.data || null);
        setLoading(false);
      })
      .catch((err) => {
        console.log(err);
        setLoading(false);
      });
  };

  const getRecentActivities = (
    userId?: number,
    rangeStart?: Dayjs | null,
    rangeEnd?: Dayjs | null
  ) => {
    setRecentLoading(true);
    const params = new URLSearchParams();
    params.set("DocumentID", "");
    params.set("UserID", userId ? String(userId) : "");
    params.set("StartDate", rangeStart ? rangeStart.format("YYYY-MM-DD") : "");
    params.set("EndDate", rangeEnd ? rangeEnd.format("YYYY-MM-DD") : "");

    apiClient
      .get(`/dashboard/get-recent-activity?${params.toString()}`)
      .then(({ data }) => {
        setRecentActivities(Array.isArray(data?.data) ? data.data : []);
        setRecentLoading(false);
      })
      .catch((err) => {
        console.log(err);
        setRecentActivities([]);
        setRecentLoading(false);
      });
  };

  const getMostStorageUsers = (
    rangeStart?: Dayjs | null,
    rangeEnd?: Dayjs | null
  ) => {
    setStorageLoading(true);
    const params = new URLSearchParams();
    params.set("StartDate", rangeStart ? rangeStart.format("YYYY-MM-DD") : "");
    params.set("EndDate", rangeEnd ? rangeEnd.format("YYYY-MM-DD") : "");

    apiClient
      .get(`/dashboard/get-most-storage-user?${params.toString()}`)
      .then(({ data }) => {
        setStorageUsers(Array.isArray(data?.data) ? data.data : []);
        setStorageLoading(false);
      })
      .catch((err) => {
        console.log(err);
        setStorageUsers([]);
        setStorageLoading(false);
      });
  };

  const getDocumentGrowthData = (year: number, userId?: number) => {
    setGrowthLoading(true);
    const params = new URLSearchParams();
    params.set("year", String(year));
    params.set("userId", userId ? String(userId) : "");

    apiClient
      .get(`/dashboard/document-growth-over-time?${params.toString()}`)
      .then(({ data }) => {
        setDocumentGrowthData(Array.isArray(data?.data) ? data.data : []);
        setGrowthLoading(false);
      })
      .catch((err) => {
        console.log(err);
        setDocumentGrowthData([]);
        setGrowthLoading(false);
      });
  };

  const getCategoryDocumentData = (
    userId?: number,
    rangeStart?: Dayjs | null,
    rangeEnd?: Dayjs | null
  ) => {
    setCategoryLoading(true);
    const params = new URLSearchParams();
    params.set("userId", userId ? String(userId) : "");
    params.set("StartDate", rangeStart ? rangeStart.format("YYYY-MM-DD") : "");
    params.set("EndDate", rangeEnd ? rangeEnd.format("YYYY-MM-DD") : "");

    apiClient
      .get(`/dashboard/document-count-by-category?${params.toString()}`)
      .then(({ data }) => {
        const categoryList = Array.isArray(data?.data) ? data.data : [];
        // Get top 10 categories
        const top10 = categoryList
          .sort(
            (a: CategoryDocumentData, b: CategoryDocumentData) =>
              b.documentCount - a.documentCount
          )
          .slice(0, 10);
        setCategoryData(top10);
        setCategoryLoading(false);
      })
      .catch((err) => {
        console.log(err);
        setCategoryData([]);
        setCategoryLoading(false);
      });
  };

  const getStatusDocumentData = (
    userId?: number,
    rangeStart?: Dayjs | null,
    rangeEnd?: Dayjs | null
  ) => {
    setStatusLoading(true);
    const params = new URLSearchParams();
    params.set("UserId", userId ? String(userId) : "");
    params.set("StartDate", rangeStart ? rangeStart.format("YYYY-MM-DD") : "");
    params.set("EndDate", rangeEnd ? rangeEnd.format("YYYY-MM-DD") : "");

    apiClient
      .get(`/dashboard/document-count-approval-status?${params.toString()}`)
      .then(({ data }) => {
        const mappedData = Array.isArray(data?.data)
          ? data.data.map((item: StatusDocumentData) => ({
            ...item,
            statusName:
              item.statusName === "Pending"
                ? "Pending Task"
                : item.statusName,
          }))
          : [];
        setStatusData(mappedData);
        setStatusLoading(false);
      })
      .catch((err) => {
        console.log(err);
        setStatusData([]);
        setStatusLoading(false);
      });
  };

  const getExpiringDocumentsData = (
    userId?: number,
    period: number = 0,
    rangeStart?: Dayjs | null,
    rangeEnd?: Dayjs | null
  ) => {
    setExpiringLoading(true);
    const params = new URLSearchParams();
    params.set("UserId", userId ? String(userId) : "");
    params.set("Period", String(period));
    params.set("StartDate", rangeStart ? rangeStart.format("YYYY-MM-DD") : "");
    params.set("EndDate", rangeEnd ? rangeEnd.format("YYYY-MM-DD") : "");

    apiClient
      .get(`/dashboard/document-expiring-by-period?${params.toString()}`)
      .then(({ data }) => {
        setExpiringData(Array.isArray(data?.data) ? data.data : []);
        setExpiringLoading(false);
      })
      .catch((err) => {
        console.log(err);
        setExpiringData([]);
        setExpiringLoading(false);
      });
  };

  const getTopActiveUsers = (
    rangeStart?: Dayjs | null,
    rangeEnd?: Dayjs | null
  ) => {
    setActiveUsersLoading(true);
    const params = new URLSearchParams();
    params.set("StartDate", rangeStart ? rangeStart.format("YYYY-MM-DD") : "");
    params.set("EndDate", rangeEnd ? rangeEnd.format("YYYY-MM-DD") : "");

    apiClient
      .get(`/dashboard/get-most-active-user?${params.toString()}`)
      .then(({ data }) => {
        setActiveUsersData(Array.isArray(data?.data) ? data.data : []);
        setActiveUsersLoading(false);
      })
      .catch((err) => {
        console.log(err);
        setActiveUsersData([]);
        setActiveUsersLoading(false);
      });
  };

  const getMostViewedDocuments = (
    rangeStart?: Dayjs | null,
    rangeEnd?: Dayjs | null
  ) => {
    setViewedDocsLoading(true);
    const params = new URLSearchParams();
    params.set("StartDate", rangeStart ? rangeStart.format("YYYY-MM-DD") : "");
    params.set("EndDate", rangeEnd ? rangeEnd.format("YYYY-MM-DD") : "");

    apiClient
      .get(`/dashboard/get-most-download-document?${params.toString()}`)
      .then(({ data }) => {
        // data.data assumed as array of {documentId, documentTitle, categoryName, viewCount}
        setViewedDocsData(Array.isArray(data?.data) ? data.data : []);
        setViewedDocsLoading(false);
      })
      .catch((err) => {
        console.log(err);
        setViewedDocsData([]);
        setViewedDocsLoading(false);
      });
  };

  const getMostDownloadedDocuments = (
    rangeStart?: Dayjs | null,
    rangeEnd?: Dayjs | null
  ) => {
    setDownloadedDocsLoading(true);
    const params = new URLSearchParams();
    params.set("StartDate", rangeStart ? rangeStart.format("YYYY-MM-DD") : "");
    params.set("EndDate", rangeEnd ? rangeEnd.format("YYYY-MM-DD") : "");

    apiClient
      .get(`/dashboard/get-most-download-document?${params.toString()}`)
      .then(({ data }) => {
        setDownloadedDocsData(Array.isArray(data?.data) ? data.data : []);
        setDownloadedDocsLoading(false);
      })
      .catch((err) => {
        console.log(err);
        setDownloadedDocsData([]);
        setDownloadedDocsLoading(false);
      });
  };

  const getUserOptions = () => {
    setUsersLoading(true);
    apiClient
      .get("/dropdown/ddlusers")
      .then(({ data }) => {
        setUserOptions(Array.isArray(data?.data) ? data.data : []);
        setUsersLoading(false);
      })
      .catch((err) => {
        console.log(err);
        setUserOptions([]);
        setUsersLoading(false);
      });
  };

  const handleUserChange = (value: number | null) => {
    setSelectedUserId(value ?? undefined);
  };

  const handleRecentActivityDateChange = (
    values: [Dayjs | null, Dayjs | null] | null
  ) => {
    setRecentActivityDateRange(values || [null, null]);
  };

  const handleCategoryDateChange = (
    values: [Dayjs | null, Dayjs | null] | null
  ) => {
    setCategoryDateRange(values || [null, null]);
  };

  const handleStatusDateChange = (
    values: [Dayjs | null, Dayjs | null] | null
  ) => {
    setStatusDateRange(values || [null, null]);
  };

  const handleActiveUsersDateChange = (
    values: [Dayjs | null, Dayjs | null] | null
  ) => {
    setActiveUsersDateRange(values || [null, null]);
  };

  const handleViewedDocsDateChange = (
    values: [Dayjs | null, Dayjs | null] | null
  ) => {
    setViewedDocsDateRange(values || [null, null]);
  };

  const handleDownloadedDocsDateChange = (
    values: [Dayjs | null, Dayjs | null] | null
  ) => {
    setDownloadedDocsDateRange(values || [null, null]);
  };



  const handleYearChange = (value: number) => {
    setSelectedYear(value);
  };

  const handleExpiringPeriodChange = (value: number) => {
    setExpiringPeriod(value);
  };



  if (loading) {
    return (
      <div
        style={{
          display: "flex",
          justifyContent: "center",
          alignItems: "center",
          minHeight: "400px",
        }}
      >
        <Spin size="large" />
      </div>
    );
  }

  return (
    <>
      {!isConnectionStringConfigured && (
        <Alert
          message="Peringatan: Connection String Belum Dikonfigurasi"
          description="Connection String belum dikonfigurasi! Silakan setup connection string di Settings terlebih dahulu agar aplikasi dapat berfungsi dengan normal."
          type="warning"
          showIcon
          closable
          style={{ marginBottom: 16 }}
        />
      )}
      {isSuperAdmin && (
        <Row style={{ marginBottom: 16 }}>
          <label htmlFor="user-select" className="text-sm font-medium mb-1">
            Filter by User
          </label>
          <Col span={24}>
            <Select
              allowClear
              placeholder="Select user"
              style={{ minWidth: 240 }}
              value={selectedUserId}
              onChange={handleUserChange}
              options={userOptions.map((option) => ({
                value: option.userId,
                label: option.fullName || option.email,
              }))}
              loading={usersLoading}
            />
          </Col>
        </Row>
      )}
      <Row gutter={[16, 16]}>
        <Col xs={24} sm={12} md={8} lg={6}>
          <Card variant="borderless">
            <Statistic
              title="Total Documents"
              value={dashboardData?.totalDocuments || 0}
              valueStyle={{ color: "#1890ff" }}
              prefix={<FileOutlined />}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} md={8} lg={6}>
          <Card variant="borderless">
            <Statistic
              title="Total Size"
              value={dashboardData?.totalSizeDocuments || "0.00 bytes"}
              valueStyle={{ color: "#1890ff" }}
              prefix={<DatabaseOutlined />}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} md={8} lg={6}>
          <Card variant="borderless">
            <Statistic
              title="Expiring Documents"
              value={dashboardData?.expiringDocuments || 0}
              valueStyle={{ color: "#faad14" }}
              prefix={<ClockCircleOutlined />}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} md={8} lg={6}>
          <Card variant="borderless">
            <Statistic
              title="Expired Documents"
              value={dashboardData?.expiredDocuments || 0}
              valueStyle={{ color: "#ff4d4f" }}
              prefix={<ExclamationCircleOutlined />}
            />
          </Card>
        </Col>
      </Row>

      <Row gutter={[16, 16]} style={{ marginTop: 16 }}>
        <Col xs={24} sm={12} md={8} lg={6}>
          <Card variant="borderless">
            <Statistic
              title="Pending Approval"
              value={dashboardData?.pendingApprovalDocuments || 0}
              valueStyle={{ color: "#faad14" }}
              prefix={<HourglassOutlined />}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} md={8} lg={6}>
          <Card variant="borderless">
            <Statistic
              title="Approved Documents"
              value={dashboardData?.approvedDocuments || 0}
              valueStyle={{ color: "#52c41a" }}
              prefix={<CheckCircleOutlined />}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} md={8} lg={6}>
          <Card variant="borderless">
            <Statistic
              title="Rejected Documents"
              value={dashboardData?.rejectedDocuments || 0}
              valueStyle={{ color: "#ff4d4f" }}
              prefix={<CloseCircleOutlined />}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} md={8} lg={6}>
          <Card variant="borderless">
            <Statistic
              title="Pending My Approval"
              value={dashboardData?.pendingMyApproval || 0}
              valueStyle={{ color: "#722ed1" }}
              prefix={<UserOutlined />}
            />
          </Card>
        </Col>
      </Row>

      <Row gutter={[16, 16]} style={{ marginTop: 16 }}>
        <Col xs={24} lg={12}>
          <Card
            title="Most Viewed Documents"
            loading={viewedDocsLoading && viewedDocsData.length === 0}
            extra={
              <Space size="small" direction="vertical">
                <RangePicker
                  value={viewedDocsDateRange}
                  onChange={handleViewedDocsDateChange}
                  allowClear
                  format="YYYY-MM-DD"
                  size="small"
                />
              </Space>
            }
            styles={{
              header: {
                alignItems: "flex-start",
              },
            }}
          >
            {viewedDocsLoading ? (
              <div
                style={{
                  display: "flex",
                  justifyContent: "center",
                  alignItems: "center",
                  minHeight: "300px",
                }}
              >
                <Spin size="large" />
              </div>
            ) : viewedDocsData.length === 0 ? (
              <Empty description="No data available" />
            ) : (
              <ResponsiveContainer width="100%" height={400}>
                <BarChart
                  data={viewedDocsData}
                  layout="vertical"
                  margin={{ top: 5, right: 30, left: 4, bottom: 5 }}
                >
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis type="number" />
                  <YAxis
                    dataKey="documentTitle"
                    type="category"
                    tick={{ fontSize: 11 }}
                    width={140}
                  />
                  <Tooltip
                    contentStyle={{
                      backgroundColor: "#fff",
                      border: "1px solid #d9d9d9",
                      borderRadius: "4px",
                    }}
                  />
                  <Legend />
                  <Bar
                    dataKey="viewCount"
                    name="View Count"
                    radius={[0, 8, 8, 0]}
                    fill="#1890ff"
                  >
                    {viewedDocsData.map((entry, index) => {
                      // Gradient from blue to purple
                      const colors = [
                        "#1890ff",
                        "#2f54eb",
                        "#597ef7",
                        "#722ed1",
                        "#9254de",
                        "#b37feb",
                        "#d3adf7",
                        "#efdbff",
                        "#f9f0ff",
                        "#fafafa",
                      ];
                      return (
                        <Cell
                          key={`cell-${entry.documentId}`}
                          fill={colors[index % colors.length]}
                        />
                      );
                    })}
                  </Bar>
                </BarChart>
              </ResponsiveContainer>
            )}
          </Card>
        </Col>
        <Col xs={24} lg={12}>
          <Card
            title={
              <div
                style={{
                  padding: 8,
                  display: "flex",
                  flexDirection: "column",
                  width: "100%",
                }}
              >
                <div>Most Downloaded Documents</div>
                <div
                  style={{
                    display: "flex",
                    justifyContent: "flex-end",
                    width: "100%",
                  }}
                >
                  <RangePicker
                    value={downloadedDocsDateRange}
                    onChange={handleDownloadedDocsDateChange}
                    allowClear
                    format="YYYY-MM-DD"
                    size="small"
                  />
                </div>
              </div>
            }
            loading={downloadedDocsLoading && downloadedDocsData.length === 0}
            styles={{
              header: {
                alignItems: "flex-start",
              },
            }}
          >
            {downloadedDocsLoading ? (
              <div
                style={{
                  display: "flex",
                  justifyContent: "center",
                  alignItems: "center",
                  minHeight: "300px",
                }}
              >
                <Spin size="large" />
              </div>
            ) : downloadedDocsData.length === 0 ? (
              <Empty description="No data available" />
            ) : (
              <ResponsiveContainer width="100%" height={400}>
                <BarChart
                  data={downloadedDocsData}
                  layout="vertical"
                  margin={{ top: 5, right: 30, left: 4, bottom: 5 }}
                >
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis type="number" allowDecimals={false} />
                  <YAxis
                    dataKey="documentTitle"
                    type="category"
                    tick={{ fontSize: 11 }}
                    width={140}
                  />
                  <Tooltip
                    contentStyle={{
                      backgroundColor: "#fff",
                      border: "1px solid #d9d9d9",
                      borderRadius: "4px",
                    }}
                  />
                  <Legend />
                  <Bar
                    dataKey="downloadCount"
                    name="Download Count"
                    radius={[0, 8, 8, 0]}
                    fill="#52c41a"
                  >
                    {downloadedDocsData.map((entry, index) => {
                      // Gradient from green to light green
                      const colors = [
                        "#52c41a",
                        "#73d13d",
                        "#95de64",
                        "#b7eb8f",
                        "#d9f7be",
                        "#f6ffed",
                        "#eaff8f",
                        "#d3f261",
                        "#bae637",
                        "#a0d911",
                      ];
                      return (
                        <Cell
                          key={`cell-${entry.documentId}`}
                          fill={colors[index % colors.length]}
                        />
                      );
                    })}
                  </Bar>
                </BarChart>
              </ResponsiveContainer>
            )}
          </Card>
        </Col>
      </Row>

      <Row gutter={[16, 16]} style={{ marginTop: 16 }}>
        <Col xs={24}>
          <Card
            title="Recent Activity"
            loading={recentLoading && recentActivities.length === 0}
            extra={
              <Space size="small" direction="vertical">
                <RangePicker
                  value={recentActivityDateRange}
                  onChange={handleRecentActivityDateChange}
                  allowClear
                  format="YYYY-MM-DD"
                  size="small"
                />
              </Space>
            }
            styles={{
              header: {
                alignItems: "flex-start",
              },
            }}
          >
            <List
              dataSource={recentActivities.slice(
                (recentActivityPage - 1) * 5,
                recentActivityPage * 5
              )}
              loading={recentLoading}
              locale={{
                emptyText: recentLoading ? (
                  <Spin size="small" />
                ) : (
                  <Empty description="No recent activity" />
                ),
              }}
              renderItem={(item) => (
                <List.Item key={item.id}>
                  <List.Item.Meta
                    title={
                      <Space direction="vertical" size={0}>
                        <Text strong>{item.documentTitle || "Untitled"}</Text>
                        <Text type="secondary">
                          Document #{item.documentID}
                        </Text>
                      </Space>
                    }
                    description={
                      <Space direction="vertical" size={0}>
                        <Text>{item.activity}</Text>
                        <Text type="secondary">By {item.activityBy}</Text>
                      </Space>
                    }
                  />
                  <Text type="secondary">
                    {item.activityDate
                      ? dayjs.utc(item.activityDate).format(dateConfig)
                      : "-"}
                  </Text>
                </List.Item>
              )}
            />
            {recentActivities.length > 5 && (
              <div style={{ marginTop: 16, textAlign: "center" }}>
                <Pagination
                  current={recentActivityPage}
                  total={recentActivities.length}
                  pageSize={5}
                  onChange={(page) => setRecentActivityPage(page)}
                  showSizeChanger={false}
                  size="small"
                />
              </div>
            )}
          </Card>
        </Col>
      </Row>

      <Row gutter={[16, 16]} style={{ marginTop: 16 }}>
        {isSuperAdminOrAdmin && (
          <Col xs={24} lg={12}>
            <Card
              title="Top 10 Active Users"
              loading={activeUsersLoading && activeUsersData.length === 0}
              extra={
                <Space size="small" direction="vertical">
                  <RangePicker
                    value={activeUsersDateRange}
                    onChange={handleActiveUsersDateChange}
                    allowClear
                    format="YYYY-MM-DD"
                    size="small"
                  />
                </Space>
              }
              styles={{
                header: {
                  alignItems: "flex-start",
                },
              }}
            >
              {activeUsersLoading ? (
                <div
                  style={{
                    display: "flex",
                    justifyContent: "center",
                    alignItems: "center",
                    minHeight: "300px",
                  }}
                >
                  <Spin size="large" />
                </div>
              ) : activeUsersData.length === 0 ? (
                <Empty description="No data available" />
              ) : (
                <ResponsiveContainer width="100%" height={400}>
                  <BarChart
                    data={activeUsersData}
                    layout="vertical"
                    margin={{ top: 5, right: 30, left: 4, bottom: 5 }}
                  >
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis type="number" />
                    <YAxis
                      dataKey="userName"
                      type="category"
                      tick={{ fontSize: 11 }}
                      width={100}
                    />
                    <Tooltip
                      contentStyle={{
                        backgroundColor: "#fff",
                        border: "1px solid #d9d9d9",
                        borderRadius: "4px",
                      }}
                    />
                    <Legend />
                    <Bar
                      dataKey="loginCount"
                      name="Login Count"
                      radius={[0, 8, 8, 0]}
                      fill="#1890ff"
                    >
                      {activeUsersData.map((entry, index) => {
                        // Gradient from blue to purple
                        const colors = [
                          "#1890ff",
                          "#2f54eb",
                          "#597ef7",
                          "#722ed1",
                          "#9254de",
                          "#b37feb",
                          "#d3adf7",
                          "#efdbff",
                          "#f9f0ff",
                          "#fafafa",
                        ];
                        return (
                          <Cell
                            key={`cell-${entry.userId}`}
                            fill={colors[index % colors.length]}
                          />
                        );
                      })}
                    </Bar>
                  </BarChart>
                </ResponsiveContainer>
              )}
            </Card>
          </Col>
        )}
        <Col xs={24} lg={isSuperAdminOrAdmin ? 12 : 24}>
          <Card
            title="Top 10 User With Most Storage"
            loading={storageLoading && storageUsers.length === 0}
            extra={
              <Space size={8} direction="vertical">
                <Text type="secondary" style={{ fontSize: 12 }}>
                  Same date range
                </Text>
                <Text strong>
                  {storageUsersDateRange[0]
                    ? storageUsersDateRange[0].format("DD MMM")
                    : "Any"}{" "}
                  -{" "}
                  {storageUsersDateRange[1]
                    ? storageUsersDateRange[1].format("DD MMM YYYY")
                    : "Today"}
                </Text>
              </Space>
            }
          >
            <List
              dataSource={storageUsers}
              loading={storageLoading}
              locale={{
                emptyText: storageLoading ? (
                  <Spin size="small" />
                ) : (
                  <Empty description="No data" />
                ),
              }}
              renderItem={(item, index) => (
                <List.Item key={item.userID}>
                  <Space align="start">
                    <Text strong>{index + 1}.</Text>
                    <Space direction="vertical" size={0}>
                      <Text strong>{item.userName}</Text>
                      <Text type="secondary">User ID: {item.userID}</Text>
                    </Space>
                  </Space>
                  <Text strong>{item.totalSize || "0 KB"}</Text>
                </List.Item>
              )}
            />
          </Card>
        </Col>
      </Row>

      <Row gutter={[16, 16]} style={{ marginTop: 16 }}>
        <Col xs={24} lg={12}>
          <Card
            title="Expiring Documents"
            loading={expiringLoading && expiringData.length === 0}
            extra={
              <Space size="small">
                <Select
                  value={expiringPeriod}
                  onChange={handleExpiringPeriodChange}
                  style={{ width: 120 }}
                  options={[
                    { value: 0, label: "This Year" },
                    { value: 1, label: "Last Year" },
                    { value: 2, label: "Next Year" },
                  ]}
                />
              </Space>
            }
          >
            {expiringLoading ? (
              <div
                style={{
                  display: "flex",
                  justifyContent: "center",
                  alignItems: "center",
                  minHeight: "300px",
                }}
              >
                <Spin size="large" />
              </div>
            ) : expiringData.length === 0 ? (
              <Empty description="No data available" />
            ) : (
              <ResponsiveContainer width="100%" height={400}>
                <LineChart
                  data={expiringData}
                  margin={{ top: 5, right: 30, left: 20, bottom: 5 }}
                >
                  <CartesianGrid strokeDasharray="3 3" vertical={true} />
                  <XAxis
                    dataKey="groupLabel"
                    tick={{ fontSize: 12 }}
                    interval={0}
                    angle={-45}
                    textAnchor="end"
                    height={70}
                  />
                  <YAxis
                    allowDecimals={false}
                    label={{
                      value: "Expiring Documents",
                      angle: -90,
                      position: "insideLeft",
                    }}
                  />
                  <Tooltip
                    contentStyle={{
                      backgroundColor: "#fff",
                      border: "1px solid #d9d9d9",
                      borderRadius: "4px",
                    }}
                  />
                  <Legend />
                  <Line
                    type="monotone"
                    dataKey="expiringDocumentCount"
                    stroke="#faad14"
                    strokeWidth={2}
                    dot={{ fill: "#faad14", r: 4 }}
                    activeDot={{ r: 6 }}
                    name="Expiring Documents"
                  />
                </LineChart>
              </ResponsiveContainer>
            )}
          </Card>
        </Col>
        <Col xs={24} lg={12}>
          <Card
            title="Document By Status"
            loading={statusLoading && statusData.length === 0}
            extra={
              <Space size="small" direction="vertical">
                <RangePicker
                  value={statusDateRange}
                  onChange={handleStatusDateChange}
                  allowClear
                  format="YYYY-MM-DD"
                  size="small"
                />
              </Space>
            }
            styles={{
              header: {
                alignItems: "flex-start",
              },
            }}
          >
            {statusLoading ? (
              <div
                style={{
                  display: "flex",
                  justifyContent: "center",
                  alignItems: "center",
                  minHeight: "300px",
                }}
              >
                <Spin size="large" />
              </div>
            ) : statusData.length === 0 ? (
              <Empty description="No data available" />
            ) : (
              <ResponsiveContainer width="100%" height={400}>
                <BarChart
                  data={statusData}
                  margin={{ top: 5, right: 30, left: 20, bottom: 10 }}
                >
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis
                    dataKey="statusName"
                    tick={{ fontSize: 12 }}
                    angle={-45}
                    textAnchor="end"
                    height={80}
                  />
                  <YAxis
                    label={{
                      value: "Document Count",
                      angle: -90,
                      position: "insideLeft",
                    }}
                  />
                  <Tooltip
                    contentStyle={{
                      backgroundColor: "#fff",
                      border: "1px solid #d9d9d9",
                      borderRadius: "4px",
                    }}
                    cursor={{ fill: "rgba(24, 144, 255, 0.1)" }}
                  />
                  <Legend />
                  <Bar
                    dataKey="documentCount"
                    name="Document Count"
                    radius={[8, 8, 0, 0]}
                  >
                    {statusData.map((entry) => {
                      const colors: { [key: number]: string } = {
                        0: "#8c8c8c", // Draft - Gray
                        1: "#faad14", // Pending Task - Orange
                        2: "#52c41a", // Approved - Green
                        3: "#ff4d4f", // Rejected - Red
                        4: "#d9d9d9", // None - Light Gray
                      };
                      return (
                        <Cell
                          key={`cell-${entry.status}`}
                          fill={colors[entry.status] || "#1890ff"}
                        />
                      );
                    })}
                  </Bar>
                </BarChart>
              </ResponsiveContainer>
            )}
          </Card>
        </Col>
      </Row>

      <Row gutter={[16, 16]} style={{ marginTop: 16 }}>
        <Col xs={24} lg={12}>
          <Card
            title={
              <div
                style={{
                  padding: 8,
                  display: "flex",
                  flexDirection: "column",
                  width: "100%",
                }}
              >
                <div>Top 10 Categories with Most Documents</div>
                <div
                  style={{
                    display: "flex",
                    justifyContent: "flex-end",
                    width: "100%",
                  }}
                >
                  <RangePicker
                    value={categoryDateRange}
                    onChange={handleCategoryDateChange}
                    allowClear
                    format="YYYY-MM-DD"
                    size="small"
                  />
                </div>
              </div>
            }
            loading={categoryLoading && categoryData.length === 0}
            styles={{
              header: {
                alignItems: "flex-start",
              },
            }}
          >
            {categoryLoading ? (
              <div
                style={{
                  display: "flex",
                  justifyContent: "center",
                  alignItems: "center",
                  minHeight: "300px",
                }}
              >
                <Spin size="large" />
              </div>
            ) : categoryData.length === 0 ? (
              <Empty description="No data available" />
            ) : (
              <ResponsiveContainer width="100%" height={400}>
                <BarChart
                  data={categoryData}
                  margin={{ top: 5, right: 30, left: 20, bottom: 10 }}
                >
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis
                    dataKey="categoryName"
                    tick={{ fontSize: 11 }}
                    angle={-45}
                    textAnchor="end"
                    height={100}
                    tickFormatter={(value) =>
                      value.length > 15 ? `${value.substring(0, 12)}...` : value
                    }
                  />
                  <YAxis
                    allowDecimals={false}
                    label={{
                      value: "Document Count",
                      angle: -90,

                      position: "insideLeft",
                    }}
                  />
                  <Tooltip
                    contentStyle={{
                      backgroundColor: "#fff",
                      border: "1px solid #d9d9d9",
                      borderRadius: "4px",
                    }}
                    cursor={{ fill: "rgba(24, 144, 255, 0.1)" }}
                  />
                  <Legend />
                  <Bar
                    dataKey="documentCount"
                    name="Document Count"
                    radius={[8, 8, 0, 0]}
                  >
                    {categoryData.map((entry, index) => {
                      const colors = [
                        "#1890ff",
                        "#52c41a",
                        "#faad14",
                        "#f5222d",
                        "#722ed1",
                        "#13c2c2",
                        "#eb2f96",
                        "#fa8c16",
                        "#a0d911",
                        "#2f54eb",
                      ];
                      return (
                        <Cell
                          key={`cell-${entry.categoryId}`}
                          fill={colors[index % colors.length]}
                        />
                      );
                    })}
                  </Bar>
                </BarChart>
              </ResponsiveContainer>
            )}
          </Card>
        </Col>
        <Col xs={24} lg={12}>
          <Card
            title="Document Growth Over Time"
            loading={growthLoading && documentGrowthData.length === 0}
            extra={
              <Space size="small">
                <Text type="secondary">Year:</Text>
                <Select
                  value={selectedYear}
                  onChange={handleYearChange}
                  style={{ width: 100 }}
                  options={[
                    { value: dayjs().year() - 2, label: dayjs().year() - 2 },
                    { value: dayjs().year() - 1, label: dayjs().year() - 1 },
                    { value: dayjs().year(), label: dayjs().year() },
                  ]}
                />
              </Space>
            }
          >
            {growthLoading ? (
              <div
                style={{
                  display: "flex",
                  justifyContent: "center",
                  alignItems: "center",
                  minHeight: "300px",
                }}
              >
                <Spin size="large" />
              </div>
            ) : documentGrowthData.length === 0 ? (
              <Empty description="No data available" />
            ) : (
              <ResponsiveContainer width="100%" height={400}>
                <LineChart
                  data={documentGrowthData}
                  margin={{ top: 5, right: 30, left: 20, bottom: 5 }}
                >
                  <CartesianGrid strokeDasharray="3 3" vertical={true} />
                  <XAxis
                    dataKey="monthName"
                    tick={{ fontSize: 12 }}
                    interval={0}
                    angle={-45}
                    textAnchor="end"
                    height={80}
                  />
                  <YAxis
                    allowDecimals={false}
                    label={{
                      value: "Total Documents",
                      angle: -90,
                      position: "insideLeft",
                    }}
                  />
                  <Tooltip
                    contentStyle={{
                      backgroundColor: "#fff",
                      border: "1px solid #d9d9d9",
                      borderRadius: "4px",
                    }}
                  />
                  <Legend />
                  <Line
                    type="monotone"
                    dataKey="totalDocuments"
                    stroke="#1890ff"
                    strokeWidth={2}
                    dot={{ fill: "#1890ff", r: 4 }}
                    activeDot={{ r: 6 }}
                    name="Total Documents"
                  />
                </LineChart>
              </ResponsiveContainer>
            )}
          </Card>
        </Col>
      </Row>
    </>
  );
};

export default Dashboard;
