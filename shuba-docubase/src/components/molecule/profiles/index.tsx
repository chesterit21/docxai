import { useEffect, useState } from "react";
import { Avatar, Button, Card, Col, Divider, Row, Space } from "antd";
import { Pencil } from "lucide-react";
import {
    CloudUploadOutlined,
    LockOutlined,
    UserOutlined
} from '@ant-design/icons';
import { useNavigate } from "react-router-dom";
import ModalChangePass from './ModalChangePass'
import ModalUploadAvatar from './ModalUploadAvatar'
import { useAuth } from "../../../context/AuthContext";
import apiClient from "../../../services/apiClient";


export default function Index() {
    const navigate = useNavigate();
    const { user } = useAuth();
    const userId = user?.['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] || 0
    
    const [openChangePass, setOpenChangePass] = useState(false)
    const [openUploadAvatar, setOpenUploadAvatar] = useState(false)
    const [profile, setProfile] = useState<any>({})

    const getProfile = () => {
        apiClient.get(`/user/get-profile?id=${userId}`)
        .then(({ data }) => {
            setProfile(data?.data)
            console.log(data?.data)
        })
        .catch(err => {
            console.log(err)
        })
    }

    useEffect(()=> {
        getProfile()
    }, [userId])

    return (
        <>
            <Row className="my-7" gutter={[12, 24]}>
                <Col xs={24} sm={24} md={24} lg={12}>
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
                <Col xs={24} sm={24} md={24} lg={12} style={{display: 'flex', justifyContent: 'end'}}>
                    <Space size="large">
                        <Button type="primary" icon={<Pencil size={14}/>} iconPosition="end" onClick={()=> navigate(`/profiles/edit/${userId}`)}>
                            Edit Profile
                        </Button>
                        <Button color="purple" variant="solid"  icon={<LockOutlined />} iconPosition="end" onClick={()=> setOpenChangePass(true)}>
                            Change Password
                        </Button>
                        <Button icon={<CloudUploadOutlined style={{fontSize: 16}}/>} iconPosition="end" onClick={()=> setOpenUploadAvatar(true)}>
                            Upload Avatar
                        </Button>
                    </Space>
                </Col>
            </Row>
            <Row gutter={[16, 16]}>
                <Col span={12}>
                    <Card styles={{ body:{padding: 0}}} style={{border: 'none'}}>
                        <p className="font-bold">Full Name</p>
                        <span className="">{profile?.fullName || ''}</span>
                        <Divider style={{margin: '10px 0px'}}></Divider>
                    </Card>
                </Col>
                <Col span={12}>
                    <Card styles={{ body:{padding: 0}}} style={{border: 'none'}}>
                        <p className="font-bold">Email</p>
                        <span className="">{profile?.emailAddress || ''}</span>
                        <Divider style={{margin: '10px 0px'}}></Divider>
                    </Card>
                </Col>
                <Col span={12}>
                    <Card styles={{ body:{padding: 0}}} style={{border: 'none'}}>
                        <p className="font-bold">Phone</p>
                        <span className="">{profile?.phoneNumber || '-'}</span>
                        <Divider style={{margin: '10px 0px'}}></Divider>
                    </Card>
                </Col>
                {/* <Col span={24}>
                    <Card styles={{ body:{padding: 0}}} style={{border: 'none'}}>
                        <p className="font-bold">Email</p>
                        <span className="">mail@mail.com</span>
                        <Divider style={{margin: '10px 0px'}}></Divider>
                    </Card>
                </Col> */}
            </Row>
            <ModalChangePass open={openChangePass} setOpen={setOpenChangePass} userId={userId}/>
            <ModalUploadAvatar open={openUploadAvatar} setOpen={setOpenUploadAvatar}/>
        </>
    )
}