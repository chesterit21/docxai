import { useEffect, useMemo, useRef, useState } from "react";
import Breadcrumb from "../../atom/Breadcrumb";
import {
  Avatar,
  Badge,
  Button,
  Card,
  Checkbox,
  Col,
  ConfigProvider,
  DatePicker,
  Empty,
  Flex,
  Form,
  Input,
  InputNumber,
  List,
  message,
  Modal,
  Popover,
  Radio,
  Row,
  Select,
  Skeleton,
  Space,
  Spin,
  Table,
  Tag,
  Tooltip,
} from "antd";
import {
  CheckOutlined,
  ClockCircleOutlined,
  CloseOutlined,
  ExclamationCircleOutlined,
  FormOutlined,
  MoreOutlined,
  PartitionOutlined,
  PlusOutlined,
  QuestionCircleOutlined,
  SaveOutlined,
  ShareAltOutlined,
  UserOutlined,
} from "@ant-design/icons";
import { Link, useNavigate, useParams } from "react-router-dom";
import { FileKey2, PencilIcon, ScanEye, Trash2 } from "lucide-react";
import LogDocument from "../../atom/ModalLogDocument";
import { AddRemainder, EditRemainder } from "../../atom/ModalRemainder";
import ModalShare from "../../atom/share/ModalShare";
import ModalWorkflow from "../../atom/workflow/ModalWorkflow";
import UploadNewVersion from "../../atom/upload/ModalUploadDocumentNew";
import ModalAttributeDocument from "../../atom/dynamicForm/ModalAddAttributeDocument";
import ModalAddAttributeExisting from "../../atom/dynamicForm/ModalAddAttributeExist";
import ShowDocument from "../../atom/document/ShowDocument";
import apiClient from "../../../services/apiClient";
import InfiniteScroll from "react-infinite-scroll-component";
import { useAuth } from "../../../context/AuthContext";
import debounce from "lodash/debounce";
import dayjs from "dayjs";
import customParseFormat from "dayjs/plugin/customParseFormat";
import utc from "dayjs/plugin/utc";
dayjs.extend(customParseFormat);
dayjs.extend(utc);

const { TextArea } = Input;
const { Option } = Select;

export default function Index() {
  const navigate = useNavigate();
  const { uuid: documentId } = useParams();
  const { config } = useAuth();
  const dateConfig =
    config?.dataGrid?.dateTimeFormat || "YYYY-MM-DD | HH:mm:ss";
  const [detailDocument, setDetailDocument] = useState<any>({});
  const [documentFiles, setDocumentFiles] = useState<any>([]);
  const [userShared, setUserShared] = useState<any>([]);
  const [userWorkflow, setUserWorkflow] = useState<any>({});
  const [fileSelected, setFileSelected] = useState<any>({});

  const [documentRemainder, setDocumentRemainder] = useState<any>([]);
  const [pageRemainder, setPageRemainder] = useState(1);
  const [totalRowRemainder, setTotalRowRemainder] = useState(0);
  const [loadingRemainder, setLoadingRemainder] = useState(false);
  const [eventDataRemainder, setEventDataRemainder] = useState<any>({});

  const [dataCollection, setDataCollection] = useState<any[]>([]);
  const [attributeSelected, setAttributeSelected] = useState<any>([]);
  const [optCollection, setOptCollection] = useState<any>([]);

  const [openConfirmEdit, setOpenConfirmEdit] = useState(false);

  const [confirmEditLoading, setConfirmEditLoading] = useState(false);
  const [openConfirmDelete, setOpenConfirmDelete] = useState(false);
  const [confirmDeleteLoading, setConfirmDeleteLoading] = useState(false);

  const [openAddRemainder, setOpenAddRemainder] = useState(false);
  const [openEditRemainder, setOpenEditRemainder] = useState(false);
  const [openDeleteRemainder, setOpenDeleteRemainder] = useState(false);
  const [confirmDeleteLoadingReminder, setConfirmDeleteLoadingReminder] =
    useState(false);

  const [openShare, setOpenShare] = useState(false);

  const [openWorkflow, setOpenWorkflow] = useState(false);
  const [openConfirmDeleteWorkflow, setOpenConfirmDeleteWorkflow] =
    useState(false);
  const [confirmDeleteLoadingWorkflow, setConfirmDeleteLoadingWorkflow] =
    useState(false);

  const [openUploadNew, setOpenUploadNew] = useState(false);
  const [openAddAttExist, setOpenAddAttExist] = useState(false);
  const [openAddAttribute, setOpenAddAttribute] = useState(false);
  const [openLog, setOpenLog] = useState(false);
  const [openDocument, setOpenDocument] = useState(false);

  const [formDocument] = Form.useForm();

  const [itemBreadcrumb, setItemBreadcrumb] = useState<any>([]);
  const [, setCategoryName] = useState<string>("");

  const [optListDocument, setOptListDocument] = useState<any[]>([]);
  const [fetching, setFetching] = useState(false);
  const inputRef = useRef<any>(null);
  const [watermarkOptions, setWatermarkOptions] = useState<any[]>([]);

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
      dataIndex: "updatedByUserName",
      key: "updatedByUserName",
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
          {row?.isMainDocumentFile ? (
            <Tag style={{ color: "blue" }}>main</Tag>
          ) : (
            <Tooltip placement="bottom" title="Set as main" color="#595959">
              <Button
                type="text"
                icon={<FileKey2 size={18} />}
                iconPosition="end"
                onClick={() => handleSetMainFile(row.id)}
              />
            </Tooltip>
          )}
          <Tooltip placement="bottom" title="Edit" color="#595959">
            <Button
              type="text"
              icon={<PencilIcon size={18} />}
              iconPosition="end"
              onClick={() => handleClickShow(items)}
            />
          </Tooltip>
          <Tooltip placement="bottom" title="Delete" color="#595959">
            <Button
              type="text"
              icon={<Trash2 size={18} />}
              iconPosition="end"
              onClick={() => handleClickDeleteFile(items)}
            />
          </Tooltip>
        </Space>
      ),
    },
  ];

  useEffect(() => {
    if (inputRef.current) {
      inputRef.current.focus();
    }
    window.scrollTo({
      top: 0,
      behavior: "smooth",
    });
  }, []);

  useEffect(() => {
    getDetailDocument();
    getDataCollection();
    getDocumentRemainder();
    fetchWatermarks();
    getListCompany(""); // Load related documents dropdown
  }, [documentId]);

  const fetchWatermarks = async () => {
    try {
      const { data } = await apiClient.get("/dropdown/ddlwatermarks");
      setWatermarkOptions(data?.data || []);
    } catch (err) {
      console.error("Error fetching watermarks:", err);
    }
  };

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

  useEffect(() => {
    const changeOptReletedDocument =
      detailDocument?.documentRelated?.map((item: any) => ({
        label: item.text,
        value: item.value,
      })) || [];
    const dataDetail = {
      ...detailDocument,
      documentRelated: changeOptReletedDocument,
      watermarkID: detailDocument?.watermarkID || undefined,
    };
    if (dataDetail.documentAttributes) {
      setAttributeSelected(
        JSON.parse(dataDetail.documentAttributes.attributeValues)
      );
      const attrValueMap = JSON.parse(
        dataDetail.documentAttributes.attributeValues
      ).reduce((acc: any, item: any) => {
        const attrName = JSON.parse(item.attributeElement);
        acc[attrName.name] =
          attrName.type !== "date" ? item.value : dayjs.utc(item.value);
        return acc;
      }, {});
      formDocument.setFieldsValue({ ...dataDetail, ...attrValueMap });
    } else {
      formDocument.setFieldsValue(dataDetail);
    }
  }, [detailDocument]);

  useEffect(() => {
    if (dataCollection.length > 0) {
      setOptCollection(
        dataCollection.map((opt: any) => {
          return { value: opt.id, label: opt.collectionName };
        })
      );
    } else {
      setOptCollection([]);
    }
  }, [dataCollection]);

  const getDetailDocument = () => {
    setItemBreadcrumb([]);
    apiClient
      .get(`/documents/get-document-dtl?id=${documentId}&mode=edit`)
      .then(({ data }) => {
        setDetailDocument(data?.data);
        setDocumentFiles(data?.data?.documentFiles || []);
        setUserShared(data?.data?.sharedTo?.sharedUser || []);
        setUserWorkflow(data?.data?.workflow || []);
      })
      .catch((err) => {
        console.log(err);
      });
  };

  const getDocumentRemainder = async () => {
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

  const getDataCollection = () => {
    apiClient
      .get(`/attributecollections?Page=1&Limit=1000`)
      .then(({ data }) => {
        setDataCollection(data.data.data);
      })
      .catch((err) => {
        console.log(err);
      });
  };

  const getListCompany = async (search: string) => {
    setFetching(true);
    try {
      const excludedIds = documentId ? [documentId] : [];
      const res = await apiClient.get(
        `/dropdown/ddldocuments?DocumentTitle=${search}&excludedIds=${excludedIds.join(
          ","
        )}`
      );
      const data = (await res?.data?.data) || [];
      const formatted = data.map((item: any) => ({
        label: item.text,
        value: item.value,
      }));
      setOptListDocument(formatted);
    } catch (err) {
      console.error(err);
      setOptListDocument([]);
    }
    setFetching(false);
  };

  const debounceFetcher = useMemo(() => {
    return debounce((value: string) => {
      getListCompany(value);
    }, 500);
  }, []);

  const handleSelectCollection = (value: number) => {
    let dd = dataCollection
      .filter((item: any) => item.id == value)
      .reduce((_: any, item: any) => {
        return JSON.parse(item.attributeElementCollection);
      }, {});
    setAttributeSelected((prev: any) => [...prev, ...dd]);
  };

  const handleSubmit = async () => {
    await formDocument.validateFields();
    setOpenConfirmEdit(true);
  };

  const handleOkAdd = async () => {
    const mainValues = await formDocument.getFieldsValue();
    const reletedDocument =
      mainValues?.documentRelated?.map((item: any) => item.value) || [];

    const documentAttributes = attributeSelected.map((attr: any) => {
      const {
        insertedAt,
        insertedBy,
        insertedByFullName,
        insertedByUserName,
        isActive,
        updatedAt,
        updatedBy,
        updatedByFullName,
        updatedByUserName,
        ...attrFiltered
      } = attr;
      const schema = JSON.parse(attrFiltered.attributeElement);
      let value = mainValues[schema.name];
      if (schema.type === "date" && value) {
        value = dayjs.utc(dayjs.utc(value).format("YYYY-MM-DD"));
      }
      return {
        ...attrFiltered,
        value,
      };
    });

    const attrString = JSON.stringify(documentAttributes);
    const dataUpdate = {
      ...mainValues,
      relatedDocumentIDs: reletedDocument,
      documentAttributes: attrString,
    };
    if (mainValues.watermarkID) {
      dataUpdate.watermarkID = mainValues.watermarkID;
    }
    setConfirmEditLoading(true);
    apiClient
      .put("/documents", dataUpdate)
      .then(({ data }) => {
        console.log(data);
        setConfirmEditLoading(false);
        setOpenConfirmEdit(false);
        message.success("Update data is successful");
        navigate(`/document/document-view/${documentId}`);
      })
      .catch((err) => {
        console.log(err);
        message.error(err.response.data.message);
        setConfirmEditLoading(false);
        setOpenConfirmEdit(false);
      });
  };

  const removeAttribute = (id: any) => {
    const attrToRemove = attributeSelected.find((a: any) => a.id === id);
    if (attrToRemove) {
      const attributeName = attrToRemove.attributeName;
      const isSystem = attrToRemove.isSystem;

      if (
        (attributeName === "Expired Date" ||
          attributeName === "Days Of Reminder") &&
        isSystem
      ) {
        const otherAttrName =
          attributeName === "Expired Date"
            ? "Days Of Reminder"
            : "Expired Date";
        setAttributeSelected((prev: any) =>
          prev.filter((i: any) => {
            if (i.id === id) return false;
            if (i.attributeName === otherAttrName && i.isSystem) return false;
            return true;
          })
        );
        return;
      }
    }
    setAttributeSelected((prev: any) => prev.filter((i: any) => i.id != id));
  };

  const handleClickShow = (value: any) => {
    setFileSelected(value);
    setOpenDocument(true);
  };

  const handleClickDeleteFile = (value: any) => {
    setFileSelected(value);
    setOpenConfirmDelete(true);
  };

  const handleSetMainFile = (id: number) => {
    const dataUpdate = {
      documentID: Number(documentId),
      documentFileID: id,
      isMainDocumentFile: true,
    };
    apiClient
      .post("/documents/set-main-file-document", dataUpdate)
      .then(({ data }) => {
        console.log(data);
        message.success("Update data is successful");
        getDetailDocument();
      })
      .catch((err) => {
        console.log(err);
        message.error(err.response.data.message);
      });
  };

  const handleOkDeleteWorkflow = () => {
    setConfirmDeleteLoadingWorkflow(true);
    apiClient
      .delete(`/documents/delete-workflow?documentId=${documentId}`)
      .then(({ data }) => {
        console.log(data);
        message.success("Delete workflow is successful");
        setConfirmDeleteLoadingWorkflow(false);
        setOpenConfirmDeleteWorkflow(false);
        getDetailDocument();
      })
      .catch((err) => {
        console.log(err);
        message.error(err.response.data.message);
        setConfirmDeleteLoadingWorkflow(false);
        setOpenConfirmDeleteWorkflow(false);
      });
  };

  const handleOkDeleteFile = () => {
    setConfirmDeleteLoading(true);
    console.log(fileSelected);
    apiClient
      .put(`/documentfiles/delete?id=${fileSelected.id}`)
      .then(({ data }) => {
        console.log(data);
        message.success("Delete file is successful");
        setConfirmDeleteLoading(false);
        setOpenConfirmDelete(false);
        getDetailDocument();
        setFileSelected({});
      })
      .catch((err) => {
        console.log(err);
        message.error(err?.response?.data?.message || "Something went wrong");
        setConfirmDeleteLoading(false);
        setOpenConfirmDelete(false);
        setFileSelected({});
      });
  };

  const handleOkDeleteReminder = async () => {
    setConfirmDeleteLoadingReminder(true);
    apiClient
      .put(`/documentreminder/delete?id=${eventDataRemainder.id}`)
      .then(({ data }) => {
        console.log(data);
        message.success("Delete reminder is successful");
        setConfirmDeleteLoadingReminder(false);
        setOpenDeleteRemainder(false);
        navigate(0);
      })
      .catch((err) => {
        console.log(err);
        message.error(err.response.data.message);
        setConfirmDeleteLoadingReminder(false);
        setOpenDeleteRemainder(false);
      });
  };

  const dynamicAttributeRenderer = (fields: any, id: number, idx: number) => {
    const lable =
      fields?.helptext?.length > 0 ? (
        <Badge
          count={
            <Tooltip placement="top" title={fields.helptext} color="#595959">
              <QuestionCircleOutlined
                style={{ color: "#8c8c8c", marginRight: -10, marginTop: 4 }}
              />
            </Tooltip>
          }
        >
          {fields.label}
        </Badge>
      ) : (
        fields.label
      );

    const commonProps = {
      name: fields.name,
      label: lable,
      rules: fields.required ? [{ required: true }] : [],
    };

    switch (fields.type) {
      case "text-field":
        return (
          <div key={idx} className="flex items-center gap-3">
            <div className="flex-auto">
              <Form.Item key={idx} {...commonProps}>
                <Input placeholder={fields?.placeholder || ""} />
              </Form.Item>
            </div>
            <Button
              type="text"
              icon={<Trash2 size={18} />}
              onClick={() => removeAttribute(id)}
            />
          </div>
        );
      case "number":
        return (
          <div key={idx} className="flex items-center gap-3">
            <div className="flex-auto">
              <Form.Item key={idx} {...commonProps}>
                <InputNumber
                  style={{ width: "100%" }}
                  min={fields.min}
                  max={fields.max}
                  placeholder={fields?.placeholder || ""}
                />
              </Form.Item>
            </div>
            <Button
              type="text"
              icon={<Trash2 size={18} />}
              onClick={() => removeAttribute(id)}
            />
          </div>
        );
      case "text-area":
        return (
          <div key={idx} className="flex items-center gap-3">
            <div className="flex-auto">
              <Form.Item key={idx} {...commonProps}>
                <TextArea rows={2} placeholder={fields?.placeholder || ""} />
              </Form.Item>
            </div>
            <Button
              type="text"
              icon={<Trash2 size={18} />}
              onClick={() => removeAttribute(id)}
            />
          </div>
        );
      case "select":
        return (
          <div key={idx} className="flex items-center gap-3">
            <div className="flex-auto">
              <Form.Item key={idx} {...commonProps}>
                <Select placeholder={fields.placeholder}>
                  {fields.options?.map((opt: any, idx: any) => (
                    <Option key={idx} value={opt.opt}>
                      {opt.opt}
                    </Option>
                  ))}
                  //{" "}
                </Select>
              </Form.Item>
            </div>
            <Button
              type="text"
              icon={<Trash2 size={18} />}
              onClick={() => removeAttribute(id)}
            />
          </div>
        );
      case "date":
        return (
          <div key={idx} className="flex items-center gap-3">
            <div className="flex-auto">
              <Form.Item key={idx} {...commonProps}>
                <DatePicker
                  placeholder={fields.format}
                  style={{ width: "100%" }}
                />
              </Form.Item>
            </div>
            <Button
              type="text"
              icon={<Trash2 size={18} />}
              onClick={() => removeAttribute(id)}
            />
          </div>
        );
      case "checkbox":
        return (
          <div key={idx} className="flex items-center gap-3">
            <div className="flex-auto">
              <Form.Item key={idx} {...commonProps}>
                <Checkbox.Group>
                  {fields.options?.map((opt: any, idx: any) => (
                    <Checkbox key={idx} value={opt.opt}>
                      {opt.opt}
                    </Checkbox>
                  ))}
                </Checkbox.Group>
              </Form.Item>
            </div>
            <Button
              type="text"
              icon={<Trash2 size={18} />}
              onClick={() => removeAttribute(id)}
            />
          </div>
        );
      case "radio":
        return (
          <div key={idx} className="flex items-center gap-3">
            <div className="flex-auto">
              <Form.Item key={idx} {...commonProps}>
                <Radio.Group>
                  {fields.options?.map((opt: any, idx: any) => (
                    <Radio key={idx} value={opt.opt}>
                      {opt.opt}
                    </Radio>
                  ))}
                </Radio.Group>
              </Form.Item>
            </div>
            <Button
              type="text"
              icon={<Trash2 size={18} />}
              onClick={() => removeAttribute(id)}
            />
          </div>
        );
      default:
        return null;
    }
  };

  return (
    <>
      <div className="grid grid-cols-3 gap-4 mb-5">
        <div className="col-span-2 p-6 bg-white rounded-2xl">
          <div className="flex justify-between">
            <Breadcrumb item={itemBreadcrumb} />
            <span className="font-bold text-blue-600">Edit</span>
          </div>
          <Form
            layout="vertical"
            form={formDocument}
          // onFinish={handleSubmit}
          >
            <Row gutter={[16, 16]}>
              <Col span={24}>
                <div className="pt-3 pb-7">
                  <Form.Item name="id" hidden>
                    <Input />
                  </Form.Item>
                  <Form.Item name="categoryID" hidden>
                    <Input />
                  </Form.Item>
                  <Form.Item
                    label="Title"
                    name="documentTitle"
                    rules={[
                      { type: "string" },
                      { required: true, message: "required" },
                    ]}
                  >
                    <Input ref={inputRef} placeholder="enter title..." />
                  </Form.Item>
                  <Form.Item
                    label="Description"
                    name="documentDesc"
                    rules={[
                      { type: "string" },
                      { required: true, message: "required" },
                    ]}
                  >
                    <TextArea rows={4} placeholder="enter description..." />
                  </Form.Item>
                  <Form.Item label="Watermark" name="watermarkID">
                    <Select
                      placeholder="Select watermark (optional)"
                      allowClear
                      options={watermarkOptions.map((opt: any) => ({
                        value: opt.value,
                        label: opt.text,
                      }))}
                    />
                  </Form.Item>
                  <Form.Item label="Collection & Attribute">
                    <span>
                      To add more detailed information about this document, you
                      can select from an existing set of attributes
                      (collections) or create a new attribute.
                    </span>
                    <div className="flex justify-between gap-5 pt-5">
                      <div className="w-[60%]">
                        <Select
                          placeholder="Select collection"
                          options={optCollection}
                          onSelect={handleSelectCollection}
                          disabled={attributeSelected.length > 0 ? true : false}
                        />
                      </div>
                      <div className="w-[40%]">
                        <Popover
                          zIndex={1}
                          trigger="click"
                          placement="bottom"
                          content={
                            <Space size="large">
                              <Button
                                variant="solid"
                                color="purple"
                                onClick={() => setOpenAddAttExist(true)}
                              >
                                Add Existing
                              </Button>
                              <Button
                                variant="solid"
                                color="purple"
                                onClick={() => setOpenAddAttribute(true)}
                              >
                                Add New
                              </Button>
                            </Space>
                          }
                        >
                          <Button
                            block
                            variant="solid"
                            color="purple"
                            icon={<PlusOutlined />}
                            iconPosition="end"
                          >
                            Add Existing or New Attribute
                          </Button>
                        </Popover>
                      </div>
                    </div>
                  </Form.Item>
                  <div className="w-2/4">
                    {attributeSelected.length > 0 &&
                      attributeSelected.map((item: any, index: number) =>
                        dynamicAttributeRenderer(
                          JSON.parse(item.attributeElement),
                          item.id,
                          index
                        )
                      )}
                  </div>
                  <Form.Item
                    label="Related Document"
                    name="documentRelated"
                    rules={[{ type: "array" }]}
                  >
                    <Select
                      mode="multiple"
                      showSearch
                      labelInValue
                      placeholder="Select document by search..."
                      filterOption={false}
                      notFoundContent={
                        fetching ? <Spin size="small" /> : "No results found"
                      }
                      onSearch={debounceFetcher}
                      options={optListDocument}
                    />
                  </Form.Item>
                  <div className="flex items-center justify-between mb-5">
                    <span>Files</span>
                    <Button
                      variant="solid"
                      color="purple"
                      icon={<PlusOutlined />}
                      iconPosition="end"
                      onClick={() => setOpenUploadNew(true)}
                    >
                      Add New File
                    </Button>
                  </div>
                  <Card styles={{ body: { padding: 0 } }}>
                    <Table
                      rowKey="id"
                      pagination={false}
                      scroll={{ x: true }}
                      columns={fileColumns}
                      dataSource={documentFiles}
                    />
                  </Card>
                </div>
              </Col>
              <Col span={24} style={{ display: "flex", justifyContent: "end" }}>
                <Space size="large">
                  <Button
                    icon={<CloseOutlined />}
                    iconPosition="end"
                    onClick={() => navigate(-1)}
                    color="danger"
                    variant="filled"
                  >
                    Cancel
                  </Button>
                  <Button
                    type="primary"
                    icon={<SaveOutlined />}
                    iconPosition="end"
                    onClick={handleSubmit}
                  >
                    Save Changed
                  </Button>
                </Space>
              </Col>
            </Row>
          </Form>
        </div>
        <div className="col-span-1 p-6 bg-white rounded-2xl h-fit">
          <Row gutter={[15, 24]}>
            <Col span={24}>
              <Card
                styles={{ body: { padding: 0 } }}
                style={{ border: "none" }}
              >
                <p>Reminders:</p>
                <div className="flex flex-col gap-3">
                  <div id="scrollableDiv" className="overflow-y-auto max-h-52">
                    <InfiniteScroll
                      dataLength={documentRemainder.length}
                      next={getDocumentRemainder}
                      hasMore={documentRemainder.length < totalRowRemainder}
                      loader={
                        <Skeleton avatar paragraph={{ rows: 1 }} active />
                      }
                      // endMessage={<Divider plain>It is all, nothing more 🤐</Divider>}
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
                                <Flex
                                  justify="space-between"
                                  align="flex-start"
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
                                  <Popover
                                    placement="bottomLeft"
                                    arrow={false}
                                    zIndex={100}
                                    content={
                                      <div className="flex flex-col items-start gap-1">
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
                                          onClick={() => [
                                            setEventDataRemainder(item),
                                            setOpenEditRemainder(true),
                                          ]}
                                        >
                                          Edit
                                        </Button>
                                        <Button
                                          type="text"
                                          icon={
                                            <Trash2 size={18} color="#595959" />
                                          }
                                          iconPosition="start"
                                          onClick={() => [
                                            setEventDataRemainder(item),
                                            setOpenDeleteRemainder(true),
                                          ]}
                                        >
                                          Delete
                                        </Button>
                                      </div>
                                    }
                                  >
                                    <Button
                                      type="text"
                                      icon={<MoreOutlined />}
                                    />
                                  </Popover>
                                </Flex>
                              </Card>
                            </List.Item>
                          )}
                        />
                      </ConfigProvider>
                    </InfiniteScroll>
                  </div>
                  <Button
                    variant="solid"
                    color="purple"
                    icon={<ClockCircleOutlined />}
                    iconPosition="end"
                    block
                    onClick={() => setOpenAddRemainder(true)}
                  >
                    Add Reminder
                  </Button>
                </div>
              </Card>
            </Col>
            <Col span={24}>
              <p>Shared to</p>
              <div className="flex flex-col gap-3">
                {userShared.length > 0 && (
                  <Avatar.Group
                    max={{
                      count: 5,
                      style: { color: "#f56a00", backgroundColor: "#fde3cf" },
                    }}
                  >
                    {userShared.map((item: any, index: number) => (
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
                <Button
                  variant="solid"
                  color="purple"
                  icon={<ShareAltOutlined />}
                  iconPosition="end"
                  block
                  onClick={() => setOpenShare(true)}
                >
                  Share
                </Button>
              </div>
            </Col>
            <Col span={24}>
              <p>Workflow</p>
              <div className="flex flex-col gap-3">
                {Object.keys(userWorkflow).length > 0 && (
                  <Avatar.Group
                    max={{
                      count: 5,
                      style: { color: "#f56a00", backgroundColor: "#fde3cf" },
                    }}
                  >
                    {userWorkflow.approvalFlows.map(
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
                <div className="flex justify-between gap-3">
                  <Button
                    variant="solid"
                    color="purple"
                    icon={<PartitionOutlined />}
                    iconPosition="end"
                    block
                    onClick={() => setOpenWorkflow(true)}
                  >
                    Add Workflow
                  </Button>
                  {Object.keys(userWorkflow).length > 0 && (
                    <Button
                      icon={<Trash2 />}
                      iconPosition="end"
                      color="danger"
                      variant="filled"
                      block
                      onClick={() => setOpenConfirmDeleteWorkflow(true)}
                    >
                      Delete Workflow
                    </Button>
                  )}
                </div>
              </div>
            </Col>
            <Col span={24}>
              <p>Audit Log</p>
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
            </Col>
          </Row>
        </div>
      </div>
      <AddRemainder
        open={openAddRemainder}
        setOpen={setOpenAddRemainder}
        documentId={documentId}
      />
      <EditRemainder
        open={openEditRemainder}
        setOpen={setOpenEditRemainder}
        eventData={eventDataRemainder}
        setEventDataRemainder={setEventDataRemainder}
        documentId={documentId}
      />
      <ModalWorkflow
        open={openWorkflow}
        setOpen={setOpenWorkflow}
        userExisting={userWorkflow}
        document={detailDocument}
        rerenderData={getDetailDocument}
      />
      <ModalShare
        open={openShare}
        setOpen={setOpenShare}
        userExisting={userShared}
        document={detailDocument}
        rerenderData={getDetailDocument}
      />
      <UploadNewVersion
        open={openUploadNew}
        setOpen={setOpenUploadNew}
        documentId={documentId}
        rerenderData={getDetailDocument}
      />
      <ModalAttributeDocument
        open={openAddAttribute}
        setOpen={setOpenAddAttribute}
        setData={setAttributeSelected}
      />
      <ModalAddAttributeExisting
        open={openAddAttExist}
        setOpen={setOpenAddAttExist}
        attributeExisting={attributeSelected}
        setAttributeExisting={setAttributeSelected}
      />
      <LogDocument
        open={openLog}
        setOpen={setOpenLog}
        documentId={documentId}
      />

      <ShowDocument
        open={openDocument}
        setOpen={setOpenDocument}
        file={fileSelected}
      />
      {/* modal confirm edit */}
      <Modal
        title={
          <div style={{ paddingLeft: 24, paddingRight: 24, paddingTop: 24 }}>
            <ExclamationCircleOutlined
              style={{ fontSize: 30, color: "gold" }}
            />
          </div>
        }
        open={openConfirmEdit}
        onCancel={() => setOpenConfirmEdit(false)}
        maskClosable={false}
        footer={(_) => (
          <>
            <Row gutter={12}>
              <Col span={12}>
                <Button
                  icon={<CloseOutlined />}
                  iconPosition="end"
                  onClick={() => setOpenConfirmEdit(false)}
                  variant="filled"
                  block
                >
                  Cancel
                </Button>
              </Col>
              <Col span={12}>
                <Button
                  type="primary"
                  icon={<CheckOutlined />}
                  iconPosition="end"
                  onClick={handleOkAdd}
                  block
                  loading={confirmEditLoading}
                >
                  Confirm
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
          <p className="text-lg font-bold" style={{ marginBottom: 0 }}>
            Are you sure want to edit Document?
          </p>
          <span className="text-gray-500">
            Make sure this document suits your need.
          </span>
        </div>
      </Modal>
      {/* modal confirm delete file*/}
      <Modal
        title={
          <div style={{ paddingLeft: 24, paddingRight: 24, paddingTop: 24 }}>
            <ExclamationCircleOutlined
              style={{ fontSize: 30, color: "gold" }}
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
                  variant="filled"
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
                  onClick={handleOkDeleteFile}
                  iconPosition="end"
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
          <p className="text-lg font-bold" style={{ marginBottom: 0 }}>
            Do you want to delete this file?
          </p>
          <span className="text-gray-500">
            Deleting this file will permanently delete from this document. Make
            sure you have backed up important data before continuing..
          </span>
        </div>
      </Modal>
      {/* modal confirm delete workflow */}
      <Modal
        title={
          <div style={{ paddingLeft: 24, paddingRight: 24, paddingTop: 24 }}>
            <ExclamationCircleOutlined
              style={{ fontSize: 30, color: "#f5222d" }}
            />
          </div>
        }
        open={openConfirmDeleteWorkflow}
        onCancel={() => setOpenConfirmDeleteWorkflow(false)}
        maskClosable={false}
        footer={(_) => (
          <>
            <Row gutter={12}>
              <Col span={12}>
                <Button
                  icon={<CloseOutlined />}
                  iconPosition="end"
                  onClick={() => setOpenConfirmDeleteWorkflow(false)}
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
                  onClick={handleOkDeleteWorkflow}
                  loading={confirmDeleteLoadingWorkflow}
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
          <p className="text-lg font-bold" style={{ marginBottom: 0 }}>
            Are you sure you want to delete workflow this Document?
          </p>
          <span className="text-gray-500">
            Click button delete to remove workflow
          </span>
        </div>
      </Modal>
      {/* modal confirm delete reminder */}
      <Modal
        title={
          <div style={{ paddingLeft: 24, paddingRight: 24, paddingTop: 24 }}>
            <ExclamationCircleOutlined
              style={{ fontSize: 30, color: "#f5222d" }}
            />
          </div>
        }
        open={openDeleteRemainder}
        onCancel={() => {
          setOpenDeleteRemainder(false);
          setEventDataRemainder({});
        }}
        maskClosable={false}
        footer={(_) => (
          <>
            <Row gutter={12}>
              <Col span={12}>
                <Button
                  icon={<CloseOutlined />}
                  iconPosition="end"
                  onClick={() => setOpenDeleteRemainder(false)}
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
                  onClick={handleOkDeleteReminder}
                  loading={confirmDeleteLoadingReminder}
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
          <p className="text-lg font-bold" style={{ marginBottom: 0 }}>
            Are you sure you want to delete reminder?
          </p>
          <span className="text-gray-500">
            Click button delete to remove reminder
          </span>
        </div>
      </Modal>
    </>
  );
}
