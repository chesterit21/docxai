import { useEffect, useState } from "react";
import {
  Avatar,
  Button,
  Col,
  Divider,
  Flex,
  Input,
  List,
  message,
  Modal,
  Row,
  Select,
  Space,
  Typography,
} from "antd";
import {
  CheckOutlined,
  CloseOutlined,
  ExclamationCircleOutlined,
  SearchOutlined,
  ShareAltOutlined,
  UsergroupAddOutlined,
  UserOutlined,
} from "@ant-design/icons";
import { Trash2, UserPlus } from "lucide-react";
import apiClient from "../../../services/apiClient";
const { Text } = Typography;

const privillege: any = {
  view: {
    isView: true,
    isEdit: false,
    isDelete: false,
  },
  editor: {
    isView: true,
    isEdit: true,
    isDelete: false,
  },
  full: {
    isView: true,
    isEdit: true,
    isDelete: true,
  },
};

export default function Index({
  open,
  setOpen,
  userExisting,
  document,
  rerenderData,
}: any) {
  const [userListAll, setUserListAll] = useState<any[]>([]); // for list all user
  const [userList, setUserList] = useState<any[]>([]); //for list user include filter data
  const [userShareListAll, setUserShareListAll] = useState<any[]>([]); //for list all user shared existing and user sharer added
  const [userShareList, setUserShareList] = useState<any[]>([]); //for list all user shared existing and user sharer added include filter
  const [openConfirm, setOpenConfirm] = useState(false);
  const [confirmLoading, setConfirmLoading] = useState(false);

  // check if open true and list user empty then get list user
  useEffect(() => {
    if (open) {
      transformUserExisting();
      getDataUser();
    }
  }, [open]);

  // set list user from list user all
  useEffect(() => {
    setUserList(userListAll);
  }, [userListAll]);

  // set list user shared from list user shared all
  useEffect(() => {
    setUserShareList(userShareListAll);
  }, [userShareListAll]);

  const transformUserExisting = async () => {
    if (Array.isArray(userExisting) && userExisting.length > 0) {
      const data = userExisting.map((item: any) => {
        let priv = "";
        if (item.isView && item.isEdit && item.isDelete) {
          priv = "full";
        } else if (item.isView && item.isEdit && !item.isDelete) {
          priv = "editor";
        } else if (item.isView && !item.isEdit && !item.isDelete) {
          priv = "view";
        }
        return { ...item, type: item.shareType, priv: priv };
      });
      setUserShareListAll(data);
    }
  };

  // get list user
  const getDataUser = async () => {
    try {
      const { data: res } = await apiClient.get(`/dropdown/ddlusersandgroups`);
      let filter =
        res?.data?.map((item: any) => {
          return { ...item, userID: item.userId, groupID: item.groupId };
        }) || [];

      if (Array.isArray(userExisting) && userExisting.length > 0) {
        filter = filter.filter(
          (a: any) =>
            !userExisting.some(
              (b: any) => b.userID === a.userID || b.groupID === a.groupID
            )
        );
      }

      setUserListAll(filter);
    } catch (err) {
      console.error(err);
    }
  };

  const handleSubmit = () => {
    // if(userShareListAll.length == 0) return message.error('Please select at least one user!')
    setOpenConfirm(true);
  };

  const handleConfrim = async () => {
    const listUser = userShareListAll.map((items: any) => {
      const data =
        items.type == "user"
          ? {
              shareType: items.type,
              userID: items.userID,
              groupID: null,
              ...privillege[items.priv],
            }
          : {
              shareType: items.type,
              userID: null,
              groupID: items.groupID,
              ...privillege[items.priv],
            };
      return data;
    });
    const dataStore = {
      documentID: document?.id,
      requestUserDocPrivillege: listUser,
    };

    console.log(dataStore);
    setConfirmLoading(true);
    await apiClient
      .post("/documents/add-document-shared", dataStore)
      .then(({ data }) => {
        console.log(data);
        setConfirmLoading(false);
        setOpenConfirm(false);
        setOpen(false);
        setUserShareListAll([]);
        rerenderData();
        message.success("Share document is successful");
      })
      .catch((err) => {
        console.log(err);
        message.error(err.response.data.message);
        setConfirmLoading(false);
        setOpenConfirm(false);
      });
  };

  const handleAdd = (row: any) => {
    const dataAdd = { ...row, priv: row?.priv ? row.priv : "view" };
    setUserListAll((prev: any) =>
      prev.filter(
        (i: any) =>
          (i.type == "user" ? i.userID : i.groupID) !==
          (i.type == "user" ? dataAdd.userID : dataAdd.groupID)
      )
    );
    setUserShareListAll((prev: any) => [...prev, dataAdd]);
  };

  const handleDelete = (item: any) => {
    setUserShareListAll((prev: any) =>
      prev.filter(
        (i: any) =>
          (i.type == "user" ? i.userID : i.groupID) !==
          (i.type == "user" ? item.userID : item.groupID)
      )
    );
    setUserListAll((prev: any) => [...prev, item]);
  };

  const handleSearch = (value: string) => {
    const str = value.length > 0 ? value.toLocaleLowerCase() : "";
    const dataFilter = userListAll.filter(
      (i: any) =>
        i.fullName?.toLocaleLowerCase().includes(str) ||
        i.groupName?.toLocaleLowerCase().includes(str)
    );
    setUserList(dataFilter);
  };

  const handleSearchExisting = (value: string) => {
    const str = value.length > 0 ? value.toLocaleLowerCase() : "";
    const dataFilter = userShareListAll.filter(
      (i: any) =>
        i.fullName?.toLocaleLowerCase().includes(str) ||
        i.groupName?.toLocaleLowerCase().includes(str)
    );
    setUserShareList(dataFilter);
  };

  const handleChangePriv = (item: any) => {
    const dataFilter = userShareListAll.map((row) =>
      (item.type == "user" ? row.userID : row.groupID) ===
      (item.type == "user" ? item.userID : item.groupID)
        ? item
        : row
    );
    setUserShareListAll(dataFilter);
  };

  const handleAfterOpenChange = (visible: boolean) => {
    if (!visible) {
      setUserShareListAll([]);
      setUserListAll([]);
    }
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
              <ShareAltOutlined
                style={{
                  border: "1px solid #d9d9d9",
                  padding: 7,
                  fontSize: 30,
                  color: "#434343",
                  borderRadius: 7,
                }}
              />
              <Space.Compact direction="vertical">
                <span>People With Access</span>
                <Text type="secondary" style={{ fontWeight: "normal" }}>
                  Create a new user by filling in the form below.
                </Text>
              </Space.Compact>
            </Space>
          </div>
        }
        destroyOnHidden
        afterOpenChange={handleAfterOpenChange}
        open={open}
        onCancel={() => setOpen(false)}
        maskClosable={false}
        footer={false}
        width={1100}
        zIndex={2}
        styles={{
          content: {
            padding: 0,
          },
        }}
        style={{ top: 20, paddingBottom: 0 }}
      >
        <Divider style={{ marginBottom: 0 }} />
        <div className="grid grid-cols-3 gap-y-5 mb-5 py-5 lg:divide-x-1 lg:divide-slate-300 h-[500px]">
          <div className="col-span-3 lg:col-span-2 ps-7 pe-5 h-full overflow-y-auto">
            <Input
              size="large"
              placeholder="Search ..."
              prefix={<SearchOutlined />}
              onChange={(e) => handleSearchExisting(e.target.value)}
            />
            <List
              itemLayout="horizontal"
              dataSource={userShareList}
              renderItem={(item) => (
                <List.Item>
                  <div className="flex flex-row px-2 justify-between items-center w-full">
                    <div className="flex gap-3 items-center">
                      <Avatar
                        size="large"
                        icon={
                          item.type == "user" ? (
                            <UserOutlined />
                          ) : (
                            <UsergroupAddOutlined />
                          )
                        }
                      />
                      <Flex vertical>
                        <span className="font-bold">
                          {item.type == "user" ? item.fullName : item.groupName}
                        </span>
                        <span style={{ textWrap: "nowrap" }}>
                          {item.type == "user" ? item.email : "Group"}
                        </span>
                      </Flex>
                    </div>
                    <div className="flex justify-between gap-5">
                      <Select
                        style={{ width: 120 }}
                        value={item.priv || "view"}
                        onChange={(e) => handleChangePriv({ ...item, priv: e })}
                        options={[
                          { value: "view", label: "View Only" },
                          { value: "editor", label: "Editor" },
                          { value: "full", label: "Full Controll" },
                        ]}
                      />
                      <Button
                        type="text"
                        icon={<Trash2 size={18} color="#595959" />}
                        iconPosition="start"
                        onClick={() => handleDelete(item)}
                      />
                    </div>
                  </div>
                </List.Item>
              )}
            />
          </div>
          <div className="col-span-3 lg:col-span-1 pe-7 ps-5 h-full overflow-y-auto">
            <Input
              size="large"
              placeholder="Search ..."
              prefix={<SearchOutlined />}
              onChange={(e) => handleSearch(e.target.value)}
            />
            <List
              itemLayout="horizontal"
              dataSource={userList}
              renderItem={(item) => (
                <List.Item
                  className="hover:cursor-pointer hover:bg-blue-50"
                  onClick={() => handleAdd(item)}
                >
                  <div className="flex px-2 items-center gap-3">
                    <UserPlus
                      size={30}
                      color="#434343"
                      style={{
                        border: "1px solid #d9d9d9",
                        padding: 5,
                        borderRadius: 7,
                      }}
                    />
                    <div className="flex gap-3 items-center">
                      <Avatar
                        size="large"
                        icon={
                          item.type == "user" ? (
                            <UserOutlined />
                          ) : (
                            <UsergroupAddOutlined />
                          )
                        }
                      />
                      <Flex vertical>
                        <span className="font-bold">
                          {item.type == "user" ? item.fullName : item.groupName}
                        </span>
                        <span style={{ textWrap: "nowrap" }}>
                          {item.type == "user" ? item.email : "Group"}
                        </span>
                      </Flex>
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
              icon={<CheckOutlined />}
              iconPosition="end"
              onClick={handleSubmit}
            >
              Submit
            </Button>
          </Space>
        </div>
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
        zIndex={5}
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
            Would you like to share document?
          </p>
          <span className="text-gray-500">
            The selected files will be shared with the people you select.
          </span>
        </div>
      </Modal>
    </>
  );
}
