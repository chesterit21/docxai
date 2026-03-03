import { useEffect, useState } from "react";
import {
  Avatar,
  Button,
  Col,
  ConfigProvider,
  Divider,
  Empty,
  Flex,
  Form,
  Input,
  List,
  message,
  Modal,
  Row,
  Select,
  Space,
  Tag,
  Typography,
} from "antd";
import {
  CheckOutlined,
  ClockCircleOutlined,
  CloseOutlined,
  ExclamationCircleOutlined,
  PartitionOutlined,
  UsergroupAddOutlined,
  UserOutlined,
} from "@ant-design/icons";
import { Trash2, UserRoundCheck } from "lucide-react";
import apiClient from "../../../services/apiClient";
const { Text } = Typography;
const { TextArea } = Input;

export default function Index({
  open,
  setOpen,
  userExisting,
  document,
  rerenderData,
}: any) {
  const [userList, setUserList] = useState<any[]>([]);
  const [userExist, setUserExist] = useState<any[]>([]);
  const [userAddList, setUserAddList] = useState<any[]>([]);
  const [openConfirm, setOpenConfirm] = useState(false);
  const [confirmLoading, setConfirmLoading] = useState(false);
  const [openManage, setOpenManage] = useState<boolean>(false);
  const [optUserList, setOptUserList] = useState<any[]>([]);
  const [form] = Form.useForm();

  useEffect(() => {
    if (openManage) {
      getDataUser();
      setUserExist(userExisting?.approvalFlows || []);
      form.setFieldsValue(userExisting);
    }
  }, [openManage]);

  // const checkUserExist = () => {
  //     if(userExisting && Object.keys(userExisting).length > 0){
  //         const data = userExisting.approvalFlows.map((item: any) => {return {...item.user}})
  //         setUserExist(data)
  //     }
  // }

  useEffect(() => {
    if (userList.length > 0) {
      setOptUserList(
        userList.map((opt) => {
          return { value: opt.userID, label: opt.fullName };
        })
      );
    } else {
      setOptUserList([]);
    }
  }, [userList]);

  const getDataUser = async () => {
    try {
      const { data: res } = await apiClient.get(`/dropdown/ddlusers`);
      let filter =
        res?.data?.map((item: any) => {
          return { ...item, userID: item.userId };
        }) || [];
      if (userExisting && Object.keys(userExisting).length > 0) {
        filter = filter.filter(
          (a: any) =>
            !userExisting.approvalFlows.some(
              (b: any) => b.user.userID === a.userID
            )
        );
      }
      setUserList(filter);
    } catch (err) {
      console.error(err);
    }
  };

  const handleConfrim = async () => {
    const listUserAdd =
      userAddList?.map((items: any) => {
        return { userID: items.userID, step: 1 };
      }) || [];
    const listUserExisting =
      userExist.length > 0
        ? userExist.map((items: any) => {
            return { userID: items.user.userID, step: items.step };
          })
        : [];
    const joinUser = [...listUserExisting, ...listUserAdd].map(
      (item: any, index: number) => {
        return {
          ...item,
          step: index + 1,
        };
      }
    );
    const formFields = await form.getFieldsValue();
    const approval = {
      documentID: document.id,
      categoryID: document.categoryID,
      ...formFields,
    };
    const dataStore = { approval: approval, flows: joinUser };
    setConfirmLoading(true);
    await apiClient
      .post("/documents/add-workflow", dataStore)
      .then(({ data }) => {
        console.log(data);
        setConfirmLoading(false);
        setOpenConfirm(false);
        setOpenManage(false);
        setOpen(false);
        form.resetFields();
        rerenderData();
        message.success("Add workflow is successful");
      })
      .catch((err) => {
        console.log(err);
        message.error(err.response.data.message);
        setConfirmLoading(false);
        setOpenConfirm(false);
      });
  };

  // const handleDeleteExist = (item: any) => {
  //     console.log(item)
  //     let check = userExist.filter((i: any) => i.user.userID !== item.userID)
  //     console.log(check)

  //     setUserExist((prev: any) => prev.filter((i: any) => i.user.userID !== item.userID))
  //     setUserList((prev: any) => [...prev, item])
  // }

  const onChange = (val: any) => {
    let userAdded = userList
      .filter((i: any) => i.userID == val)
      .reduce((_, item) => {
        return item;
      }, {});
    setUserAddList((prev: any) => [...prev, userAdded]);
    setUserList((prev: any) => prev.filter((i: any) => i.userID != val));
  };

  const handleDeleteUserAdded = (item: any) => {
    setUserList((prev: any) => [...prev, item]);
    setUserAddList((prev: any) =>
      prev.filter((i: any) => i.userID !== item.userID)
    );
  };

  const handleSubmit = () => {
    // if (userAddList.length == 0) return message.error('Please select user to add to workflow')
    setOpenConfirm(true);
  };

  const handleAfterOpenChange = (visible: boolean) => {
    if (!visible) {
      setUserList([]);
      setUserAddList([]);
      form.resetFields();
    }
  };

  const handleDeletUserExist = async (item: any, approvalId: any) => {
    const dataStore = {
      approvalID: approvalId,
      userID: item.userID,
    };
    await apiClient
      .post("/documents/delete-workflow-user", dataStore)
      .then(({ data }) => {
        console.log(data);
        message.success("Delete user is successful");
        setUserExist((prev: any) =>
          prev.filter((i: any) => i.user.userID !== item.userID)
        );
        setUserList((prev: any) => [...prev, item]);
        rerenderData();
      })
      .catch((err) => {
        console.log(err);
        message.error(err.response.data.message);
      });
  };

  return (
    <>
      <Modal
        title={
          <div
            className="space-align-block"
            style={{ paddingLeft: 24, paddingRight: 24, paddingTop: 24 }}
          >
            <Space align="center" size="middle">
              <PartitionOutlined
                style={{
                  border: "1px solid #d9d9d9",
                  padding: 7,
                  fontSize: 30,
                  color: "#434343",
                  borderRadius: 7,
                }}
              />
              <Space.Compact direction="vertical">
                <span>Workflow</span>
                <Text type="secondary" style={{ fontWeight: "normal" }}>
                  This document will be sent to the user whose email you wrote
                  below.
                </Text>
              </Space.Compact>
            </Space>
          </div>
        }
        destroyOnHidden
        open={open}
        onCancel={() => setOpen(false)}
        maskClosable={false}
        footer={false}
        width={600}
        zIndex={10}
        styles={{
          content: {
            padding: 0,
          },
        }}
        style={{ top: 20 }}
      >
        <Divider style={{ marginBottom: 0 }} />
        <div className="grid grid-cols-1 px-5 py-5">
          <div className="col-span-1">
            <List
              itemLayout="horizontal"
              dataSource={userExisting?.approvalFlows || []}
              renderItem={(item: any) => (
                <List.Item style={{ borderBlockEnd: 0 }}>
                  <div className="flex flex-row justify-between items-center w-full">
                    <div className="flex gap-3">
                      <Avatar size="large" icon={<UserOutlined />} />
                      <Flex vertical>
                        <span className="font-bold">{item.user.fullName}</span>
                        <span style={{ textWrap: "nowrap" }}>
                          {item.user.email}
                        </span>
                      </Flex>
                    </div>
                    <div className="flex justify-between items-center">
                      <Tag
                        color={
                          item.approvalActivityCode == 2
                            ? "orange"
                            : item.approvalActivityCode == 3
                            ? "green"
                            : "magenta"
                        }
                        style={{ height: "100%", paddingInline: 10 }}
                      >
                        {item.approvalActivityCode == 2
                          ? "Pending Approval "
                          : item.approvalActivityCode == 3
                          ? "Approve "
                          : "Rejected "}
                        {item.approvalActivityCode == 2 ? (
                          <ClockCircleOutlined />
                        ) : item.approvalActivityCode == 3 ? (
                          <CheckOutlined />
                        ) : (
                          <ExclamationCircleOutlined />
                        )}
                      </Tag>
                    </div>
                  </div>
                </List.Item>
              )}
            />
          </div>
        </div>
        <Divider style={{ marginTop: 0, marginBottom: 0 }} />
        <div style={{ padding: 20, paddingBottom: 20, justifySelf: "end" }}>
          <Space>
            <Button
              icon={<CloseOutlined />}
              iconPosition="end"
              onClick={() => setOpen(false)}
              color="danger"
              variant="filled"
            >
              Cancel
            </Button>
            <Button
              type="primary"
              icon={<PartitionOutlined />}
              iconPosition="end"
              onClick={() => setOpenManage(true)}
            >
              Manage Workflow
            </Button>
          </Space>
        </div>
      </Modal>
      {/* modal manage */}
      <Modal
        title={
          <div
            className="space-align-block"
            style={{ paddingLeft: 24, paddingRight: 24, paddingTop: 24 }}
          >
            <Space align="center" size="middle">
              <PartitionOutlined
                style={{
                  border: "1px solid #d9d9d9",
                  padding: 7,
                  fontSize: 30,
                  color: "#434343",
                  borderRadius: 7,
                }}
              />
              <Space.Compact direction="vertical">
                <span>Workflow</span>
                <Text type="secondary" style={{ fontWeight: "normal" }}>
                  This document will be sent to the user whose email you wrote
                  below.
                </Text>
              </Space.Compact>
            </Space>
          </div>
        }
        destroyOnHidden
        afterOpenChange={handleAfterOpenChange}
        open={openManage}
        onCancel={() => setOpenManage(false)}
        maskClosable={false}
        footer={false}
        width={600}
        zIndex={20}
        style={{ top: 20 }}
        styles={{
          content: {
            padding: 0,
          },
        }}
      >
        <Form layout="vertical" form={form} onFinish={handleSubmit}>
          <Divider style={{ marginBottom: 0 }} />
          <div className="grid grid-cols-1 gap-y-7 px-5 py-5">
            <div className="col-span-1">
              <div className="text-[16px] font-bold text-slate-600 mb-3">
                Add Workflow
              </div>
              <Space.Compact style={{ width: "100%", marginBottom: 10 }}>
                <Select
                  showSearch
                  placeholder="Select a user"
                  style={{ width: "100%" }}
                  onSelect={onChange}
                  filterOption={(input, option) =>
                    (option?.label ?? "")
                      .toLowerCase()
                      .includes(input.toLowerCase())
                  }
                  options={optUserList}
                />
                <UsergroupAddOutlined
                  style={{
                    border: "1px solid #d9d9d9",
                    background: "#e7e7e7",
                    paddingLeft: 10,
                    paddingRight: 10,
                    fontSize: 20,
                    color: "#7c7c7c",
                    borderEndEndRadius: 5,
                    borderTopRightRadius: 5,
                  }}
                />
              </Space.Compact>
              <ConfigProvider
                renderEmpty={() => (
                  <Empty
                    image={Empty.PRESENTED_IMAGE_SIMPLE}
                    description="No users added"
                    styles={{
                      root: {
                        marginBlock: 0,
                      },
                    }}
                  />
                )}
              >
                <List
                  itemLayout="horizontal"
                  dataSource={userAddList}
                  renderItem={(item) => (
                    <List.Item>
                      <div className="flex flex-row justify-between items-center w-full">
                        <div className="flex px-2 items-center gap-3">
                          <UserRoundCheck
                            size={30}
                            color="#434343"
                            style={{
                              border: "1px solid #d9d9d9",
                              padding: 5,
                              borderRadius: 7,
                            }}
                          />
                          <div className="flex gap-3">
                            <Avatar size="large" icon={<UserOutlined />} />
                            <Flex vertical>
                              <span className="font-bold">{item.fullName}</span>
                              <span style={{ textWrap: "nowrap" }}>
                                {item.email}
                              </span>
                            </Flex>
                          </div>
                        </div>
                        <Button
                          type="text"
                          icon={<Trash2 size={18} color="#595959" />}
                          iconPosition="start"
                          onClick={() => handleDeleteUserAdded(item)}
                        />
                      </div>
                    </List.Item>
                  )}
                />
              </ConfigProvider>
            </div>
            <div className="col-span-1">
              <span className="text-[16px] font-bold text-slate-600 mb-0">
                User Existing
              </span>
              <ConfigProvider
                renderEmpty={() => (
                  <Empty
                    image={Empty.PRESENTED_IMAGE_SIMPLE}
                    description="No users exist"
                    styles={{
                      root: {
                        marginBlock: 0,
                      },
                    }}
                  />
                )}
              >
                <List
                  itemLayout="horizontal"
                  // dataSource={userExisting?.approvalFlows || []}
                  dataSource={userExist}
                  renderItem={(item: any) => (
                    <List.Item>
                      <div className="flex flex-row justify-between items-center w-full">
                        <div className="flex gap-3">
                          <Avatar size="large" icon={<UserOutlined />} />
                          <Flex vertical>
                            <span className="font-bold">
                              {item.user.fullName}
                            </span>
                            <span style={{ textWrap: "nowrap" }}>
                              {item.user.email}
                            </span>
                          </Flex>
                        </div>
                        <div className="flex justify-between items-center">
                          <Tag
                            color={
                              item.approvalActivityCode == 2
                                ? "orange"
                                : item.approvalActivityCode == 3
                                ? "green"
                                : "magenta"
                            }
                            style={{ height: "100%", paddingInline: 10 }}
                          >
                            {item.approvalActivityCode == 2
                              ? "Pending Approval "
                              : item.approvalActivityCode == 3
                              ? "Approve "
                              : "Rejected "}
                            {item.approvalActivityCode == 2 ? (
                              <ClockCircleOutlined />
                            ) : item.approvalActivityCode == 3 ? (
                              <CheckOutlined />
                            ) : (
                              <ExclamationCircleOutlined />
                            )}
                          </Tag>
                          <Button
                            type="text"
                            icon={<Trash2 size={18} color="#595959" />}
                            iconPosition="start"
                            onClick={() =>
                              handleDeletUserExist(item.user, item.approvalID)
                            }
                          />
                        </div>
                      </div>
                    </List.Item>
                  )}
                />
              </ConfigProvider>
            </div>
            {/* <div className="col-span-1">
                            <p className="text-[16px] font-bold text-slate-600 mb-10">File</p>
                            <Card styles={{body: { padding: 0 }}}>
                                {
                                    type == 'document' ?
                                    <Table pagination={false} columns={documentColumns} dataSource={fileList} />:
                                    <Table pagination={false} columns={categoryColumns} dataSource={categoryDummy} />
                                }
                            </Card>
                        </div> */}
            <div className="col-span-1">
              <Form.Item
                label="Note"
                name="notes"
                rules={[{ type: "string" }, { required: true }]}
              >
                <TextArea rows={4} placeholder="enter a note..." />
              </Form.Item>
            </div>
          </div>
          <Divider style={{ marginTop: 0, marginBottom: 0 }} />
          <div style={{ padding: 20, paddingBottom: 20, justifySelf: "end" }}>
            <Space>
              <Button
                icon={<CloseOutlined />}
                iconPosition="end"
                onClick={() => setOpenManage(false)}
                color="danger"
                variant="filled"
              >
                Cancel
              </Button>
              <Button
                type="primary"
                icon={<CheckOutlined />}
                iconPosition="end"
                htmlType="submit"
              >
                Submit
              </Button>
            </Space>
          </div>
        </Form>
      </Modal>
      {/* modal confirm share */}
      <Modal
        title={
          <div style={{ paddingLeft: 24, paddingRight: 24, paddingTop: 24 }}>
            <ExclamationCircleOutlined
              style={{ fontSize: 30, color: "gold" }}
            />
          </div>
        }
        open={openConfirm}
        onCancel={() => setOpenConfirm(false)}
        maskClosable={false}
        footer={(_) => (
          <>
            <Row gutter={12}>
              <Col span={12}>
                <Button
                  icon={<CloseOutlined />}
                  iconPosition="end"
                  onClick={() => setOpenConfirm(false)}
                  variant="filled"
                  block
                >
                  Back
                </Button>
              </Col>
              <Col span={12}>
                <Button
                  type="primary"
                  icon={<CheckOutlined />}
                  iconPosition="end"
                  onClick={handleConfrim}
                  block
                  loading={confirmLoading}
                >
                  Confirm
                </Button>
              </Col>
            </Row>
          </>
        )}
        width={400}
        zIndex={30}
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
            Do you want to send a workflow Document someone added to the list?
          </p>
          <span className="text-gray-500">
            The file you will be send to user on the list.
          </span>
        </div>
      </Modal>
    </>
  );
}
