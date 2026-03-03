import {
  Button,
  Card,
  Col,
  ConfigProvider,
  Empty,
  Flex,
  Input,
  List,
  message,
  Modal,
  Popover,
  Row,
  Space,
  Tooltip,
  Typography,
  type PaginationProps,
} from "antd";
import Breadcrumb from "../../atom/Breadcrumb";
import {
  ArrowLeftOutlined,
  ArrowRightOutlined,
  CloseOutlined,
  DashOutlined,
  DownloadOutlined,
  ExclamationCircleOutlined,
  FolderFilled,
  FormOutlined,
  PlusOutlined,
  SearchOutlined,
  ShareAltOutlined,
  StarFilled,
} from "@ant-design/icons";
import { useEffect, useState } from "react";
import { FileCheck2, Trash2 } from "lucide-react";
import { Link, useNavigate, useParams } from "react-router-dom";
import ModalAddCategory from "./ModalAddCategory";
import ModalEditCategory from "./ModalEditCategory";
import ModalShare from "../../atom/share/ModalShareDirectCat";
import ModalWorkflow from "../../atom/workflow/ModalWorkflowCategory";
import TableComplex from "../../atom/viewData/TableComplex";
import ModalAddDocument from "../../atom/upload/ModalAddDocument";
import ModalUploadDocument from "../../atom/upload/ModalUploadDocument";
import apiClient from "../../../services/apiClient";
import { useAuth } from "../../../context/AuthContext";
import dayjs from "dayjs";
import customParseFormat from "dayjs/plugin/customParseFormat";
dayjs.extend(customParseFormat);

const { Paragraph } = Typography;

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

const TableItemList = () => {
  const navigate = useNavigate();
  const { config } = useAuth();
  const pageLengthConfig = config?.dataGrid?.rowPerPage || 5;
  const dateConfig =
    config?.dataGrid?.dateTimeFormat || "YYYY-MM-DD | HH:mm:ss";
  const { uuid: categoryId, name: CategoryName } = useParams();
  const [dataCategory, setDataCategory] = useState<any>([]);
  const [pageCategory, setPageCategory] = useState<number>(1);
  const pageLengthCategory = 12;
  const [loadingCategory, setLoadingCategory] = useState(false);
  const [totalRowCategory, setTotalRowCategory] = useState<number>(0);
  const [searchCategory, setSearchCategory] = useState<string>("");
  const [eventData, setEventData] = useState<any>({});
  const [detailCategory, setDetailCategory] = useState<any>({});
  const [categoryWorkflow, setcategoryWorkflow] = useState<any>({});

  const [dataDocument, setDataDocument] = useState<any[]>([]);
  const [pageDocument, setPageDocument] = useState<number>(1);
  const pageLengthDocument = pageLengthConfig;
  const [loadingDocument, setLoadingDocument] = useState(false);
  const [totalRowDocument, setTotalRowDocument] = useState<number>(0);
  const [searchDocument, setSearchDocument] = useState<string>("");

  const [openEditCategory, setOpenEditCategory] = useState(false);
  const [deleteCategory, setDeleteCategory] = useState<any>({});
  const [openConfirmDelete, setOpenConfirmDelete] = useState(false);
  const [confirmDeleteLoading, setConfirmDeleteLoading] = useState(false);
  const [openShare, setOpenShare] = useState(false);
  const [categoryShare, setCategoryShare] = useState<any>({});
  const [openWorkflow, setOpenWorkflow] = useState(false);
  const [openAddDocument, setOpenAddDocument] = useState(false);
  const [openUploadDocument, setOpenUploadDocument] = useState(false);
  const [itemBreadcrumb, setItemBreadcrumb] = useState<any>([]);

  useEffect(() => {
    getDataCategory();
  }, [categoryId, pageCategory]);

  useEffect(() => {
    getDetailCategory();
  }, [categoryId]);

  useEffect(() => {
    getDataDocument();
  }, [categoryId, pageDocument]);

  useEffect(() => {
    if (Object.keys(detailCategory).length > 0) {
      // const defaultItem = [{ title: <Link to="/document">Document</Link> }]
      const item =
        detailCategory.parents &&
        Array.isArray(detailCategory.parents) &&
        detailCategory.parents.length > 0
          ? detailCategory.parents.reverse().map((parent: any) => ({
              title: (
                <Link to={`/document/summit-corp/${parent.id}/${parent.name}`}>
                  {parent.name}
                </Link>
              ),
            }))
          : [];
      setItemBreadcrumb([
        ...item,
        {
          title: (
            <div className="flex items-center gap-2 font-bold">
              {CategoryName}
              <Popover
                placement="bottomLeft"
                arrow={false}
                zIndex={100}
                content={
                  <div className="w-fit max-w-[350px] text-xs">
                    <p className="text-justify">
                      {detailCategory.categoryDesc}
                    </p>
                    <p className="text-justify mb-0!">
                      Created By: {detailCategory?.insertedByFullName || "-"},
                      Created Date:{" "}
                      {detailCategory?.insertedAt
                        ? dayjs.utc(detailCategory.insertedAt).format(dateConfig)
                        : "-"}
                    </p>
                    <p className="text-justify mb-0!">
                      Update By: {detailCategory?.updatedByFullName || "-"},
                      Last Update Date:{" "}
                      {detailCategory?.lastUpdateDate
                        ? dayjs.utc(detailCategory.lastUpdateDate).format(
                            dateConfig
                          )
                        : "-"}
                    </p>
                    {item?.isShared && (
                      <p className="text-justify mb-0!">
                        Shared By: {detailCategory?.sharedByFullName || "-"},
                        Shared Date:{" "}
                        {detailCategory?.sharedAt
                          ? dayjs.utc(detailCategory.sharedAt).format(dateConfig)
                          : "-"}
                      </p>
                    )}
                  </div>
                }
              >
                <ExclamationCircleOutlined className="text-blue-300!" />
              </Popover>
            </div>
          ),
        },
      ]);
    }
  }, [detailCategory]);

  const getDetailCategory = () => {
    setItemBreadcrumb([]);
    setSearchDocument("");
    apiClient
      .get(`/category/get-by-id?id=${categoryId}`)
      .then(({ data }) => {
        setDetailCategory(data.data);
      })
      .catch((err) => {
        console.log(err);
      });
  };

  const getDataCategory = () => {
    setLoadingCategory(true);
    apiClient
      .get(
        `/category?ParentCategoryId=${categoryId}&CategoryName=${searchCategory}&Page=${pageCategory}&Limit=${pageLengthCategory}`
      )
      .then(({ data }) => {
        setDataCategory(data.data.data);
        setTotalRowCategory(data.data.totalRecords);
        setLoadingCategory(false);
      })
      .catch((err) => {
        console.log(err);
        setLoadingCategory(false);
      });
  };

  const getDataDocument = () => {
    setLoadingDocument(true);
    apiClient
      .get(
        `/documents/list-document-category?CategoryId=${categoryId}&DocumentTile=${searchDocument}&Page=${pageDocument}&Limit=${pageLengthDocument}`
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

  const handleKeyDown = (event: any) => {
    if (event.key === "Enter" || event.code === "Enter") {
      const str =
        searchCategory.length > 0 ? searchCategory.toLocaleLowerCase() : "";
      setSearchCategory(str);
      getDataCategory();
    }
  };

  const handleKeyDownDocument = (event: any) => {
    if (event.key === "Enter" || event.code === "Enter") {
      const str = searchDocument.length > 0 ? searchDocument : "";
      setSearchDocument(str);
      getDataDocument();
    }
  };

  const handleUpdateClick = (item: any) => {
    setEventData(item);
    setOpenEditCategory(true);
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
      .put(`/category/delete?id=${deleteCategory.id}`)
      .then(({ data }) => {
        console.log(data);
        setConfirmDeleteLoading(false);
        setOpenConfirmDelete(false);
        setDeleteCategory({});
        getDataCategory();
      })
      .catch((err) => {
        console.log(err);
        setConfirmDeleteLoading(false);
      });
  };

  async function toggleFavorite(record: any) {
    const newData = dataCategory.map((row: any) =>
      row.id == record.id ? { ...row, isFavorite: !row.isFavorite } : row
    );
    if (!record.isFavorite) {
      await apiClient
        .put(`/category/add-favorite?categoryId=${record.id}`)
        .then(({ data }) => {
          console.log(data);
          message.success("Add to favorite successfully");
          setDataCategory(newData);
        })
        .catch((err) => {
          console.log(err);
          message.error(err.response.data.message);
        });
    } else {
      await apiClient
        .put(`/category/un-favorite?categoryId=${record.id}`)
        .then(({ data }) => {
          console.log(data);
          message.success("Remove from favorite successfully");
          setDataCategory(newData);
        })
        .catch((err) => {
          console.log(err);
          message.error(err.response.data.message);
        });
    }
  }

  const handleFlowCatOpen = (item: any) => {
    setcategoryWorkflow(item);
    setOpenWorkflow(true);
  };

  return (
    <>
      <Breadcrumb item={itemBreadcrumb} />
      <Row style={{ marginBottom: 20 }}>
        <Col span={24}>
          <Flex justify="space-between">
            <span className="text-2xl font-bold">{CategoryName}</span>
          </Flex>
        </Col>
      </Row>
      <Row gutter={[24, 24]} style={{ marginBottom: 40 }}>
        <Col span={24}>
          <Flex justify="space-between">
            <span className="text-xl font-bold">Categories</span>
            <Space size="large">
              <Input
                size="middle"
                onChange={(e: any) => setSearchCategory(e.target.value)}
                onKeyDown={handleKeyDown}
                placeholder="Search ..."
                prefix={<SearchOutlined />}
              />
              <ModalAddCategory
                rerenderData={getDataCategory}
                parent={categoryId}
              />
            </Space>
          </Flex>
        </Col>
        <Col span={24}>
          <ConfigProvider
            renderEmpty={() => (
              <Empty
                image={Empty.PRESENTED_IMAGE_SIMPLE}
                description="No sub category"
                styles={{
                  root: {
                    marginBlock: 0,
                  },
                }}
              />
            )}
          >
            <List
              grid={{
                gutter: 16,
                xs: 2,
                sm: 3,
                md: 3,
                lg: 5,
                xl: 5,
                xxl: 5,
              }}
              pagination={{
                onChange: (page) => {
                  setPageCategory(page);
                },
                hideOnSinglePage: true,
                pageSize: pageLengthCategory,
                total: totalRowCategory,
                itemRender: itemRender,
                position: "bottom",
                align: "center",
              }}
              dataSource={dataCategory}
              loading={loadingCategory}
              renderItem={(item: any, index: number) => (
                <List.Item>
                  <div className="col-span-1" key={index}>
                    <Card
                      styles={{
                        body: { padding: 13, backgroundColor: "#f0f0f0" },
                      }}
                    >
                      <div className="flex flex-col w-full h-[77px]">
                        <div className="flex justify-between">
                          <Popover
                            placement="bottomLeft"
                            arrow={false}
                            zIndex={100}
                            content={
                              <div className="w-fit max-w-[350px] text-xs">
                                <Space align="start">
                                  <ExclamationCircleOutlined />
                                  <div>
                                    <p className="text-justify mb-2!">
                                      {item.categoryDesc}
                                    </p>
                                    <p className="text-justify mb-0!">
                                      Created By: {item?.ownerFullName || "-"},
                                      Created Date:{" "}
                                      {item?.insertedAt
                                        ? dayjs.utc(item.insertedAt).format(
                                            dateConfig
                                          )
                                        : "-"}
                                    </p>
                                    <p className="text-justify mb-0!">
                                      Update By:{" "}
                                      {item?.updatedByFullName || "-"}, Last
                                      Update Date:{" "}
                                      {item?.lastUpdateDate
                                        ? dayjs.utc(item.lastUpdateDate).format(
                                            dateConfig
                                          )
                                        : "-"}
                                    </p>
                                    {item?.isShared && (
                                      <p className="text-justify mb-0!">
                                        Shared By:{" "}
                                        {item?.sharedByFullName || "-"}, Shared
                                        Date:{" "}
                                        {item?.sharedDate
                                          ? dayjs.utc(item.sharedDate).format(
                                              dateConfig
                                            )
                                          : "-"}
                                      </p>
                                    )}
                                  </div>
                                </Space>
                              </div>
                            }
                          >
                            <FolderFilled
                              style={{
                                fontSize: 50,
                                color:
                                  item.isNeedApproval && item.isShared
                                    ? "#fa541c"
                                    : item.isShared
                                    ? "#52c41a"
                                    : item.isNeedApproval
                                    ? "#ffc53d"
                                    : "#9254de",
                              }}
                              onClick={() =>
                                navigate(
                                  `/document/summit-corp/${item.id}/${item.categoryName}`
                                )
                              }
                            />
                          </Popover>
                          <Popover
                            placement="bottomLeft"
                            arrow={false}
                            zIndex={100}
                            content={
                              <div className="w-[230px]">
                                <Row gutter={12}>
                                  <Col span={12}>
                                    <Tooltip
                                      placement="left"
                                      title="Favorite"
                                      color="#595959"
                                    >
                                      <Button
                                        type="text"
                                        icon={
                                          item.isFavorite ? (
                                            <StarFilled
                                              style={{
                                                fontSize: 18,
                                                color: "#fa8c16",
                                              }}
                                            />
                                          ) : (
                                            <StarFilled
                                              style={{
                                                fontSize: 18,
                                                color: "#d9d9d9",
                                              }}
                                            />
                                          )
                                        }
                                        iconPosition="start"
                                        onClick={() => toggleFavorite(item)}
                                      >
                                        Fav
                                      </Button>
                                    </Tooltip>
                                    <Tooltip
                                      placement="left"
                                      title="Share"
                                      color="#595959"
                                    >
                                      <Button
                                        type="text"
                                        icon={
                                          <FileCheck2
                                            size={18}
                                            color="#595959"
                                          />
                                        }
                                        iconPosition="start"
                                        onClick={() => handleFlowCatOpen(item)}
                                      >
                                        Workflow
                                      </Button>
                                    </Tooltip>
                                    <Tooltip
                                      placement="left"
                                      title="Share"
                                      color="#595959"
                                    >
                                      <Button
                                        type="text"
                                        icon={
                                          <ShareAltOutlined
                                            style={{
                                              fontSize: 18,
                                              color: "#595959",
                                            }}
                                          />
                                        }
                                        iconPosition="start"
                                        onClick={() => {
                                          setCategoryShare(item);
                                          setOpenShare(true);
                                        }}
                                      >
                                        Share
                                      </Button>
                                    </Tooltip>
                                  </Col>
                                  <Col span={12}>
                                    <Tooltip
                                      placement="left"
                                      title="Edit"
                                      color="#595959"
                                    >
                                      <Button
                                        type="text"
                                        icon={
                                          <FormOutlined
                                            style={{
                                              fontSize: 18,
                                              color: "#595959",
                                            }}
                                          />
                                        }
                                        iconPosition="start"
                                        onClick={() => handleUpdateClick(item)}
                                      >
                                        Edit
                                      </Button>
                                    </Tooltip>
                                    <Tooltip
                                      placement="left"
                                      title="Delete"
                                      color="#595959"
                                    >
                                      <Button
                                        type="text"
                                        icon={
                                          <Trash2 size={18} color="#595959" />
                                        }
                                        iconPosition="start"
                                        onClick={() =>
                                          handleDeleteCategory(item)
                                        }
                                      >
                                        Delete
                                      </Button>
                                    </Tooltip>
                                  </Col>
                                </Row>
                              </div>
                            }
                          >
                            <Button type="text" icon={<DashOutlined />} />
                          </Popover>
                        </div>
                        <div className="flex flex-col overflow-y-auto">
                          <Paragraph
                            ellipsis={{
                              rows: 1,
                              expandable: true,
                              symbol: "more",
                            }}
                            style={{ marginBottom: 0 }}
                            className="text-md font-bold"
                          >
                            {item.categoryName}
                          </Paragraph>
                          {/* <Paragraph 
                                                ellipsis={{ rows: 2, expandable: true, symbol: 'more' }}
                                                style={{marginBottom: 0}}
                                                >
                                                    {
                                                        item?.isShared ? (
                                                            <span style={{fontSize:11}}>
                                                                Shared By: {item?.sharedBy || '-'}
                                                            </span>
                                                        ) : (
                                                            <span style={{fontSize:11}}>
                                                                Last Update Date: {item?.lastUpdateDate ? dayjs(item.lastUpdateDate).format(dateConfig): '-'}
                                                            </span>
                                                        )
                                                    }
                                                </Paragraph> */}
                        </div>
                      </div>
                    </Card>
                  </div>
                </List.Item>
              )}
            />
          </ConfigProvider>
        </Col>
      </Row>
      <Row style={{ marginBottom: 20 }}>
        <Col span={24}>
          <div className="grid grid-cols-2 lg:grid-cols-12 gap-4 mb-5">
            <div className="col-span-2 lg:col-span-3">
              <span className="text-xl font-bold">Document</span>
            </div>
            <div className="col-span-4 lg:col-span-5 lg:col-end-13 flex justify-start lg:justify-end gap-4">
              <Input
                value={searchDocument}
                className="w-[200px]! flex-none"
                size="middle"
                onChange={(e: any) => setSearchDocument(e.target.value)}
                onKeyDown={handleKeyDownDocument}
                placeholder="Search ..."
                prefix={<SearchOutlined />}
              />
              <Button
                icon={<PlusOutlined />}
                iconPosition="end"
                onClick={() => setOpenAddDocument(true)}
              >
                Add Document
              </Button>
              <Button
                icon={<DownloadOutlined />}
                iconPosition="end"
                onClick={() => setOpenUploadDocument(true)}
              >
                Upload File
              </Button>
            </div>
          </div>
        </Col>
        <Col span={24}>
          <Card
            styles={{
              body: {
                padding: 0,
              },
            }}
          >
            <TableComplex
              data={dataDocument}
              setData={setDataDocument}
              pageSize={pageLengthDocument}
              page={pageDocument}
              setPage={setPageDocument}
              totalRow={totalRowDocument}
              loading={loadingDocument}
              rerenderData={getDataDocument}
            />
          </Card>
        </Col>
      </Row>
      {/* modal workflow */}
      <ModalWorkflow
        open={openWorkflow}
        setOpen={setOpenWorkflow}
        category={categoryWorkflow}
        rerenderData={getDataCategory}
      />
      {/* modal share */}
      <ModalShare
        open={openShare}
        setOpen={setOpenShare}
        category={categoryShare.id ? categoryShare : detailCategory}
        rerenderData={getDataCategory}
      />
      {/* modal edit */}
      <ModalEditCategory
        open={openEditCategory}
        setOpen={setOpenEditCategory}
        eventData={eventData}
        rerenderData={getDataCategory}
        parent={categoryId}
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
            Are you sure you want to delete this Category?
          </p>
          <span className="text-gray-500">
            Deleting this folder/category will permanently delete all items
            within it. Make sure you have backed up important data before
            continuing..
          </span>
        </div>
      </Modal>
      {/* modal add document */}
      <ModalAddDocument
        open={openAddDocument}
        setOpen={setOpenAddDocument}
        categoryId={categoryId}
        rerenderData={getDataDocument}
      />
      {/* modal add document */}
      <ModalUploadDocument
        open={openUploadDocument}
        setOpen={setOpenUploadDocument}
        categoryId={categoryId}
        rerenderData={getDataDocument}
      />
    </>
  );
};

export default TableItemList;
