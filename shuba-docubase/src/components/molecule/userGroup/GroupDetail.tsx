import { useEffect, useState } from "react";
import Breadcrumb from "../../atom/Breadcrumb";
import {
  Avatar,
  Button,
  Card,
  Col,
  Divider,
  Flex,
  Row,
  Space,
  Table,
  Input,
} from "antd";
import { useNavigate, useParams } from "react-router-dom";
import apiClient from "../../../services/apiClient";
import { UserOutlined } from "@ant-design/icons";
import { useAuth } from "../../../context/AuthContext";
import dayjs from "dayjs";
import customParseFormat from "dayjs/plugin/customParseFormat";

dayjs.extend(customParseFormat);

export default function Index() {
  const navigate = useNavigate();
  const { config } = useAuth();
  const dateConfig =
    config?.dataGrid?.dateTimeFormat || "YYYY-MM-DD | HH:mm:ss";
  const { uuid: documentId } = useParams();
  const [dataDetail, setDataDetail] = useState<any>({});
  const [groupMember, setGroupMember] = useState<any[]>([]);
  const [memberSearch, setMemberSearch] = useState<string>("");

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
      dataIndex: "insertedByFullName",
      key: "insertedByFullName",
      title: "Inserted By",
    },
    {
      dataIndex: "insertedAt",
      key: "insertedAt",
      title: "Inserted At",
      render: (items: any) => (
        <Flex vertical>
          <span>{items ? dayjs.utc(items).format(dateConfig) : ""}</span>
        </Flex>
      ),
    },
    {
      dataIndex: "updatedByFullName",
      key: "updatedByFullName",
      title: "Updated By",
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
  ];

  useEffect(() => {
    getDataDetail();
    getDataMember();
  }, [documentId]);

  const getDataDetail = () => {
    apiClient
      .get(`/group/get-byid?id=${documentId}`)
      .then(({ data }) => {
        setDataDetail(data?.data || {});
      })
      .catch((err) => {
        console.log(err);
      });
  };

  const getDataMember = () => {
    apiClient
      .get(`/group/get-group-members?id=${documentId}`)
      .then(({ data }) => {
        setGroupMember(data?.data?.[0]?.userGroup || []);
      })
      .catch((err) => {
        console.log(err);
      });
  };

  const filteredMembers = groupMember.filter((member) => {
    if (!memberSearch) return true;
    return member?.fullName?.toLowerCase().includes(memberSearch.toLowerCase());
  });

  return (
    <>
      <Breadcrumb
        item={[
          { title: <span style={{ fontWeight: "bold" }}>Group Detail</span> },
        ]}
      />
      <Row gutter={[16, 16]} style={{ marginTop: 30 }}>
        <Col span={24}>
          <Card styles={{ body: { padding: 0 } }} style={{ border: "none" }}>
            <p className="font-bold">Group Name</p>
            <span className="">{dataDetail?.groupName || "-"}</span>
            <Divider style={{ margin: "10px 0px" }}></Divider>
          </Card>
        </Col>
        <Col span={24}>
          <Card styles={{ body: { padding: 0 } }} style={{ border: "none" }}>
            <p className="font-bold">Description</p>
            <span className="">{dataDetail?.groupDescription || "-"}</span>
            <Divider style={{ margin: "10px 0px" }}></Divider>
          </Card>
        </Col>
        <Col span={24}>
          <p className="font-bold">Member List</p>
          <Input
            placeholder="Search member"
            style={{ marginBottom: 12 }}
            value={memberSearch}
            onChange={(e) => setMemberSearch(e.target.value)}
          />
          <Card
            styles={{
              body: {
                padding: 0,
              },
            }}
          >
            <Table
              scroll={{ x: 1000 }}
              pagination={false}
              columns={columns}
              dataSource={filteredMembers}
            />
          </Card>
        </Col>
        <Col span={24} style={{ display: "flex", justifyContent: "end" }}>
          <Space size="large">
            <Button
              onClick={() => navigate("/users-group?tab=group")}
              color="danger"
              variant="filled"
            >
              Back
            </Button>
          </Space>
        </Col>
      </Row>
    </>
  );
}
