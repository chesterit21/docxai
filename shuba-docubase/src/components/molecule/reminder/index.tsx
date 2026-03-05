import "./style.css";
import React, { useCallback, useEffect, useState } from "react";
import {
  Col,
  Flex,
  Row,
  Space,
  DatePicker,
  Form,
  Table,
  Button,
  Input,
  type PaginationProps,
} from "antd";
import {
  ArrowLeftOutlined,
  ArrowRightOutlined,
  SearchOutlined,
  EyeOutlined,
} from "@ant-design/icons";
import { useAuth } from "../../../context/AuthContext";
import apiClient from "../../../services/apiClient";
import { useNavigate } from "react-router-dom";
import dayjs from "dayjs";
import customParseFormat from "dayjs/plugin/customParseFormat";
import relativeTime from "dayjs/plugin/relativeTime";
import utc from "dayjs/plugin/utc";

dayjs.extend(customParseFormat);
dayjs.extend(relativeTime);
dayjs.extend(utc);

const now = dayjs.utc().format("YYYY-MM-DD");
const intervalDay = dayjs.utc().add(-1, "month").format("YYYY-MM-DD");

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

export default function Index() {
  const navigate = useNavigate();
  const { config } = useAuth();
  const pageLengthConfig = config?.dataGrid?.rowPerPage || 5;
  const dateConfig =
    config?.dataGrid?.dateTimeFormat || "YYYY-MM-DD | HH:mm:ss";

  const [data, setData] = useState<any[]>([]);
  const [page, setPage] = useState<number>(1);
  const pageLength = pageLengthConfig;
  const [totalRow, setTotalRow] = useState<number>(0);
  const [loading, setLoading] = useState(false);
  const [form] = Form.useForm();

  const getReminder = useCallback(async () => {
    setLoading(true);
    const queryParams = new URLSearchParams({
      ReminderDesc: form.getFieldValue("search") || "",
      DtFrom: form.getFieldValue("from")?.format("YYYY-MM-DD") || "",
      DtTo: form.getFieldValue("to")?.format("YYYY-MM-DD") || "",
    });

    apiClient
      .get(
        `/documentreminder/get-top-document-reminders?${queryParams}&Page=${page}&Limit=${pageLength}`
      )
      .then(({ data }) => {
        setData(data?.data?.data || []);
        setTotalRow(data?.data?.totalRecords || 0);
        setLoading(false);
      })
      .catch((err) => {
        console.log(err);
        setLoading(false);
        setData([]);
      });
  }, [form, page, pageLength]);

  useEffect(() => {
    form.setFieldsValue({ from: dayjs.utc(intervalDay), to: dayjs.utc(now) });
    getReminder();
  }, [form, page, getReminder]);

  const onChange = async () => {
    setPage(1);
    await getReminder();
  };

  const handleKeyDown = async (
    event: React.KeyboardEvent<HTMLInputElement>
  ) => {
    if (event.key === "Enter") {
      setPage(1);
      await getReminder();
    }
  };

  const columns = [
    {
      title: "Document Name",
      dataIndex: "documentTitle",
      key: "documentTitle",
      render: (text: string) => <span className="font-bold">{text}</span>,
    },
    {
      title: "Reminder Description",
      dataIndex: "reminderDesc",
      key: "reminderDesc",
      render: (text: string) => <span>{text}</span>,
    },
    {
      title: "Reminder Time",
      dataIndex: "reminderDateTime",
      key: "reminderDateTime",
      render: (date: string) => dayjs.utc(date).format(dateConfig),
    },
    {
      title: "Action",
      dataIndex: "action",
      key: "action",
      render: (_: any, record: any) => (
        <Button
          icon={<EyeOutlined />}
          onClick={() =>
            navigate(
              `/document/document-view/${record.documentId}`
            )
          }
        />
      ),
    },
  ];

  return (
    <>
      <Row>
        <Col span={24}>
          <div className="text-2xl font-bold mb-8">Reminder</div>
          <Form layout="horizontal" form={form}>
            <Flex justify="space-between">
              <div>
                <Form.Item name="search">
                  <Input
                    size="middle"
                    onKeyDown={handleKeyDown}
                    placeholder="Search ..."
                    prefix={<SearchOutlined />}
                  />
                </Form.Item>
              </div>
              <Space size="large">
                <Form.Item name="from" label="From" rules={[{ type: "date" }]}>
                  <DatePicker
                    format="YYYY-MM-DD"
                    placeholder="YYYY-MM-DD"
                    onChange={onChange}
                  />
                </Form.Item>
                <Form.Item name="to" label="To" rules={[{ type: "date" }]}>
                  <DatePicker
                    format="YYYY-MM-DD"
                    placeholder="YYYY-MM-DD"
                    onChange={onChange}
                  />
                </Form.Item>
              </Space>
            </Flex>
          </Form>
        </Col>
      </Row>
      <Row>
        <Col span={24}>
          <Table
            dataSource={data}
            columns={columns}
            loading={loading}
            rowKey="id"
            pagination={{
              current: page,
              pageSize: pageLength,
              total: totalRow,
              onChange: (page) => setPage(page),
              itemRender: itemRender,
              align: "center",
            }}
          />
        </Col>
      </Row>
    </>
  );
}
