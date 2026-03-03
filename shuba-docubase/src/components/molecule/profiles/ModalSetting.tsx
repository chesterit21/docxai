import { useEffect, useState } from "react";
import { Modal, Space, Typography, Divider, Form, Input, Button, Tabs, InputNumber } from "antd";
import {
    CloseOutlined,
    SaveOutlined,
    SettingOutlined,
} from '@ant-design/icons';
import apiClient from '../../../services/apiClient';


const { Text } = Typography;


export default function Index({open, setOpen}: any) {
    const [dataSetting, setDataSetting] = useState<any>({});
    const [form] = Form.useForm();

    useEffect(() => {
        if(open){
            apiClient.get('/setting')
            .then(({ data }) => {
                console.log(data?.data);
                setDataSetting(data?.data);
                form.resetFields();
            })
            .catch(err => {
                console.log(err);
            })
        }
    }, [open]);

    useEffect(() => {
        form.setFieldsValue(dataSetting);
    }, [dataSetting]);

    const formatLabel = (text: string): string => {
        return text.replace(/([a-z])([A-Z])/g, '$1 $2');
    }

    const handleSubmit = ()=> {
        console.log('submit', dataSetting);
        apiClient.post('/setting', dataSetting)
        .then(({ data }) => {
            console.log(data?.data);
            form.resetFields();
            setOpen(false);
        })
        .catch(err => {
            console.log(err);
        })
    }

    const handleChangeVal = (key1: any, key2: any, value: any) => {
        setDataSetting((prevState: any) => ({ ...prevState, [key1]: { ...prevState[key1], [key2]: value }}))
    }

    const handleClose = () => {
        setOpen(false);
        setDataSetting({});     
        form.resetFields();
    }

    return (
        <>
            <Modal
            title={(
            <div className="space-align-block" style={{paddingLeft: 24, paddingRight: 24, paddingTop: 24}}>
                <Space align="center" size="middle">
                <SettingOutlined style={{border: '1px solid #d9d9d9', padding: 7, fontSize: 30, color: '#434343', borderRadius: 7}}/>
                <Space.Compact direction="vertical">
                    <span>Setting</span>
                    <Text type="secondary" style={{fontWeight: 'normal'}}>Setting your application</Text>
                </Space.Compact>
                </Space>
            </div>
            )}
            open={open}
            onCancel={handleClose}
            maskClosable={false}
            destroyOnHidden
            footer={false}
            width={800}
            styles={{
                content: {
                    padding: 0,
                },
            }}
            style={{ top: 20 }}
            >
                <Divider style={{marginBottom: 0}}/>
                <Form 
                layout='vertical'
                onFinish={handleSubmit}
                form={form}
                >
                    <div style={{padding: 24, paddingBottom: 10, minHeight: 250}}>
                        <Tabs
                        tabPosition="left"
                        items={
                            Object.keys(dataSetting).map((groupKey, index) => {
                            const groupData = dataSetting[groupKey];

                            if (!groupData || typeof groupData !== 'object') {
                                return {
                                label: <span className="capitalize">{formatLabel(groupKey)}</span>,
                                key: `index-${index}`,
                                children: <div className="text-gray-500 italic">No data available</div>,
                                };
                            }

                            return {
                                label: <span className="capitalize">{formatLabel(groupKey)}</span>,
                                key: `index-${index}`,
                                children: (
                                <>
                                    {Object.keys(groupData).map((fieldKey) => {
                                    const value = groupData[fieldKey];
                                    const isNumberField = ['rowPerPage', 'idleTimeoutAfterMinutes', 'attempts', 'port', 'commandTimeout', 'timeout', 'purgingPeriodInMonth', 'allowedReloginAfterMinutes'].includes(fieldKey);

                                    return (
                                        <Form.Item
                                        key={`${groupKey}-${fieldKey}`}
                                        label={<span className="capitalize">{formatLabel(fieldKey)}</span>}
                                        name={[groupKey, fieldKey]}
                                        // rules={[{ required: true }]}
                                        >
                                        {isNumberField ? (
                                            <InputNumber
                                            onChange={(val) => handleChangeVal(groupKey, fieldKey, val)}
                                            min={0}
                                            style={{ width: '100%' }}
                                            value={value}
                                            />
                                        ) : (
                                            <Input
                                            onChange={(e) => handleChangeVal(groupKey, fieldKey, e.target.value)}
                                            placeholder="Enter value..."
                                            value={value}
                                            />
                                        )}
                                        </Form.Item>
                                    );
                                    })}
                                </>
                                ),
                            };
                            })
                        }
                        />

                    </div>
                    <Divider style={{marginTop:0, marginBottom: 0}}/>
                    <div style={{padding: 20, paddingBottom: 20, justifySelf: 'end'}}>
                        <Space>
                            <Button icon={<CloseOutlined />} iconPosition="end" onClick={()=> setOpen(false)} color="danger" variant="filled">
                            Cancel
                            </Button>
                            <Button type="primary" icon={<SaveOutlined />} iconPosition="end" htmlType='submit'>
                            Save
                            </Button>
                        </Space>
                    </div>
                </Form>
            </Modal>
        </>
    )
}