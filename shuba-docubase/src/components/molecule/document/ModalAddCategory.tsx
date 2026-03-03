import { useState } from "react";
import { Button, Divider, Form, Input, message, Modal, Space, Typography } from "antd";
import {
    CloseOutlined,
    FolderFilled,
    PlusOutlined,
    SaveOutlined,
} from '@ant-design/icons';
import apiClient from '../../../services/apiClient';

const { Text } = Typography;
const { TextArea } = Input;


export default function Index({rerenderData, parent}: any) {
    const [openAdd, setOpenAdd] = useState(false);
    const [confirmAddLoading, setConfirmAddLoading] = useState(false);
    const [form] = Form.useForm();
    const handleSubmit = () => {
        form.validateFields().then((values) => {
        const dataStore = parent || parent != undefined ? {...values, parentId: parent} : values
        apiClient.post('/category', dataStore)
            .then(({ data }) => {
                console.log(data)
                setConfirmAddLoading(false);
                setOpenAdd(false)
                message.success('Create data is successful')
                form.resetFields()
                rerenderData();
            })
            .catch(err => {
                console.log(err)
                message.error(err.response.data.message)
                setConfirmAddLoading(false);
            })
        })
    }
    return (
        <>
            <Button variant="solid" color="purple" icon={<PlusOutlined />} iconPosition="end" onClick={()=> setOpenAdd(true)}>
                Create Category
            </Button>
            <Modal
            title={(
            <div className="space-align-block" style={{paddingLeft: 24, paddingRight: 24, paddingTop: 24}}>
                <Space align="center" size="middle">
                <FolderFilled style={{ fontSize: 60, color: '#9254de'}}/>
                <Space.Compact direction="vertical">
                    <span>New Categories</span>
                    <Text type="secondary" style={{fontWeight: 'normal'}}>Create a new folder by entering the name in the field below.</Text>
                </Space.Compact>
                </Space>
            </div>
            )}
            open={openAdd}
            onCancel={()=> setOpenAdd(false)}
            maskClosable={false}
            footer={false}
            width={600}
            styles={{
            content: {
                padding: 0
            },
            }}
            >
                <Divider style={{marginBottom: 0}}/>
                <Form
                form={form}
                layout='vertical'
                onFinish={handleSubmit}
                >
                    <div style={{padding: 24, paddingBottom: 10}}>
                        <Form.Item 
                        label="Categories Name"
                        name="categoryName"
                        rules={[{ type: 'string'}, {required: true}]}
                        >
                            <Input placeholder="enter category name..." />
                        </Form.Item>
                        <Form.Item 
                        label="Description"
                        name="categoryDesc"
                        rules={[{ type: 'string'}, {required: false}]}
                        >
                            <TextArea rows={4} placeholder="enter description..." />
                        </Form.Item>
                    </div>
                    <Divider style={{marginTop:0, marginBottom: 0}}/>
                    <div style={{padding: 20, paddingBottom: 20, justifySelf: 'end'}}>
                        <Space>
                            <Button icon={<CloseOutlined />} iconPosition="end" onClick={()=> setOpenAdd(false)} color="danger" variant="filled">
                            Cancel
                            </Button>
                            <Button type="primary" icon={<SaveOutlined />} iconPosition="end" htmlType='submit' loading={confirmAddLoading}>
                            Save
                            </Button>
                        </Space>
                    </div>
                </Form>
            </Modal>
        </>
    )
}