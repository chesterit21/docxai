import { Button, Divider, Form, Input, message, Modal, Space, Typography } from "antd";
import {
    CloseOutlined,
    FolderFilled,
    SaveOutlined,
} from '@ant-design/icons';
import { useEffect, useState } from "react";
import apiClient from "../../../services/apiClient";
const { Text } = Typography;
const { TextArea } = Input;


export default function Index({open, setOpen, eventData={}, rerenderData, parent}: any) {
    const [form] = Form.useForm();
    const [loading, setLoading] = useState<boolean>(false)
    
    useEffect(()=> {
        if(Object.keys(eventData).length > 0) {
            form.setFieldsValue(eventData)
        }
    }, [eventData, form])

    const handleSubmit = async () => {
        const values = await form.validateFields();
        const dataUpdate = parent || parent != undefined ? {...values, parentId: parent} : values
        apiClient.put(`/category`, dataUpdate)
        .then(({ data }) => {
            console.log(data)
            setLoading(false)
            form.resetFields()
            message.success('Update data is successful')
            rerenderData()
            setOpen(false);
        })
        .catch(err => {
            console.log(err)
            message.error(err.response.data.message)
            setLoading(false)
        })
    }
    return (
        <>
            <Modal
            title={(
            <div className="space-align-block" style={{paddingLeft: 24, paddingRight: 24, paddingTop: 24}}>
                <Space align="center" size="middle">
                <FolderFilled style={{ fontSize: 60, color: '#9254de'}}/>
                <Space.Compact direction="vertical">
                    <span>Edit Categories</span>
                    <Text type="secondary" style={{fontWeight: 'normal'}}>Edit Categories name by entering in the field below.</Text>
                </Space.Compact>
                </Space>
            </div>
            )}
            open={open}
            onCancel={()=> setOpen(false)}
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
                layout='vertical'
                form={form}
                onFinish={handleSubmit}
                >
                    <div style={{padding: 24, paddingBottom: 10}}>
                        <Form.Item 
                        name="id"
                        hidden
                        >
                            <Input />
                        </Form.Item>
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
                        rules={[{ type: 'string'}, {required: true}]}
                        >
                            <TextArea rows={4} placeholder="enter description..." />
                        </Form.Item>
                    </div>
                    <Divider style={{marginTop:0, marginBottom: 0}}/>
                    <div style={{padding: 20, paddingBottom: 20, justifySelf: 'end'}}>
                        <Space>
                            <Button icon={<CloseOutlined />} iconPosition="end" onClick={()=> setOpen(false)} color="danger" variant="filled">
                            Cancel
                            </Button>
                            <Button type="primary" icon={<SaveOutlined />} iconPosition="end" htmlType='submit' loading={loading}>
                            Save
                            </Button>
                        </Space>
                    </div>
                </Form>
            </Modal>
        </>
    )
}