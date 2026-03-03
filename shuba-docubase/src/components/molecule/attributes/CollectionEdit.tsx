import { useEffect, useState } from "react";
import Breadcrumb from "../../atom/Breadcrumb";
import {
  Button,
  Checkbox,
  Col,
  DatePicker,
  Flex,
  Form,
  Input,
  message,
  Modal,
  Row,
  Space,
  Table,
} from "antd";
import {
  CheckOutlined,
  CloseOutlined,
  ExclamationCircleOutlined,
  MenuFoldOutlined,
  SaveOutlined,
  SearchOutlined,
} from "@ant-design/icons";
import { useNavigate, useParams } from "react-router-dom";
import { Trash2 } from "lucide-react";
import apiClient from "../../../services/apiClient";

const { TextArea } = Input;

export default function Index() {
  const navigate = useNavigate();
  const { uuid } = useParams();

  const [listAttributeExist, setListAttributeExist] = useState<any>([]);
  const [listAttribute, setListAttribute] = useState<any>([]);
  const [listAttributeAll, setListAttributeAll] = useState<any>([]);
  const [openConfirmAdd, setOpenConfirmAdd] = useState(false);
  const [confirmAddLoading, setConfirmAddLoading] = useState(false);
  const [search, setSearch] = useState<string>("");
  const [form] = Form.useForm();
  const columns = [
    {
      key: "attributeName",
      dataIndex: "attributeName",
      title: "Attribute Name",
      render: (items: any) => (
        <Space>
          <MenuFoldOutlined />
          <span className="font-bold">{items}</span>
        </Space>
      ),
    },
    {
      key: "attributeType",
      dataIndex: "attributeType",
      title: "Attribute Type",
    },
  ];

  const getData = async () => {
    try {
      const { data: detailRes } = await apiClient.get(
        `/attributecollections/get-by-id?id=${uuid}`
      );
      const attributeElementCollection = JSON.parse(
        detailRes.data.attributeElementCollection
      );

      setListAttributeExist(attributeElementCollection);
      form.setFieldsValue(detailRes.data);

      const { data: allAttrRes } = await apiClient.get(
        `/attribute?Page=1&Limit=10000`
      );

      const filteredAttributes = allAttrRes.data.data.filter(
        (a: any) => !attributeElementCollection.some((b: any) => b.id === a.id)
      );

      setListAttributeAll(filteredAttributes);
    } catch (err) {
      console.error(err);
    }
  };

  useEffect(() => {
    getData();
  }, []);

  useEffect(() => {
    setListAttribute(listAttributeAll);
  }, [listAttributeAll]);

  const handleSubmit = () => {
    if (listAttributeExist.length > 0) {
      setOpenConfirmAdd(true);
    } else {
      message.error("Please select at least one attribute");
    }
  };

  const handleOkAdd = async () => {
    form.validateFields().then((values) => {
      let updateAttr = {
        id: values.id,
        collectionName: values.collectionName,
        collectionDescription: values.collectionDescription || "",
        attributeElementCollection:
          listAttributeExist.length > 0
            ? JSON.stringify(listAttributeExist)
            : "",
      };
      console.log(updateAttr);
      setConfirmAddLoading(true);
      apiClient
        .put("/attributecollections", updateAttr)
        .then(({ data }) => {
          console.log(data);
          setConfirmAddLoading(false);
          setOpenConfirmAdd(false);
          navigate("/attributes");
          message.success("Update data is successful");
        })
        .catch((err) => {
          console.log(err);
          message.error(err.response.data.message);
          setConfirmAddLoading(false);
          setOpenConfirmAdd(false);
        });
    });
  };

  const handleClickRow = (row: any) => {
    setSearch("");
    const cleanData = { ...row };
    const attr = cleanData.attributeElement
      ? JSON.parse(cleanData.attributeElement)
      : {};
    if (attr.required === undefined) {
      attr.required = false;
      cleanData.attributeElement = JSON.stringify(attr);
    }
    setListAttributeAll((prev: any) =>
      prev.filter((i: any) => i.id != cleanData.id)
    );
    setListAttributeExist((prev: any) => [...prev, cleanData]);
  };

  const removeAttribute = (item: any) => {
    setSearch("");
    setListAttributeExist((prev: any) =>
      prev.filter((i: any) => i.id != item.id)
    );
    setListAttributeAll((prev: any) => [...prev, item]);
  };

  const handleKeyDown = (event: any) => {
    if (event.key === "Enter" || event.code === "Enter") {
      const str = search.length > 0 ? search.toLocaleLowerCase() : "";
      const dataFilter = listAttributeAll.filter((i: any) =>
        i.attributeName.toLocaleLowerCase().includes(str)
      );
      setListAttribute(dataFilter);
    }
  };

  const handleToggleRequired = (index: number) => {
    const updatedAttributes = [...listAttributeExist];
    const item = { ...updatedAttributes[index] };
    const attr = item.attributeElement ? JSON.parse(item.attributeElement) : {};
    attr.required = !attr.required;
    item.attributeElement = JSON.stringify(attr);
    updatedAttributes[index] = item;
    setListAttributeExist(updatedAttributes);
  };

  const DynamicAttributeRenderer = ({ fields }: any) => {
    return fields.map((item: any, index: number) => {
      const attr = item?.attributeElement
        ? JSON.parse(item.attributeElement)
        : {};
      return (
        <Form.Item
          key={index}
          name={attr?.name}
          label={
            <span>
              {attr?.label}{" "}
              <span className="text-slate-400 font-light italic">
                ({attr?.type})
              </span>
            </span>
          }
        >
          <Col>
            <Flex align="center">
              {(attr?.type === "text-field" || attr?.type === "number") && (
                <Input placeholder={attr?.placeholder || ""} disabled />
              )}
              {attr?.type === "text-area" && (
                <TextArea
                  rows={2}
                  placeholder={attr?.placeholder || ""}
                  disabled
                />
              )}
              {(attr?.type === "select" ||
                attr?.type === "checkbox" ||
                attr?.type === "radio") && (
                <Space size="large">
                  {attr?.options?.map((el: any) => (
                    <Input disabled value={el.opt} />
                  ))}
                </Space>
              )}
              {attr?.type === "date" && (
                <DatePicker
                  placeholder={attr?.format}
                  style={{ width: "100%" }}
                  disabled
                />
              )}
              <Button
                type="text"
                icon={<Trash2 size={18} />}
                onClick={() => removeAttribute(item)}
              />
            </Flex>
            <div className="my-2 flex gap-2">
              <Checkbox
                className="[&_.ant-checkbox-inner]:border-slate-800"
                checked={attr?.required}
                onChange={() => handleToggleRequired(index)}
              />
              <span className="font-normal text-slate-800">Required</span>
            </div>
          </Col>
        </Form.Item>
      );
    });
  };

  return (
    <>
      <div className="grid grid-cols-3 gap-4 mb-5">
        <div className="col-span-2 bg-white p-6 rounded-2xl h-fit">
          <Breadcrumb
            item={[
              {
                title: (
                  <span style={{ fontWeight: "bold" }}>Collection Edit</span>
                ),
              },
            ]}
          />
          <Form layout="vertical" form={form} onFinish={handleSubmit}>
            <Row gutter={[16, 16]} style={{ marginTop: 30 }}>
              <Col span={24}>
                <div style={{ padding: 24, paddingBottom: 10 }}>
                  <Form.Item name="id" hidden={true}>
                    <Input />
                  </Form.Item>
                  <Form.Item
                    label="Collection Name"
                    name="collectionName"
                    rules={[{ type: "string" }, { required: true }]}
                  >
                    <Input placeholder="enter collection name..." />
                  </Form.Item>
                  <Form.Item
                    label="Description"
                    name="collectionDescription"
                    rules={[{ type: "string" }]}
                  >
                    <TextArea rows={4} placeholder="enter description..." />
                  </Form.Item>
                  {listAttributeExist.length > 0 && (
                    <DynamicAttributeRenderer fields={listAttributeExist} />
                  )}
                </div>
              </Col>
              <Col span={24} style={{ display: "flex", justifyContent: "end" }}>
                <Space size="large">
                  <Button
                    icon={<CloseOutlined />}
                    iconPosition="end"
                    onClick={() => navigate("/attributes")}
                    color="danger"
                    variant="filled"
                  >
                    Cancel
                  </Button>
                  <Button
                    type="primary"
                    icon={<SaveOutlined />}
                    iconPosition="end"
                    htmlType="submit"
                  >
                    Save Changed
                  </Button>
                </Space>
              </Col>
            </Row>
          </Form>
        </div>
        <div className="col-span-1 bg-white p-6 rounded-2xl h-fit">
          <Row gutter={12}>
            <Col span={24}>
              <Input
                onChange={(e: any) => setSearch(e.target.value)}
                onKeyDown={handleKeyDown}
                size="large"
                value={search}
                placeholder="Search ..."
                prefix={<SearchOutlined />}
              />
            </Col>
            <Col span={24}>
              <p className="pt-5">Attributes</p>
              <Table
                pagination={false}
                columns={columns}
                dataSource={listAttribute}
                rowClassName="hover:cursor-pointer"
                rowKey={(record: any) => record.id}
                scroll={{ y: 450 }}
                onRow={(record, _) => {
                  return {
                    onClick: () => {
                      handleClickRow(record);
                    },
                  };
                }}
              />
            </Col>
          </Row>
        </div>
      </div>
      {/* modal confirm add */}
      <Modal
        title={
          <div style={{ paddingLeft: 24, paddingRight: 24, paddingTop: 24 }}>
            <ExclamationCircleOutlined
              style={{ fontSize: 30, color: "gold" }}
            />
          </div>
        }
        open={openConfirmAdd}
        onCancel={() => setOpenConfirmAdd(false)}
        maskClosable={false}
        footer={(_) => (
          <>
            <Row gutter={12}>
              <Col span={12}>
                <Button
                  icon={<CloseOutlined />}
                  iconPosition="end"
                  onClick={() => setOpenConfirmAdd(false)}
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
                  loading={confirmAddLoading}
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
          <p className="font-bold text-lg" style={{ marginBottom: 0 }}>
            Are you sure want to edit Collection?
          </p>
          <span className="text-gray-500">
            Make sure this collection suits your need.
          </span>
        </div>
      </Modal>
    </>
  );
}
