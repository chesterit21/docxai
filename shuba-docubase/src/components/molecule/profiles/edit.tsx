import { useEffect, useState } from "react";
import Breadcrumb  from '../../atom/Breadcrumb';
import { Avatar, Button, Col, Row, Space, Form, Input, Modal, message } from "antd";
import {
    CheckOutlined,
    CloseOutlined,
    ExclamationCircleOutlined,
    SaveOutlined,
    UserOutlined
} from '@ant-design/icons';
import { useNavigate } from "react-router-dom";
import apiClient from "../../../services/apiClient";
import { useAuth } from "../../../context/AuthContext";


export default function Index() {
    const navigate = useNavigate();
    const { user } = useAuth();
    const userId = user?.['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] || 0
    
    const [open, setOpen] = useState(false);
    const [confirmLoading, setConfirmLoading] = useState(false);
    const [profile, setProfile] = useState<any>({})
    const [form] = Form.useForm();

  


    const getProfile = () => {
        apiClient.get(`/user/get-profile?id=${userId}`)
        .then(({ data }) => {
            setProfile(data?.data)
        })
        .catch(err => {
            console.log(err)
        })
    }

    useEffect(()=> {
        getProfile()
    }, [userId])

    useEffect(()=> {
        form.setFieldsValue(profile)
    }, [profile])

    const handleOk = () => {
        setConfirmLoading(true);
        const dataStore = form.getFieldsValue()
        dataStore.phoneNumber = dataStore.phoneNumber
        ? dataStore.phoneNumber.toString().startsWith("0")
          ? "62" + dataStore.phoneNumber.toString().slice(1)
          : dataStore.phoneNumber.toString()
        : "";

        apiClient.post('/user/update-profile', dataStore)
        .then(({ data }) => {
            console.log(data)
            message.success('Update profile is successful')
            setConfirmLoading(false);
            navigate('/profiles')
        })
        .catch(err => {
            console.log(err)
            setConfirmLoading(false);
            message.error(err.response.data.message)
        })
    };

    return (
        <>
            <Breadcrumb item={[{title: 'Profiles'}, {title: <span className='font-bold'>Edit</span>}]}/>
            <Row className="my-7" gutter={[12, 24]}>
                <Col xs={24} sm={24} md={24} lg={24}>
                    <Space align="center" size="middle">
                        {
                            profile?.userMedia?.photoUrl ?
                            <Avatar size={89} src={import.meta.env.VITE_API_URL + profile.userMedia.photoUrl} /> :
                            <Avatar size={89} icon={<UserOutlined />} />
                        }
                        <Space direction="vertical" size="small">
                            <span className="text-2xl font-bold">{profile?.fullName || ''}</span>
                            <span>{profile?.userName || ''}</span>
                        </Space>
                    </Space>
                </Col>
            </Row>
            <Form layout="vertical" form={form}>
                <Row gutter={[16, 16]}>
                    <Form.Item 
                    name="userId"
                    hidden
                    >
                        <Input />
                    </Form.Item>
                    <Col span={12}>
                        <Form.Item 
                        label="First Name"
                        name="fullName"
                        rules={[{ type: 'string'}, {required: true}]}
                        >
                            <Input placeholder="Full Name"/>
                        </Form.Item>
                    </Col>
                    <Col span={12}>
                        <Form.Item 
                        label="Email"
                        name="emailAddress"
                        rules={[{ type: 'email'}, {required: true}]}
                        >
                            <Input placeholder="Last Name"/>
                        </Form.Item>
                    </Col>
                    <Col span={12}>
                        <Form.Item 
                        label="Phone"
                        name="phoneNumber"
                        rules={[{ type: 'string'}, {required: true}]}
                        >
                            <Input placeholder="Ex. 628123456789"/>
                        </Form.Item>
                    </Col>
                    {/* <Col span={24}>
                        <Form.Item 
                        label="Email"
                        name="email"
                        rules={[{ type: 'email'}, {required: true}]}
                        initialValue="mail@mail.com"
                        >
                            <Input placeholder="Email"/>
                        </Form.Item>
                    </Col> */}
                    <Col span={24} style={{display: 'flex', justifyContent: 'end'}}>
                        <Space size="large">
                            <Button icon={<CloseOutlined />} iconPosition="end" onClick={()=> navigate('/profiles')} color="danger" variant="filled">
                                Cancel
                            </Button>
                            <Button type="primary" icon={<SaveOutlined />} iconPosition="end" onClick={()=> setOpen(true)}>
                                Save Changed
                            </Button>
                        </Space>
                    </Col>
                </Row>
            </Form>
            <Modal
                title={(
                    <div style={{paddingLeft: 24, paddingRight: 24, paddingTop: 24}}>
                        <ExclamationCircleOutlined style={{fontSize: 30, color: 'gold'}}/>
                    </div>
                )}
                open={open}
                onCancel={()=> setOpen(false)}
                maskClosable={false}
                footer={(_,) => (
                <>
                    <Row gutter={12}>
                        <Col span={12}>
                            <Button icon={<CloseOutlined />} iconPosition="end" onClick={()=> setOpen(false)} variant="filled" block>
                            Cancel
                            </Button>
                        </Col>
                        <Col span={12}>
                            <Button type="primary" icon={<CheckOutlined />} iconPosition="end" onClick={handleOk} block loading={confirmLoading}>
                            Confirm
                            </Button>
                        </Col>
                    </Row>
                </>
                )}
                width={400}
                styles={{
                content: {
                    padding: 0
                },
                footer: {
                    padding: 20
                }
                }}
            >
                <div style={{padding: '10px 24px'}}>
                    <p className="font-bold text-lg" style={{marginBottom: 0}}>Do you want to update your profile?</p>
                    <span className="text-gray-500">The data on your profile will be update after you click confirm</span>
                </div>
            </Modal>
        </>
    )
}