import { useEffect, useState } from "react";
import Breadcrumb from "../../atom/Breadcrumb";
import {
  Avatar,
  Button,
  Card,
  Col,
  ConfigProvider,
  Divider,
  Empty,
  List,
  message,
  Modal,
  Row,
  Skeleton,
  Space,
  Table,
  Tag,
  Tooltip,
} from "antd";
import {
  CheckOutlined,
  ClockCircleOutlined,
  CloseOutlined,
  ExclamationCircleOutlined,
  FileExcelFilled,
  FileImageOutlined,
  FilePdfOutlined,
  FilePptOutlined,
  FileTextOutlined,
  FileWordFilled,
  MailOutlined,
  UserOutlined,
} from "@ant-design/icons";
import { Link, useNavigate, useParams } from "react-router-dom";
import { Download, Pencil, ScanEye, Trash2 } from "lucide-react";
import LogDocument from "../../atom/ModalLogDocument";
import SendEmail from "../../atom/sendEmail/ModalSendEmail";
import apiClient from "../../../services/apiClient";
import ShowDocumentOnly from "../../atom/document/ShowDocumentOnly";
import { useAuth } from "../../../context/AuthContext";
import InfiniteScroll from "react-infinite-scroll-component";
import dayjs from "dayjs";
import customParseFormat from "dayjs/plugin/customParseFormat";
import { useMemo } from "react";
import utc from "dayjs/plugin/utc";
dayjs.extend(customParseFormat);
dayjs.extend(utc);

const iconList: any = {
  ".docx": <FileWordFilled style={{ fontSize: 40, color: "#1677ff" }} />,
  ".doc": <FileWordFilled style={{ fontSize: 40, color: "#1677ff" }} />,
  ".xls": <FileExcelFilled style={{ fontSize: 40, color: "#52c41a" }} />,
  ".xlsx": <FileExcelFilled style={{ fontSize: 40, color: "#52c41a" }} />,
  ".pdf": <FilePdfOutlined style={{ fontSize: 40, color: "#f5222d" }} />,
  ".png": <FileImageOutlined style={{ fontSize: 40, color: "#08979c" }} />,
  ".jpg": <FileImageOutlined style={{ fontSize: 40, color: "#08979c" }} />,
  ".jpeg": <FileImageOutlined style={{ fontSize: 40, color: "#08979c" }} />,
  ".txt": <FileTextOutlined style={{ fontSize: 40, color: "#595959" }} />,
  ".ppt": <FilePptOutlined style={{ fontSize: 40, color: "#ff7a45" }} />,
  ".pptx": <FilePptOutlined style={{ fontSize: 40, color: "#ff7a45" }} />,
};

export default function Index() {
  const navigate = useNavigate();
  const { uuid: documentId } = useParams();
  const { config } = useAuth();
  const dateConfig =
    config?.dataGrid?.dateTimeFormat || "YYYY-MM-DD | HH:mm:ss";

  const [, contextHolder] = message.useMessage();
  const [detailDocument, setDetailDocument] = useState<any>({});
  const [documentFiles, setDocumentFiles] = useState<any>([]);
  const [openConfirmDelete, setOpenConfirmDelete] = useState(false);
  const [confirmDeleteLoading, setConfirmDeleteLoading] = useState(false);
  const [fileSelected, setFileSelected] = useState<any>({});
  const [openDocument, setOpenDocument] = useState(false);
  const [userShareExist, setUserShareExist] = useState<any>([]);
  const [userWorkflowExist, setUserWorkflowExist] = useState<any>([]);

  const [documentRemainder, setDocumentRemainder] = useState<any>([]);
  const [pageRemainder, setPageRemainder] = useState(1);
  const [totalRowRemainder, setTotalRowRemainder] = useState(0);
  const [loadingRemainder, setLoadingRemainder] = useState(false);

  const [openLog, setOpenLog] = useState(false);
  const [openSendEmail, setOpenSendEmail] = useState(false);
  const [itemBreadcrumb, setItemBreadcrumb] = useState<any>([]);
  const [categoryName, setCategoryName] = useState<string>("");

  const fileColumns = [
    {
      dataIndex: "updateAt",
      key: "updateAt",
      title: "Last Updated",
      render: (item: any) => dayjs.utc(item).format(dateConfig),
    },
    {
      key: "documentFileName",
      dataIndex: "documentFileName",
      title: "File Name",
      render: (item: any) => {
        return <Tag style={{ color: "blue" }}>{item}</Tag>;
      },
    },
    {
      key: "documentType",
      dataIndex: "documentType",
      title: "Extension",
    },
    {
      dataIndex: "updatedByFullName",
      key: "updatedByFullName",
      title: "Update By",
    },
    {
      dataIndex: "documentFileSize",
      key: "documentFileSize",
      title: "File Size",
    },
    {
      title: "Action",
      key: "action",
      render: (items: any, row: any) => (
        <Space>
          {row?.isMainDocumentFile && <Tag style={{ color: "blue" }}>main</Tag>}
          <Tooltip placement="bottom" title="View" color="#595959">
            <Button
              type="text"
              icon={<ScanEye size={18} />}
              iconPosition="end"
              disabled={!priv.isView}
              onClick={() => handleClickShow(items)}
            />
          </Tooltip>
          <Tooltip placement="bottom" title="Download" color="#595959">
            <Button
              type="text"
              icon={<Download size={18} />}
              iconPosition="end"
              onClick={() => handleDownload(items)}
            />
          </Tooltip>
        </Space>
      ),
    },
  ];

  useEffect(() => {
    getDetailDocument();
    getDocumentRemainder();
  }, [documentId]);

  useEffect(() => {
    if (Object.keys(detailDocument).length > 0) {
      const item =
        detailDocument.parentsCategory &&
          Array.isArray(detailDocument.parentsCategory) &&
          detailDocument.parentsCategory.length > 0
          ? detailDocument.parentsCategory.reverse().map((parent: any) => ({
            title: (
              <Link to={`/document/summit-corp/${parent.id}/${parent.name}`}>
                {parent.name}
              </Link>
            ),
          }))
          : [];

      // Fetch category name and add to breadcrumb
      if (detailDocument.categoryID) {
        apiClient
          .get(`/category/get-by-id?id=${detailDocument.categoryID}`)
          .then(({ data }) => {
            const catName = data.data.categoryName;
            setCategoryName(catName);
            const safeCategoryName = encodeURIComponent(catName);
            const updatedItem = [
              ...item,
              {
                title: (
                  <Link
                    to={`/document/summit-corp/${detailDocument.categoryID}/${safeCategoryName}`}
                  >
                    {catName}
                  </Link>
                ),
              },
            ];
            setItemBreadcrumb(updatedItem);
          })
          .catch((err) => {
            console.error("Failed to fetch category name:", err);
            setItemBreadcrumb(item);
          });
      } else {
        setItemBreadcrumb(item);
      }
    }
  }, [detailDocument]);

  const handleBack = () => {
    if (detailDocument.categoryID && categoryName) {
      const safeCategoryName = encodeURIComponent(categoryName);
      navigate(
        `/document/summit-corp/${detailDocument.categoryID}/${safeCategoryName}`
      );
    } else {
      console.error("Category information not available");
      navigate("/dashboard");
    }
  };

  const { user } = useAuth(); // pastikan context mengembalikan user aktif
  const userId =
    user?.["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"] || 0;

  const { priv } = useMemo(() => {
    const isOwner = detailDocument?.userofOwner?.userID === Number(userId);

    const sharedList = detailDocument?.sharedTo?.sharedUser ?? [];
    const me = sharedList.find((u: any) => u.userID === Number(userId)) || {};

    // Owner selalu full access
    const p = {
      isView: !!(isOwner || me.isView),
      isEdit: !!(isOwner || me.isEdit),
      isDelete: !!(isOwner || me.isDelete),
    };

    return { priv: p };
  }, [detailDocument, user]);

  const getDetailDocument = () => {
    setItemBreadcrumb([]);
    apiClient
      .get(`/documents/get-document-dtl?id=${documentId}&mode=view`)
      .then(({ data }) => {
        setDetailDocument(data?.data || []);
        setDocumentFiles(data?.data?.documentFiles || []);
        setUserShareExist(data?.data?.sharedTo?.sharedUser || []);
        setUserWorkflowExist(data?.data?.workflow || []);
      })
      .catch((err) => {
        console.log(err);
      });
  };

  const getDocumentRemainder = () => {
    if (loadingRemainder) {
      return;
    }
    setLoadingRemainder(true);
    apiClient
      .get(
        `/documentreminder/get-document-reminders?DocumentID=${documentId}&Page=${pageRemainder}&Limit=2`
      )
      .then(({ data }) => {
        const getData = data.data.data;
        const results = Array.isArray(getData) ? getData : [];
        setDocumentRemainder([...documentRemainder, ...results]);
        setTotalRowRemainder(data?.data?.totalRecords);
        setLoadingRemainder(false);
        setPageRemainder(pageRemainder + 1);
      })
      .catch((err) => {
        console.log(err);
        setLoadingRemainder(false);
      });
  };

  const handleDelete = () => {
    setOpenConfirmDelete(true);
  };

  const handleGoEdit = () => {
    if (!priv.isEdit) {
      return message.warning(
        "View-only: you don't have permission to edit this document."
      );
    }
    navigate(`/document/document-edit/${documentId}`);
  };

  const handleOkDelete = () => {
    if (!documentId) return message.error("Document ID not found!");
    setConfirmDeleteLoading(true);
    apiClient
      .put(`/documents/delete?id=${documentId}`)
      .then(({ data }) => {
        console.log(data);
        setConfirmDeleteLoading(false);
        setOpenConfirmDelete(false);
        message.success("Delete data is successful");
        setTimeout(() => {
          navigate(-1);
        }, 500);
      })
      .catch((err) => {
        console.log(err);
        setConfirmDeleteLoading(false);
        setOpenConfirmDelete(false);
        message.error(err.response.data.message);
      });
  };

  const handleClickShow = (value: any) => {
    setFileSelected(value);
    setOpenDocument(true);
  };

  const handleDownload = (items: any) => {
    apiClient
      .get(`/documentfiles/download?documentfileId=${items.id}`, {
        responseType: "blob",
      })
      .then(({ data }) => {
        const fileURL = window.URL.createObjectURL(data);
        let alink = document.createElement("a");
        alink.href = fileURL;
        alink.download = items.documentFileName + items.documentType;
        alink.click();
        document.body.removeChild(alink);
      })
      .catch((err) => {
        console.log(err);
      });
    if (items.documentFilePath.lenght == 0)
      return message.error("Can not find the file");
    // fetch(import.meta.env.VITE_API_URL + items.documentFilePath).then((response) => {
    //     response.blob().then((blob) => {
    //         const fileURL = window.URL.createObjectURL(blob);
    //         let alink = document.createElement("a");
    //         alink.href = fileURL;
    //         alink.download = items.documentFileName + items.documentType;
    //         alink.click();
    //         document.body.removeChild(alink);
    //     });
    // });
  };

  return (
    <>
      {contextHolder}
      <div className="grid grid-cols-3 gap-4 mb-5">
        <div className="col-span-2 bg-white p-6 rounded-2xl h-fit">
          <div className="flex justify-between">
            <Breadcrumb item={itemBreadcrumb} />
            <span className="font-bold text-blue-600">View</span>
          </div>
          <div className="flex justify-end">
            <Button
              icon={<MailOutlined style={{ fontSize: 16 }} />}
              iconPosition="end"
              onClick={() => setOpenSendEmail(true)}
            >
              Send Email
            </Button>
          </div>
          <Row gutter={[16, 16]} style={{ marginTop: 0 }}>
            <Col span={24}>
              <Card
                styles={{ body: { padding: 0 } }}
                style={{ border: "none" }}
              >
                <p className="font-bold">Title</p>
                <span className="">{detailDocument?.documentTitle || "-"}</span>
                <Divider style={{ margin: "10px 0px" }}></Divider>
              </Card>
            </Col>
            <Col span={24}>
              <Card
                styles={{ body: { padding: 0 } }}
                style={{ border: "none" }}
              >
                <p className="font-bold">Status Approval</p>
                {detailDocument.approvalStatus == 1 ? (
                  <Tag color="orange">
                    {detailDocument.approvalStatusDesc} <ClockCircleOutlined />
                  </Tag>
                ) : detailDocument.approvalStatus == 2 ? (
                  <Tag color="green">
                    {detailDocument.approvalStatusDesc} <CheckOutlined />
                  </Tag>
                ) : detailDocument.approvalStatus == 3 ? (
                  <Tag color="magenta">
                    {detailDocument.approvalStatusDesc}{" "}
                    <ExclamationCircleOutlined />
                  </Tag>
                ) : (
                  <Tag color="default">{detailDocument.approvalStatusDesc}</Tag>
                )}
                <Divider style={{ margin: "10px 0px" }}></Divider>
              </Card>
            </Col>

            <Col span={24}>
              <Card
                styles={{ body: { padding: 0 } }}
                style={{ border: "none" }}
              >
                <p className="font-bold">Description</p>
                <span className="">{detailDocument?.documentDesc || "-"}</span>
                <Divider style={{ margin: "10px 0px" }}></Divider>
              </Card>
            </Col>
            {detailDocument?.documentAttributes &&
              Object.keys(detailDocument.documentAttributes).length > 0 &&
              JSON.parse(detailDocument.documentAttributes.attributeValues).map(
                (item: any, index: number) => (
                  <Col span={24} key={index}>
                    <Card
                      styles={{ body: { padding: 0 } }}
                      style={{ border: "none" }}
                    >
                      <p className="font-bold">{item.attributeName}</p>
                      <span className="">
                        {item.attributeType == "date"
                          ? dayjs.utc(item.value).format("YYYY-MM-DD")
                          : item.attributeType == "checkbox"
                            ? (item.value || []).join(", ")
                            : item.value}
                      </span>
                      <Divider style={{ margin: "10px 0px" }}></Divider>
                    </Card>
                  </Col>
                )
              )}
            <Col span={24}>
              <Card
                styles={{ body: { padding: 0 } }}
                style={{ border: "none" }}
              >
                <p className="font-bold">Related Document</p>
                <div className="flex flex-col gap-1">
                  {detailDocument?.documentRelated &&
                    detailDocument?.documentRelated?.length > 0
                    ? detailDocument.documentRelated.map(
                      (item: any, index: number) => (
                        <div
                          className="flex items-center gap-1.5"
                          key={index}
                        >
                          <FileTextOutlined
                            style={{ fontSize: 17, color: "#595959" }}
                          />
                          <Link to={`/document/document-view/${item.value}`}>
                            <span className="text-slate-800 font-semibold">
                              {item.text}
                            </span>
                          </Link>
                        </div>
                      )
                    )
                    : "-"}
                </div>
                <Divider style={{ margin: "10px 0px" }}></Divider>
              </Card>
            </Col>
            <Col span={24}>
              <p className="font-bold">Files</p>
              <Card styles={{ body: { padding: 0 } }}>
                <Table
                  rowKey="id"
                  pagination={false}
                  scroll={{ x: true }}
                  columns={fileColumns}
                  dataSource={documentFiles}
                />
              </Card>
            </Col>
            <Col span={24}>
              <div className="flex justify-between mt-5">
                <div className="flex justify-between gap-3">
                  <Button
                    type="primary"
                    icon={<Pencil size={14} />}
                    iconPosition="end"
                    onClick={handleGoEdit}
                    disabled={!priv.isEdit}
                  >
                    Edit
                  </Button>
                  <Button
                    type="primary"
                    icon={<Trash2 size={14} />}
                    iconPosition="end"
                    danger
                    onClick={handleDelete}
                    disabled={!priv.isDelete}
                  >
                    Delete
                  </Button>
                </div>
                <Button
                  icon={<CloseOutlined />}
                  iconPosition="end"
                  onClick={handleBack}
                  color="danger"
                  variant="filled"
                >
                  Back
                </Button>
              </div>

              <Space size="large"></Space>
            </Col>
          </Row>
        </div>
        <div className="col-span-1 bg-white p-6 rounded-2xl h-fit">
          <Row gutter={[15, 16]}>
            <Col span={24}>
              <div className="flex items-center gap-3">
                {iconList[detailDocument?.mainDocumentFile?.documentType]}
                <span
                  className="font-bold hover:underline hover:cursor-pointer break-words"
                  style={{ wordWrap: "break-word", overflowWrap: "break-word" }}
                  onClick={() =>
                    handleClickShow(detailDocument?.mainDocumentFile || {})
                  }
                >
                  {detailDocument?.mainDocumentFile?.documentFileName || "-"}
                </span>
              </div>
            </Col>
            <Col span={24}>
              <Card
                styles={{ body: { padding: 0 } }}
                style={{ border: "none" }}
              >
                <p className="font-bold">Document ID</p>
                <div className="flex justify-between">
                  <span className="">{detailDocument?.id || "-"}</span>
                </div>
                <Divider style={{ margin: "10px 0px" }}></Divider>
              </Card>
            </Col>
            <Col span={24}>
              <Card
                styles={{ body: { padding: 0 } }}
                style={{ border: "none" }}
              >
                <p className="font-bold">Owner</p>
                <span className="">
                  {detailDocument?.userofOwner?.fullName || "-"}
                </span>
                <Divider style={{ margin: "10px 0px" }}></Divider>
              </Card>
            </Col>
            <Col span={24}>
              <Card
                styles={{ body: { padding: 0 } }}
                style={{ border: "none" }}
              >
                <p className="font-bold">Last Update</p>
                <span className="">
                  {detailDocument?.updatedAt
                    ? dayjs.utc(detailDocument.updatedAt).format(dateConfig)
                    : "-"}
                </span>
                <Divider style={{ margin: "10px 0px" }}></Divider>
              </Card>
            </Col>
            <Col span={24}>
              <Card
                styles={{ body: { padding: 0 } }}
                style={{ border: "none" }}
              >
                <p className="font-bold">Document Size</p>
                <span className="">{detailDocument.fileSize || "-"}</span>
                <Divider style={{ margin: "10px 0px" }}></Divider>
              </Card>
            </Col>
            <Col span={24}>
              <p className="font-bold">Reminders:</p>
              <div id="scrollableDiv" className="max-h-52 overflow-y-auto">
                <InfiniteScroll
                  dataLength={documentRemainder.length}
                  next={getDocumentRemainder}
                  hasMore={documentRemainder.length < totalRowRemainder}
                  loader={<Skeleton avatar paragraph={{ rows: 1 }} active />}
                  scrollableTarget="scrollableDiv"
                >
                  <ConfigProvider
                    renderEmpty={() => (
                      <Empty
                        image={Empty.PRESENTED_IMAGE_SIMPLE}
                        description="Has no remainder"
                        styles={{
                          root: {
                            marginBlock: 0,
                          },
                        }}
                      />
                    )}
                  >
                    <List
                      dataSource={documentRemainder}
                      renderItem={(item: any, index: number) => (
                        <List.Item>
                          <Card
                            style={{ width: "100%" }}
                            styles={{ body: { padding: 13 } }}
                            key={index}
                          >
                            <Space size={10} direction="vertical">
                              <Tag
                                className={
                                  dayjs
                                    .utc(item.reminderDateTime)
                                    .isAfter(dayjs.utc())
                                    ? "!bg-green-300"
                                    : "!bg-red-400 !text-slate-50"
                                }
                              >
                                {dayjs
                                  .utc(item.reminderDateTime)
                                  .format(dateConfig)}
                              </Tag>
                              <p>{item?.reminderDesc || "-"}</p>
                            </Space>
                          </Card>
                        </List.Item>
                      )}
                    />
                  </ConfigProvider>
                </InfiniteScroll>
              </div>
            </Col>
            <Col span={24}>
              <Card
                styles={{ body: { padding: 0 } }}
                style={{ border: "none" }}
              >
                <p className="font-bold">Shared to</p>
                {userShareExist.length > 0 && (
                  <Avatar.Group
                    max={{
                      count: 5,
                      style: { color: "#f56a00", backgroundColor: "#fde3cf" },
                    }}
                  >
                    {userShareExist.map((item: any, index: number) => (
                      <Tooltip
                        key={index}
                        title={item.userName}
                        placement="top"
                      >
                        <Avatar icon={<UserOutlined />} />{" "}
                      </Tooltip>
                    ))}
                  </Avatar.Group>
                )}
              </Card>
            </Col>
            <Col span={24}>
              <Card
                styles={{ body: { padding: 0 } }}
                style={{ border: "none" }}
              >
                <p className="font-bold">Workflow</p>
                {Object.keys(userWorkflowExist).length > 0 && (
                  <Avatar.Group
                    max={{
                      count: 5,
                      style: { color: "#f56a00", backgroundColor: "#fde3cf" },
                    }}
                  >
                    {userWorkflowExist.approvalFlows.map(
                      (item: any, index: number) => (
                        <Tooltip
                          key={index}
                          title={item.user.fullName}
                          placement="top"
                        >
                          <Avatar icon={<UserOutlined />} />
                        </Tooltip>
                      )
                    )}
                  </Avatar.Group>
                )}
              </Card>
            </Col>
            <Col span={24}>
              <Card
                styles={{ body: { padding: 0 } }}
                style={{ border: "none" }}
              >
                <p className="font-bold">Audit Log</p>
                <Button
                  size="large"
                  variant="solid"
                  color="purple"
                  icon={<ScanEye size={16} />}
                  iconPosition="end"
                  block
                  onClick={() => setOpenLog(true)}
                >
                  Audit Log Detail
                </Button>
              </Card>
            </Col>
          </Row>
        </div>
      </div>
      <SendEmail
        open={openSendEmail}
        setOpen={setOpenSendEmail}
        document={detailDocument}
        fileList={documentFiles}
      />
      <LogDocument
        open={openLog}
        setOpen={setOpenLog}
        documentId={documentId}
      />
      <ShowDocumentOnly
        open={openDocument}
        setOpen={setOpenDocument}
        file={fileSelected}
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
            Are you sure you want to delete this Document?
          </p>
          <span className="text-gray-500">
            Deleting this Document will permanently delete all items within it.
            Make sure you have backed up important data before continuing..
          </span>
        </div>
      </Modal>
    </>
  );
}
