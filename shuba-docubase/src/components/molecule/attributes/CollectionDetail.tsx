import Breadcrumb  from '../../atom/Breadcrumb';
import { Button, Card, Checkbox, Col, DatePicker, Divider, Input, Row, Space } from "antd";
import { useEffect, useState } from 'react';
import { useParams, useNavigate } from "react-router-dom";
import apiClient from '../../../services/apiClient';

const { TextArea } = Input;

export default function Index() {
    const navigate = useNavigate();
    const {uuid} = useParams();
    const [data, setData] = useState<any>({})

    useEffect(()=> {
        apiClient.get(`/attributecollections/get-by-id?id=${uuid}`)
        .then(({ data }) => {
            setData(data.data)
        })
        .catch(err => {
            console.log(err)
        })
    }, [])


    return (
        <>
            <Breadcrumb item={[{title: <span style={{fontWeight: 'bold'}}>Collection View</span>}]}/>
            <Row gutter={[16, 16]} style={{marginTop: 30}}>
                <Col span={24}>
                    <Card styles={{ body:{padding: 0}}} style={{border: 'none'}}>
                        <p className="font-bold">Collection Name</p>
                        <span className="">{data?.collectionName}</span>
                        <Divider style={{margin: '10px 0px'}}></Divider>
                    </Card>
                </Col>
                <Col span={24}>
                    <Card styles={{ body:{padding: 0}}} style={{border: 'none'}}>
                        <p className="font-bold">Description</p>
                        <span className="">{data?.collectionDescription}</span>
                        <Divider style={{margin: '10px 0px'}}></Divider>
                    </Card>
                </Col>
                {
                    Object.keys(data).length > 0 && JSON.parse(data?.attributeElementCollection || []).length > 0 && JSON.parse(data.attributeElementCollection).map((item: any, Index: number) => {
                        const attr = item?.attributeElement ? JSON.parse(item.attributeElement) : {}
                        return (
                          <>
                            {Object.keys(attr).length > 0 && (
                              <Col span={24} key={Index}>
                                <Card
                                  styles={{ body: { padding: 0 } }}
                                  style={{ border: "none" }}
                                >
                                  <p className="font-bold">
                                    {attr?.label}{" "}
                                    <span className="text-slate-400 font-light italic">
                                      ({attr?.type})
                                    </span>
                                  </p>
                                  {(attr?.type === "text-field" ||
                                    attr?.type === "number") && (
                                    <Input
                                      placeholder={attr?.placeholder || ""}
                                      disabled
                                      style={{ width: "50%" }}
                                    />
                                  )}
                                  {attr?.type === "text-area" && (
                                    <TextArea
                                      rows={2}
                                      placeholder={attr?.placeholder || ""}
                                      disabled
                                      style={{ width: "50%" }}
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
                                      disabled
                                      style={{ width: "50%" }}
                                    />
                                  )}
                                  <div className="my-2 flex gap-2">
                                    <Checkbox
                                      checked={attr?.required}
                                      disabled
                                    />{" "}
                                    <span className="font-light text-slate-400">Required</span>
                                  </div>
                                  <Divider
                                    style={{ margin: "10px 0px" }}
                                  ></Divider>
                                </Card>
                              </Col>
                            )}
                          </>
                        );
                    })
                }
                <Col span={24} style={{display: 'flex', justifyContent: 'end'}}>
                    <Space size="large">
                        <Button onClick={()=> navigate('/attributes')} color="danger" variant="filled">
                            Back
                        </Button>
                    </Space>
                </Col>
            </Row>
        </>
    )
}