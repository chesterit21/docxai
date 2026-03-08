import { useState } from "react";
import { Button, Col, Divider, Modal, Row, Space, Steps, Typography, Form, Select, message } from "antd";
import {
    ArrowRightOutlined,
    CheckOutlined,
    CloseOutlined,
    ExclamationCircleOutlined,
    PlusOutlined,
} from '@ant-design/icons';
import { ArrowLeft } from "lucide-react";
const { Text } = Typography;
const { Option } = Select;
import { CreateAttribute } from './CreateAttribute'
// import apiClient from '../../../services/apiClient';

export default function Index({ open, setOpen, setData }: any) {
    const [openConfirmAdd, setOpenConfirmAdd] = useState(false);
    const [confirmAddLoading, setConfirmAddLoading] = useState(false);
    const [current, setCurrent] = useState(0);
    const [selectType, setSelectType] = useState<string>('')
    const [newAttribute, setNewAttribute] = useState({})
    const [checkValidateAtt, setCheckValidateAttr] = useState(false)

    const next = () => {
        setCurrent(current + 1);
    };

    const prev = () => {
        setCurrent(current - 1);
    };

    const validateSubmit = () => {
        setCheckValidateAttr(true)
    }

    const handleSubmit = () => {
        setOpenConfirmAdd(true);
    }

    const handleOkAdd = () => {
        if (Object.keys(newAttribute).length > 0) {
            setConfirmAddLoading(true);
            const dataStore = { ...newAttribute, id: Date.now() }
            setData((prev: any) => [...prev, dataStore])
            setTimeout(() => {
                setNewAttribute({})
                setConfirmAddLoading(false);
                setOpenConfirmAdd(false);
                setOpen(false)
                setSelectType('')
                setCurrent(0)
                message.success('Create data is successful')
            }, 500);
        }
    };

    const onModalClose = () => {
        setCurrent(0)
        setSelectType('')
    }

    const steps = [
        {
            title: 'Select Type',
            content: (
                <Form
                    layout='vertical'
                >
                    <Form.Item
                        label="Attribute Type"
                        name="type"
                        rules={[{ required: true }]}
                    >
                        <Select onChange={(option) => setSelectType(option)} placeholder="Select type">
                            <Option value="text-field">Text Field</Option>
                            <Option value="text-area">Text Area</Option>
                            <Option value="number">Number</Option>
                            <Option value="date">Date</Option>
                            <Option value="select">Select</Option>
                            <Option value="checkbox">Checkbox Group</Option>
                            <Option value="radio">Radio Group</Option>
                        </Select>
                    </Form.Item>

                </Form>
            ),
        },
        {
            title: 'Define Field',
            content: (
                <CreateAttribute
                    selectedType={selectType}
                    setAttribute={setNewAttribute}
                    checkValidateAtt={checkValidateAtt}
                    setCheckValidateAttr={setCheckValidateAttr}
                    submited={handleSubmit}
                />),
        }];

    const stepItems = steps.map((item) => ({ key: item.title, title: item.title }));

    return (
        <>
            <Modal
                title={(
                    <div className="space-align-block" style={{ paddingLeft: 24, paddingRight: 24, paddingTop: 24 }}>
                        <Space align="center" size="middle">
                            <PlusOutlined style={{ border: '1px solid #d9d9d9', padding: 7, fontSize: 30, color: '#434343', borderRadius: 7 }} />
                            <Space.Compact direction="vertical">
                                <span>Add New Attribute</span>
                                <Text type="secondary" style={{ fontWeight: 'normal' }}>Create a new Attribute by selecting and filling in the from below.</Text>
                            </Space.Compact>
                        </Space>
                    </div>
                )}
                open={open}
                onCancel={() => setOpen(false)}
                afterClose={onModalClose}
                destroyOnHidden
                maskClosable={false}
                footer={false}
                width={600}
                styles={{
                    content: {
                        padding: 0
                    },
                }}
                style={{ top: 20 }}
            >
                <Divider style={{ marginBottom: 0 }} />
                <div className='px-7 py-5'>
                    <Steps current={current} items={stepItems} />
                    <div style={{ padding: 24, paddingBottom: 10 }}>
                        {steps[current].content}
                    </div>
                </div>
                <Divider style={{ marginTop: 0, marginBottom: 0 }} />
                <div style={{ padding: 20, paddingBottom: 20, justifySelf: 'end' }}>
                    <Space>
                        {current > 0 && (
                            <Button icon={<ArrowLeft />} iconPosition="start" style={{ margin: '0 8px' }} onClick={() => [, setSelectType(''), prev()]}>
                                Previous
                            </Button>
                        )}
                        <Button icon={<CloseOutlined />} iconPosition="end" onClick={() => setOpen(false)} color="danger" variant="filled">
                            Cancel
                        </Button>
                        {current < steps.length - 1 && (
                            <Button
                                type="primary"
                                icon={<ArrowRightOutlined />}
                                iconPosition="end"
                                disabled={selectType.length > 1 ? false : true}
                                onClick={() => next()}>
                                Next
                            </Button>
                        )}
                        {current === steps.length - 1 && (
                            <Button type="primary" icon={<PlusOutlined />} iconPosition="end" onClick={validateSubmit}>
                                Create Attribute
                            </Button>
                        )}

                    </Space>
                </div>
            </Modal>
            {/* modal confirm add */}
            <Modal
                zIndex={1060}
                title={(
                    <div style={{ paddingLeft: 24, paddingRight: 24, paddingTop: 24 }}>
                        <ExclamationCircleOutlined style={{ fontSize: 30, color: 'gold' }} />
                    </div>
                )}
                open={openConfirmAdd}
                onCancel={() => setOpenConfirmAdd(false)}
                maskClosable={false}
                footer={(_,) => (
                    <>
                        <Row gutter={12}>
                            <Col span={12}>
                                <Button icon={<CloseOutlined />} iconPosition="end" onClick={() => setOpenConfirmAdd(false)} variant="filled" block>
                                    Cancel
                                </Button>
                            </Col>
                            <Col span={12}>
                                <Button type="primary" icon={<CheckOutlined />} iconPosition="end" onClick={handleOkAdd} block loading={confirmAddLoading}>
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
                <div style={{ padding: '10px 24px' }}>
                    <p className="font-bold text-lg" style={{ marginBottom: 0 }}>Are you sure you want to create an Attribute?</p>
                    <span className="text-gray-500">Make sure this Attribute suits your needs.</span>
                </div>
            </Modal>
        </>
    )
}