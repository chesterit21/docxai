import { Button, Col, Flex, message, Row, Segmented, Space } from "antd";
import { AppstoreOutlined, BarsOutlined } from "@ant-design/icons";
import { useCallback, useEffect, useRef, useState } from "react";
import { ListTodo } from "lucide-react";
import { useSearchParams } from "react-router-dom";
import { useAuth } from "../../../context/AuthContext";
import TableComplex from "../../atom/viewData/TableComplexRowSelection";
import ListComplex from "../../atom/viewData/ListComplex";
import apiClient from "../../../services/apiClient";

const TableItemList = () => {
  const [searchParams] = useSearchParams();
  const typeOfSearch = searchParams.get("type");

  const { config } = useAuth();
  const pageLengthConfig = config?.dataGrid?.rowPerPage || 5;

  const [data, setData] = useState<any[]>([]);
  const [page, setPage] = useState<number>(1);
  const pageLength = pageLengthConfig;
  const [totalRow, setTotalRow] = useState<number>(0);
  const [loading, setLoading] = useState(false);
  const [viewType, setViewType] = useState<"table" | "list">("table");
  const [btnAddList, setBtnAddList] = useState<boolean>(true);
  const [selectedRowKeys, setSelectedRowKeys] = useState<React.Key[]>([]);

  const lastFetchKey = useRef<string>("");

  const getDocument = useCallback(async () => {
    setLoading(true);
    const uri = typeOfSearch === "advance" ? "/top/advsearch" : "/top/search";

    const params = new URLSearchParams(searchParams.toString());
    params.set("sort", "asc");
    params.set("Page", page.toString());
    params.set("Limit", pageLengthConfig.toString());

    try {
      const { data } = await apiClient.get(`${uri}?${params.toString()}`);
      setData(data.data.data);
      setTotalRow(data.data.totalRecords);
    } catch (err) {
      console.error(err);
      setData([]);
    } finally {
      setLoading(false);
    }
  }, [typeOfSearch, searchParams, page, pageLengthConfig]);

  useEffect(() => {
    const currentKey = `${typeOfSearch}-${searchParams.toString()}-${page}-${pageLengthConfig}`;
    if (lastFetchKey.current === currentKey) return;

    lastFetchKey.current = currentKey;
    getDocument();
  }, [getDocument, typeOfSearch, searchParams, page, pageLengthConfig]);

  useEffect(() => {
    if (selectedRowKeys.length > 0) {
      setBtnAddList(false);
    } else {
      setBtnAddList(true);
    }
  }, [selectedRowKeys]);

  const handleClickAddToList = () => {
    const dataStore = { documentIds: selectedRowKeys };
    console.log(dataStore);
    setLoading(true);
    apiClient
      .post("/top/add-itemlist", dataStore)
      .then(({ data }) => {
        console.log(data);
        setLoading(false);
        message.success("Add to list is successful");
      })
      .catch((err) => {
        console.log(err);
        message.error(err.response.data.message);
        setLoading(false);
      });
  };

  return (
    <>
      <Row style={{ marginBottom: 20 }}>
        <Col span={24}>
          <Flex justify="space-between">
            <span className="text-2xl font-bold">Search Result</span>
            <Space size="large">
              {viewType == "table" && (
                <Button
                  icon={<ListTodo size={20} />}
                  iconPosition="end"
                  onClick={handleClickAddToList}
                  disabled={btnAddList}
                >
                  Add To list
                </Button>
              )}
              <Segmented
                size="large"
                onChange={(val: any) => setViewType(val)}
                options={[
                  { value: "table", icon: <AppstoreOutlined /> },
                  { value: "list", icon: <BarsOutlined /> },
                ]}
              />
            </Space>
          </Flex>
        </Col>
      </Row>
      <Row>
        <Col span={24}>
          {viewType == "table" ? (
            <TableComplex
              setSelectedRowKeys={setSelectedRowKeys}
              data={data}
              setData={setData}
              pageSize={pageLength}
              page={page}
              setPage={setPage}
              totalRow={totalRow}
              loading={loading}
            />
          ) : (
            <ListComplex
              data={data}
              pageLength={pageLength}
              page={page}
              setPage={setPage}
              total={totalRow}
              loading={loading}
            />
          )}
        </Col>
      </Row>
    </>
  );
};

export default TableItemList;
