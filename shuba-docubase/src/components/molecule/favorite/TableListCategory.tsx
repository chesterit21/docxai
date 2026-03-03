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
  FolderFilled,
  SearchOutlined,
  ShareAltOutlined,
} from "@ant-design/icons";
import { useEffect, useState } from "react";
import { Trash2 } from "lucide-react";
import type { PaginationProps } from "antd";
import { useAuth } from "../../../context/AuthContext";
import { Link } from "react-router-dom";
import apiClient from "../../../services/apiClient";
import ModalShare from "../../atom/share/ModalShareDirectCat";

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
  none: <FolderFilled style={{ fontSize: 37, color: "#9254de" }} />,
};

const TableItemList = () => {
  const { config } = useAuth();
  const pageLengthConfig = config?.dataGrid?.rowPerPage || 5;
  const dateConfig =
    config?.dataGrid?.dateTimeFormat || "YYYY-MM-DD | HH:mm:ss";

  const [dataCategory, setDataCategory] = useState<any[]>([]);
  const [page, setPage] = useState<number>(1);
  const pageLength = pageLengthConfig;
  const [loading, setLoading] = useState(false);
  const [totalRow, setTotalRow] = useState<number>(0);
  const [search, setSearch] = useState<string>("");
  const [openShare, setOpenShare] = useState(false);
  const [categoryShare, setCategoryShare] = useState<any>({});
  const [deleteCategory, setDeleteCategory] = useState<any>({});
  const [openConfirmDelete, setOpenConfirmDelete] = useState(false);
  const [confirmDeleteLoading, setConfirmDeleteLoading] = useState(false);

  const columns = [
    {
      dataIndex: "categoryName",
      key: "categoryName",
      title: "Category Name",
      render: (items: any, row: any) => (
        <Link to={`/document/summit-corp/${row.id}/${items}`}>
          <Flex gap="small" align="center">
            {iconList["none"]}
            <Flex vertical>
              <span className="text-slate-800 font-bold hover:underline">
                {items}
              </span>
            </Flex>
          </Flex>
        </Link>
      ),
    },
    {
      dataIndex: "categoryDesc",
      key: "categoryDesc",
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
              onClick={() => {
                setCategoryShare(row);
                setOpenShare(true);
              }}
            />
          </Tooltip>
          {/* <Tooltip placement="bottom" title="Edit" color='#595959'>
                    <Button type="text" icon={<Pencil size={18} color='#595959'/>} onClick={()=> navigate(`/document/document-edit/${row.id}`)}/>
                </Tooltip> */}
          <Tooltip placement="bottom" title="Delete" color="#595959">
            <Button
              type="text"
              icon={<Trash2 size={18} color="#595959" />}
              onClick={() => handleDeleteCategory(row)}
            />
          </Tooltip>
        </Space>
      ),
    },
  ];

  useEffect(() => {
    getData();
  }, [page]);

  const getData = () => {
    setLoading(true);
    apiClient
      .get(
        `/favorite/get-fav-categories?CategoryName=${search}&Page=${page}&Limit=${pageLength}`
      )
      .then(({ data }) => {
        setDataCategory(data.data.data);
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

  const handleKeyDownDocument = (event: any) => {
    if (event.key === "Enter" || event.code === "Enter") {
      const str = search.length > 0 ? search.toLocaleLowerCase() : "";
      setSearch(str);
      getData();
    }
  };

  const handleDeleteCategory = (item: any) => {
    setDeleteCategory(item);
    setOpenConfirmDelete(true);
  };

  const handleOkDeleteCategory = () => {
    if (Object.keys(deleteCategory).length == 0)
      return message.error("Category ID not found!");
    setConfirmDeleteLoading(true);
    apiClient
      .put(`/category/un-favorite?categoryId=${deleteCategory.id}`)
      .then(({ data }) => {
        console.log(data);
        setConfirmDeleteLoading(false);
        setOpenConfirmDelete(false);
        setDeleteCategory({});
        getData();
        message.success("Remove from favorite successfully");
      })
      .catch((err) => {
        console.log(err);
        setConfirmDeleteLoading(false);
        message.error(err.response.data.message);
      });
  };

  return (
    <>
      <Row style={{ marginBottom: 20 }}>
        <Col span={24}>
          <Flex justify="space-between">
            <span className="text-2xl font-bold">Category</span>
            <Space size="large">
              <Input
                size="middle"
                onChange={(e: any) => setSearch(e.target.value)}
                onKeyDown={handleKeyDownDocument}
                placeholder="Search ..."
                prefix={<SearchOutlined />}
              />
              {/* <Button icon={<MailOutlined />} iconPosition="end">
                                Send Email
                            </Button> */}
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
                pageSize: pageLength,
                total: totalRow,
              }}
              columns={columns}
              dataSource={dataCategory}
              loading={loading}
            />
          </Card>
        </Col>
      </Row>
      {/* modal share */}
      <ModalShare
        open={openShare}
        setOpen={setOpenShare}
        category={categoryShare}
        rerenderData={getData}
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
                  onClick={handleOkDeleteCategory}
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
            Are you sure you want to delete this Category from favorite?
          </p>
          <span className="text-gray-500">
            Deleting this category will remove form favorite
          </span>
        </div>
      </Modal>
    </>
  );
};

export default TableItemList;
