import {
  Button,
  Card,
  Col,
  Flex,
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
  FileExcelFilled,
  FilePdfOutlined,
  FileWordFilled,
  SearchOutlined,
  ShareAltOutlined,
} from "@ant-design/icons";
import { useEffect, useState } from "react";
import { Pencil, Trash2 } from "lucide-react";
import type { PaginationProps } from "antd";
import { useAuth } from "../../../context/AuthContext";
import { Link, useNavigate } from "react-router-dom";
import apiClient from "../../../services/apiClient";
import ModalShare from "../../atom/share/ModalShareDirect";

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

const iconList: any = {
  ".docx": <FileWordFilled style={{ fontSize: 35, color: "#1677ff" }} />,
  ".doc": <FileWordFilled style={{ fontSize: 35, color: "#1677ff" }} />,
  ".xls": <FileExcelFilled style={{ fontSize: 35, color: "#52c41a" }} />,
  ".xlsx": <FileExcelFilled style={{ fontSize: 35, color: "#52c41a" }} />,
  ".pdf": <FilePdfOutlined style={{ fontSize: 35, color: "#f5222d" }} />,
};

const TableItemList = () => {
  const navigate = useNavigate();
  const { config } = useAuth();
  const pageLengthConfig = config?.dataGrid?.rowPerPage || 5;
  const dateConfig =
    config?.dataGrid?.dateTimeFormat || "YYYY-MM-DD | HH:mm:ss";

  const [dataDocument, setDataDocument] = useState<any[]>([]);
  const [pageDocument, setPageDocument] = useState<number>(1);
  const pageLengthDocument = pageLengthConfig;
  const [loadingDocument, setLoadingDocument] = useState(false);
  const [totalRowDocument, setTotalRowDocument] = useState<number>(0);
  const [searchDocument, setSearchDocument] = useState<string>("");
  const [deleteData, setDeleteData] = useState<any>({});
  const [openConfirmDelete, setOpenConfirmDelete] = useState(false);
  const [confirmDeleteLoading, setConfirmDeleteLoading] = useState(false);
  const [openShare, setOpenShare] = useState(false);
  const [dataShare, setDataShare] = useState<any>({});

  const columns = [
    {
      dataIndex: "documentTitle",
      key: "documentTitle",
      title: "Document Name",
      render: (items: any, row: any) => (
        <Link to={`/document/document-view/${row.id}`}>
          <Flex gap="small" align="center">
            {iconList[row.fileType]}
            <Flex vertical>
              <span className="text-slate-800 font-bold hover:underline">
                {items}
              </span>
              <span className="text-slate-500 text-nowrap">
                {row.expiryDate}
              </span>
            </Flex>
          </Flex>
        </Link>
      ),
    },
    // {
    //     dataIndex: 'id',
    //     key: 'id',
    //     title: 'Document ID',
    //     render: (items: any) => (
    //         <Link to={`/document/document-view/${items}`}>
    //             <span className="text-slate-800 font-bold hover:underline">{items}</span>
    //         </Link>
    //     ),
    // },
    {
      dataIndex: "documentDesc",
      key: "documentDesc",
      title: "Description",
      width: "15%",
    },
    {
      key: "ownerFullname",
      dataIndex: "ownerFullname",
      title: "Owner",
      render: (items: any, row: any) => (
        <Flex vertical>
          <span style={{ fontWeight: "bold" }}>{items}</span>
          <span>
            {row?.insertedAt ? dayjs.utc(row.insertedAt).format(dateConfig) : ""}
          </span>
        </Flex>
      ),
    },
    {
      title: "Last Update",
      key: "updatedByFullName",
      dataIndex: "updatedByFullName",
      render: (items: any, row: any) => (
        <Flex vertical>
          <span style={{ fontWeight: "bold" }}>{items}</span>
          <span>
            {row?.updateAt ? dayjs.utc(row.updateAt).format(dateConfig) : ""}
          </span>
        </Flex>
      ),
    },
    {
      key: "size",
      dataIndex: "size",
      title: "Size",
    },
    {
      title: "Action",
      key: "action",
      render: (_: any, row: any) => (
        <Space size="middle">
          <Tooltip placement="bottom" title="Share" color="#595959">
            <Button
              type="text"
              icon={
                <ShareAltOutlined style={{ fontSize: 18, color: "#595959" }} />
              }
              onClick={() => handleOpenShare(row)}
            />
          </Tooltip>
          <Tooltip placement="bottom" title="Edit" color="#595959">
            <Button
              type="text"
              icon={<Pencil size={18} color="#595959" />}
              onClick={() => navigate(`/document/document-edit/${row.id}`)}
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
    getDataDocument();
  }, [pageDocument]);

  const getDataDocument = () => {
    setLoadingDocument(true);
    apiClient
      .get(
        `/favorite/get-fav-document?DocumentTitle=${searchDocument}&Page=${pageDocument}&Limit=${pageLengthDocument}`
      )
      .then(({ data }) => {
        setDataDocument(data.data.data);
        setTotalRowDocument(data.data.totalRecords);
        setLoadingDocument(false);
      })
      .catch((err) => {
        console.log(err);
        setLoadingDocument(false);
      });
  };

  const handleTableChange: TableProps<any>["onChange"] = (pagination) => {
    setPageDocument(Number(pagination.current));
  };

  const handleKeyDownDocument = (event: any) => {
    if (event.key === "Enter" || event.code === "Enter") {
      const str =
        searchDocument.length > 0 ? searchDocument.toLocaleLowerCase() : "";
      setSearchDocument(str);
      getDataDocument();
    }
  };

  const handleDelete = (item: any) => {
    setDeleteData(item);
    setOpenConfirmDelete(true);
  };

  const handleOkDelete = () => {
    if (Object.keys(deleteData).length == 0)
      return message.error("Document ID not found!");
    setConfirmDeleteLoading(true);
    apiClient
      .put(`/documents/un-favorite?documentId=${deleteData.id}`)
      .then(({ data }) => {
        console.log(data);
        setConfirmDeleteLoading(false);
        setOpenConfirmDelete(false);
        setDeleteData({});
        getDataDocument();
        message.success("Remove from favorite successfully");
      })
      .catch((err) => {
        console.log(err);
        setConfirmDeleteLoading(false);
        message.error(err.response.data.message);
      });
  };

  const handleOpenShare = (items: any) => {
    setDataShare(items.id);
    setOpenShare(true);
  };

  return (
    <>
      <Row style={{ marginBottom: 20 }}>
        <Col span={24}>
          <Flex justify="space-between">
            <span className="text-2xl font-bold">Document</span>
            <Space size="large">
              <Input
                size="middle"
                onChange={(e: any) => setSearchDocument(e.target.value)}
                onKeyDown={handleKeyDownDocument}
                placeholder="Search ..."
                prefix={<SearchOutlined />}
              />
            </Space>
          </Flex>
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
                pageSize: pageLengthDocument,
                total: totalRowDocument,
              }}
              columns={columns}
              dataSource={dataDocument}
              loading={loadingDocument}
            />
          </Card>
        </Col>
      </Row>
      {/* modal share */}
      <ModalShare
        open={openShare}
        setOpen={setOpenShare}
        document={dataShare}
        rerenderData={getDataDocument}
      />
      {/* modal confirm delete */}
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
            Are you sure you want to delete this Document from favorite?
          </p>
          <span className="text-gray-500">
            Deleting this document will remove form favorite
          </span>
        </div>
      </Modal>
    </>
  );
};

export default TableItemList;
