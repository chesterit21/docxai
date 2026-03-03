import Breadcrumb  from '../../atom/Breadcrumb';
import { Avatar, Button, Card, Col, Divider, Row, Space, Tag } from "antd";
import {
    UsergroupAddOutlined,
} from '@ant-design/icons';
import { useNavigate, useParams } from "react-router-dom";
import apiClient from '../../../services/apiClient';
import { useEffect, useState } from 'react';


export default function Index() {
  const navigate = useNavigate();
    const { uuid: documentId } = useParams();
    const [dataDetail, setDataDetail] = useState<any>({})

    useEffect(()=> {
        getDataDetail()
    }, [documentId]);

    const getDataDetail = ()=> {
        apiClient.get(`/user/id?id=${documentId}`)
        .then(({ data }) => {
            setDataDetail(data?.data || {})
        })
        .catch(err => {
            console.log(err)
        })
    }

    return (
        <>
            <Breadcrumb item={[{title: 'Users & Group'},{title: <span style={{fontWeight: 'bold'}}>User Detail</span>}]}/>
            <Row gutter={[16, 16]} style={{marginTop: 30}}>
                <Col span={24}>
                    <Card styles={{ body:{padding: 0}}} style={{border: 'none'}}>
                        <p className="font-bold">fullname</p>
                        <span className="">{dataDetail?.fullName || '-'}</span>
                        <Divider style={{margin: '10px 0px'}}></Divider>
                    </Card>
                </Col>
                <Col span={12}>
                    <Card styles={{ body:{padding: 0}}} style={{border: 'none'}}>
                        <p className="font-bold">Username</p>
                        <span className="">{dataDetail?.userName || '-'}</span>
                        <Divider style={{margin: '10px 0px'}}></Divider>
                    </Card>
                </Col>
                <Col span={12}>
                    <Card styles={{ body:{padding: 0}}} style={{border: 'none'}}>
                        <p className="font-bold">AD User</p>
                        <span className="">{dataDetail?.isADUser ? 'True' : 'False'}</span>
                        <Divider style={{margin: '10px 0px'}}></Divider>
                    </Card>
                </Col>
                <Col span={12}>
                    <Card styles={{ body:{padding: 0}}} style={{border: 'none'}}>
                        <p className="font-bold">Email</p>
                        <span className="">{dataDetail?.emailAddress || '-'}</span>
                        <Divider style={{margin: '10px 0px'}}></Divider>
                    </Card>
                </Col>
                <Col span={12}>
                    <Card styles={{ body:{padding: 0}}} style={{border: 'none'}}>
                        <p className="font-bold">Email Verified</p>
                        <span className="">{dataDetail?.emailVerified ? 'True' : 'False'}</span>
                        <Divider style={{margin: '10px 0px'}}></Divider>
                    </Card>
                </Col>
                <Col span={12}>
                    <Card styles={{ body:{padding: 0}}} style={{border: 'none'}}>
                        <p className="font-bold">User Type</p>
                        <span className="">{dataDetail?.userType || '-'}</span>
                        <Divider style={{margin: '10px 0px'}}></Divider>
                    </Card>
                </Col>
                <Col span={12}>
                    <Card styles={{ body:{padding: 0}}} style={{border: 'none'}}>
                        <p className="font-bold">Status User</p>
                        <span className="">{dataDetail?.isActive  ? 'Active' : 'Inactive'}</span>
                        <Divider style={{margin: '10px 0px'}}></Divider>
                    </Card>
                </Col>
                <Col span={24}>
                    <Card styles={{ body:{padding: 0}}} style={{border: 'none'}}>
                        <p className="font-bold">Company</p>
                        <Space size="small">
                            {
                                dataDetail?.company?.name || '-'
                            }
                        </Space>
                        <Divider style={{margin: '10px 0px'}}></Divider>
                    </Card>
                </Col>
                <Col span={24}>
                    <Card styles={{ body:{padding: 0}}} style={{border: 'none'}}>
                        <p className="font-bold">Group</p>
                        <Space size="small">
                            {
                                dataDetail?.groups?.map((item: any) => (
                                    <Tag color="default" style={{padding: '2px 7px'}}>
                                        <Avatar style={{ backgroundColor: '#87d068' }} icon={<UsergroupAddOutlined />}  size={20}/> {item.groupName}
                                    </Tag>
                                )) || '-'
                            }
                        </Space>
                        <Divider style={{margin: '10px 0px'}}></Divider>
                    </Card>
                </Col>
                <Col span={24} style={{display: 'flex', justifyContent: 'end'}}>
                    <Space size="large">
                        <Button onClick={()=> navigate('/users-group')} color="danger" variant="filled">
                            Back
                        </Button>
                    </Space>
                </Col>
            </Row>
        </>
    )
}