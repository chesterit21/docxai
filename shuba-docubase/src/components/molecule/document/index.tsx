import { useEffect, useState } from "react";
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
  Segmented,
  Space,
  Tooltip,
  Typography,
  type PaginationProps,
} from "antd";
import {
  AppstoreOutlined,
  ArrowLeftOutlined,
  ArrowRightOutlined,
  BarsOutlined,
  CloseOutlined,
  DashOutlined,
  ExclamationCircleOutlined,
  FolderFilled,
  FormOutlined,
  SearchOutlined,
  ShareAltOutlined,
  StarFilled,
} from "@ant-design/icons";
import { FileCheck2, Trash2 } from "lucide-react";
import TableComplex from "../../atom/viewData/TableComplex";
import ListComplex from "../../atom/viewData/ListComplex";
import ModalAddCategory from "./ModalAddCategory";
import ModalEditCategory from "./ModalEditCategory";
import ModalShare from "../../atom/share/ModalShareDirectCat";
import ModalWorkflow from "../../atom/workflow/ModalWorkflowCategory";
import { useNavigate } from "react-router-dom";
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

export default function Index() {
  const navigate = useNavigate();
  const { config } = useAuth();
  const pageLengthConfig = config?.dataGrid?.rowPerPage || 5;
  const dateConfig =
    config?.dataGrid?.dateTimeFormat || "YYYY-MM-DD | HH:mm:ss";
  const [dataCategory, setDataCategory] = useState<any>([]);
  const [totalRowCategory, setTotalRowCategory] = useState<number>(0);
  const [pageCategory, setPageCategory] = useState<number>(1);
  const pageLengthCategory = 12;
  const [loadingCategory, setLoadingCategory] = useState(false);
  const [searchCategory, setSearchCategory] = useState<string>("");

  const [documentHighlight, setDocumentHighlight] = useState<any>([]);
  const [pageHighlight, setPageHighlight] = useState<number>(1);
  const pageLengthHighlight = pageLengthConfig;
  const [totalRowHighlight, setTotalRowHighlight] = useState<number>(0);
  const [loadingHighlight, setLoadingHighlight] = useState(false);

  const [documentRecent, setDocumentRecent] = useState<any>([]);
  const [pageRecent, setPageRecent] = useState<number>(1);
  const pageLengthRecent = pageLengthConfig;
  const [totalRowRecent, setTotalRowRecent] = useState<number>(0);
  const [loadingRecent, setLoadingRecent] = useState(false);

  const [viewType, setViewType] = useState<"table" | "list">("table");
  const [openEditCategory, setOpenEditCategory] = useState(false);
  const [deleteCategory, setDeleteCategory] = useState<any>({});
  const [openConfirmDelete, setOpenConfirmDelete] = useState(false);
  const [confirmDeleteLoading, setConfirmDeleteLoading] = useState(false);
  const [openShare, setOpenShare] = useState(false);
  const [openWorkflow, setOpenWorkflow] = useState(false);
  const [eventData, setEventData] = useState<any>({});
  const [categoryWorkflow, setCategoryWorkflow] = useState<any>({});
  const [categoryShared, setCategoryShared] = useState<any>({});

  const getCategory = () => {
    setLoadingCategory(true);
    apiClient
      .get(
        `/category?CategoryName=${searchCategory}&Page=${pageCategory}&Limit=${pageLengthCategory}`
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

  const getDocumentHighlight = () => {
    setLoadingHighlight(true);
    apiClient
      .get(
        `/documents/get-highlight-document?Page=${pageHighlight}&Limit=${pageLengthHighlight}`
      )
      .then(({ data }) => {
        setDocumentHighlight(data.data.data);
        setTotalRowHighlight(data.data.totalRecords);
        setLoadingHighlight(false);
      })
      .catch((err) => {
        console.log(err);
        setLoadingHighlight(false);
      });
  };

  const getDocumentRecent = () => {
    setLoadingRecent(true);
    apiClient
      .get(
        `/documents/get-recent-document?Page=${pageRecent}&Limit=${pageLengthRecent}`
      )
      .then(({ data }) => {
        setDocumentRecent(data.data.data);
        setTotalRowRecent(data.data.totalRecords);
        setLoadingRecent(false);
      })
      .catch((err) => {
        console.log(err);
        setLoadingRecent(false);
      });
  };

  useEffect(() => {
    getCategory();
  }, [pageCategory]);

  useEffect(() => {
    getDocumentHighlight();
  }, [pageHighlight]);

  useEffect(() => {
    getDocumentRecent();
  }, [pageRecent]);

  // useEffect(()=> {
  //     setDocumentHighlight(documentDummy)
  //     setDocumentRecent(documentDummy)
  // }, [])

  const handleKeyDown = (event: any) => {
    if (event.key === "Enter" || event.code === "Enter") {
      const str =
        searchCategory.length > 0 ? searchCategory.toLocaleLowerCase() : "";
      setSearchCategory(str);
      getCategory();
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
        getCategory();
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
    setCategoryWorkflow(item);
    setOpenWorkflow(true);
  };

  const handleSharedOpen = (item: any) => {
    setCategoryShared(item);
    setOpenShare(true);
  };

  const canEditCat = (item: any) => !!item.isOwned || !!item.privillege?.isEdit;
  const canDeleteCat = (item: any) =>
    !!item.isOwned || !!item.privillege?.isDelete;
  const canWorkflow = (item: any) =>
    !!item.isOwned || !!item.privillege?.isEdit;
  const canShare = (item: any) => item.isOwned || canEditCat(item);
  return (
    <>
      <Row gutter={[12, 50]} style={{ marginBottom: 20 }}>
        <Col span={24}>
          <div className="mb-5">
            <Flex justify="space-between" align="center">
              <span className="text-2xl font-bold">Categories</span>
              <Space size="large">
                <Input
                  size="middle"
                  onChange={(e: any) => setSearchCategory(e.target.value)}
                  onKeyDown={handleKeyDown}
                  placeholder="Search ..."
                  prefix={<SearchOutlined />}
                />
                <ModalAddCategory rerenderData={getCategory} />
              </Space>
            </Flex>
          </div>
          <ConfigProvider
            renderEmpty={() => (
              <Empty
                image={Empty.PRESENTED_IMAGE_SIMPLE}
                description="Has no category"
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
                                        ? dayjs
                                            .utc(item.insertedAt)
                                            .format(dateConfig)
                                        : "-"}
                                    </p>
                                    <p className="text-justify mb-0!">
                                      Update By:{" "}
                                      {item?.updatedByFullName || "-"}, Last
                                      Update Date:{" "}
                                      {item?.lastUpdateDate
                                        ? dayjs
                                            .utc(item.lastUpdateDate)
                                            .format(dateConfig)
                                        : "-"}
                                    </p>
                                    {item?.isShared && (
                                      <p className="text-justify mb-0!">
                                        Shared By:{" "}
                                        {item?.sharedByFullName || "-"}, Shared
                                        Date:{" "}
                                        {item?.sharedAt
                                          ? dayjs
                                              .utc(item.sharedAt)
                                              .format(dateConfig)
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

                                    {/* Workflow */}
                                    <Tooltip
                                      placement="left"
                                      title={
                                        canWorkflow(item)
                                          ? "Workflow"
                                          : "No permission"
                                      }
                                      color="#595959"
                                    >
                                      <Button
                                        type="text"
                                        disabled={!canWorkflow(item)}
                                        icon={
                                          <FileCheck2
                                            size={18}
                                            color={
                                              canWorkflow(item)
                                                ? "#595959"
                                                : "#d9d9d9"
                                            }
                                          />
                                        }
                                        iconPosition="start"
                                        onClick={() =>
                                          canWorkflow(item)
                                            ? handleFlowCatOpen(item)
                                            : message.warning(
                                                "View-only: cannot access workflow"
                                              )
                                        }
                                      >
                                        Workflow
                                      </Button>
                                    </Tooltip>

                                    {/* Share */}
                                    <Tooltip
                                      placement="left"
                                      title={
                                        canShare(item)
                                          ? "Share"
                                          : "No permission"
                                      }
                                      color="#595959"
                                    >
                                      <Button
                                        type="text"
                                        disabled={!canShare(item)}
                                        icon={
                                          <ShareAltOutlined
                                            style={{
                                              fontSize: 18,
                                              color: canShare(item)
                                                ? "#595959"
                                                : "#d9d9d9",
                                            }}
                                          />
                                        }
                                        iconPosition="start"
                                        onClick={() =>
                                          canShare(item)
                                            ? handleSharedOpen(item)
                                            : message.warning(
                                                "View-only: cannot share"
                                              )
                                        }
                                      >
                                        Share
                                      </Button>
                                    </Tooltip>
                                  </Col>

                                  <Col span={12}>
                                    <Tooltip
                                      placement="left"
                                      title={
                                        canEditCat(item)
                                          ? "Edit"
                                          : "No edit permission"
                                      }
                                      color="#595959"
                                    >
                                      <Button
                                        type="text"
                                        disabled={!canEditCat(item)}
                                        icon={
                                          <FormOutlined
                                            style={{
                                              fontSize: 18,
                                              color: canEditCat(item)
                                                ? "#595959"
                                                : "#d9d9d9",
                                            }}
                                          />
                                        }
                                        iconPosition="start"
                                        onClick={() =>
                                          canEditCat(item)
                                            ? handleUpdateClick(item)
                                            : message.warning(
                                                "View-only: cannot edit"
                                              )
                                        }
                                      >
                                        Edit
                                      </Button>
                                    </Tooltip>

                                    <Tooltip
                                      placement="left"
                                      title={
                                        canDeleteCat(item)
                                          ? "Delete"
                                          : "No delete permission"
                                      }
                                      color="#595959"
                                    >
                                      <Button
                                        type="text"
                                        disabled={!canDeleteCat(item)}
                                        icon={
                                          <Trash2
                                            size={18}
                                            color={
                                              canDeleteCat(item)
                                                ? "#595959"
                                                : "#d9d9d9"
                                            }
                                          />
                                        }
                                        iconPosition="start"
                                        onClick={() =>
                                          canDeleteCat(item)
                                            ? handleDeleteCategory(item)
                                            : message.warning(
                                                "View-only: cannot delete"
                                              )
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
        <Col span={24}>
          <div className="mb-5">
            <Flex justify="space-between" align="center">
              <span className="text-xl text-red-600">Highlight Document</span>
              <Space size="large">
                <Segmented
                  size="large"
                  onChange={(val: any) => [setViewType(val)]}
                  options={[
                    { value: "table", icon: <AppstoreOutlined /> },
                    { value: "list", icon: <BarsOutlined /> },
                  ]}
                />
              </Space>
            </Flex>
          </div>
          {viewType == "table" ? (
            <TableComplex
              data={documentHighlight}
              setData={setDocumentHighlight}
              pageSize={pageLengthHighlight}
              page={pageHighlight}
              setPage={setPageHighlight}
              totalRow={totalRowHighlight}
              loading={loadingHighlight}
              rerenderData={getDocumentHighlight}
              anotherData={documentRecent}
              setAnotherData={setDocumentRecent}
              anotherRerenderData={getDocumentRecent}
            />
          ) : (
            <ListComplex
              data={documentHighlight}
              pageLength={pageLengthHighlight}
              page={pageHighlight}
              setPage={setPageHighlight}
              total={totalRowHighlight}
              loading={loadingHighlight}
              rerenderData={getDocumentHighlight}
            />
          )}
        </Col>
        <Col span={24}>
          <div className="mb-5">
            <Flex justify="space-between" align="center">
              <span className="text-xl">Recent Document</span>
            </Flex>
          </div>
          {viewType == "table" ? (
            <TableComplex
              data={documentRecent}
              setData={setDocumentRecent}
              pageSize={pageLengthRecent}
              page={pageRecent}
              setPage={setPageRecent}
              totalRow={totalRowRecent}
              loading={loadingRecent}
              rerenderData={getDocumentRecent}
              anotherData={documentHighlight}
              setAnotherData={setDocumentHighlight}
              anotherRerenderData={getDocumentHighlight}
            />
          ) : (
            <ListComplex
              data={documentRecent}
              pageLength={pageLengthRecent}
              page={pageRecent}
              setPage={setPageRecent}
              total={totalRowRecent}
              loading={loadingRecent}
              rerenderData={getDocumentRecent}
            />
          )}
        </Col>
      </Row>
      {/* modal workflow */}
      <ModalWorkflow
        open={openWorkflow}
        setOpen={setOpenWorkflow}
        category={categoryWorkflow}
        rerenderData={getCategory}
      />
      {/* modal share */}
      <ModalShare
        open={openShare}
        setOpen={setOpenShare}
        category={categoryShared}
        rerenderData={getCategory}
      />
      {/* modal edit */}
      <ModalEditCategory
        open={openEditCategory}
        setOpen={setOpenEditCategory}
        eventData={eventData}
        rerenderData={getCategory}
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
    </>
  );
}
