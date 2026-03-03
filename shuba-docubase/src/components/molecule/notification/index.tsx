import './style.css'
import { useEffect, useState } from 'react';
import { Col, Flex, Row, Space, DatePicker, Form, Avatar, List, Button, Input, Modal, Divider, Card, Typography, type PaginationProps } from 'antd';
import {
    ArrowLeftOutlined,
    ArrowRightOutlined,
    BellOutlined,
    CloseOutlined,
    SearchOutlined,
    UserOutlined,
} from '@ant-design/icons';
import { useAuth } from '../../../context/AuthContext';
import apiClient from '../../../services/apiClient';
import dayjs from 'dayjs';
import customParseFormat from 'dayjs/plugin/customParseFormat';
dayjs.extend(customParseFormat);
const now = dayjs.utc().format('YYYY-MM-DD')
const intervalDay = dayjs.utc().add(-1, 'month').format('YYYY-MM-DD')
const { Text } = Typography;


const itemRender: PaginationProps['itemRender'] = (_, type, originalElement) => {
  if (type === 'prev') {
    return (
        <Button type='link' icon={<ArrowLeftOutlined />} iconPosition="start" style={{border: '1px solid #d9d9d9', fontWeight: 'bold', alignItems: 'baseline'}}> 
            <span style={{alignSelf: "center"}}>Prev</span>
        </Button>
    );
  }
  if (type === 'next') {
    return (
        <Button type='link' icon={<ArrowRightOutlined />} iconPosition="end" style={{border: '1px solid #d9d9d9', fontWeight: 'bold', alignItems: 'baseline'}}>
            <span style={{alignSelf: "center"}}>Next</span>
            {/* Next */}
        </Button>
    );
  }
  return originalElement;
};

export default function Index () {
    const { config } = useAuth();
    const pageLengthConfig = config?.dataGrid?.rowPerPage || 5;
    const dateConfig = config?.dataGrid?.dateTimeFormat || 'YYYY-MM-DD | HH:mm:ss';
    
    const [data, setData] = useState<any[]>([])
    const [page, setPage] = useState<number>(1)
    const pageLength = pageLengthConfig
    const [totalRow, setTotalRow] = useState<number>(0)
    const [loading, setLoading] = useState(false);
    const [open, setOpen] = useState(false)
    const [detail, setDetail] = useState<any>({})
    const [form] = Form.useForm();

    let queryParams = new URLSearchParams({
        DocumentTitle: '',
        DtFrom: intervalDay,
        DtTo: now,
    });

    useEffect(()=> {
        form.setFieldsValue({from: dayjs.utc(intervalDay), to: dayjs.utc(now)})
        getDocument()
    }, [page])

    useEffect(()=> {
        if (!open) {
            setDetail({})
        }
    }, [open])

    const getDocument = async () => {
        setLoading(true)
        apiClient.get(`/notification/search?${queryParams}&Page=${page}&Limit=${pageLength}`)
        .then(({ data }) => {
            console.log(data.data.data)
            setData(data.data.data)
            setTotalRow(data.data.totalRecords)
            setLoading(false)
        })
        .catch(err => {
            console.log(err)
            setLoading(false)
            setData([])
        })
    }

    const onChange = async () => {
        queryParams = new URLSearchParams({
            DocumentTitle: form.getFieldValue('search') || '',
            DtFrom: form.getFieldValue('from')?.format('YYYY-MM-DD') || '',
            DtTo: form.getFieldValue('to')?.format('YYYY-MM-DD') || '',
        });
        await getDocument()
    };

    const handleKeyDown = async (event: any) => {
        if (event.key === 'Enter' || event.code === 'Enter') {
            queryParams = new URLSearchParams({
                DocumentTitle: form.getFieldValue('search') || '',
                DtFrom: form.getFieldValue('from')?.format('YYYY-MM-DD') || '',
                DtTo: form.getFieldValue('to')?.format('YYYY-MM-DD') || '',
            });
            await getDocument()
        }
    }

    const handleDetailClick = (item: any) => {
        setDetail(item)
        setOpen(true)
    }
    
    return (
        <>
            <Row>
                <Col span={24}>
                    <div className='text-2xl font-bold mb-8'>Notification</div>
                    <Form
                    layout='horizontal'
                    form={form}
                    >
                        <Flex justify='space-between'>
                                <div>
                                    <Form.Item
                                    name="search"
                                    >
                                        <Input size="middle" onKeyDown={handleKeyDown} placeholder="Search ..." prefix={<SearchOutlined />}/>
                                    </Form.Item>
                                </div>
                                <Space size="large">
                                    <Form.Item 
                                    name="from"
                                    label="From"
                                    rules={[{ type: 'date'}]}
                                    >
                                        <DatePicker format='YYYY-MM-DD' placeholder='YYYY-MM-DD'/>
                                    </Form.Item>
                                    <Form.Item
                                    name="to"
                                    label="To"
                                    rules={[{ type: 'date'}]}
                                    >
                                        <DatePicker format='YYYY-MM-DD' placeholder='YYYY-MM-DD' onChange={onChange}/>
                                    </Form.Item>
                                </Space>
                        </Flex>
                    </Form>
                </Col>
            </Row>
            <Row gutter={[24, 24]}>
                <Col span={24}>
                    <List
                    className='costum-list'
                    // header={<div style={{border: 'none'}}>Saturday, 01/01/2001</div>}
                    pagination={{
                        onChange: (page) => {
                            setPage(page);
                        },
                        hideOnSinglePage: true,
                        pageSize: pageLength,
                        total: totalRow,
                        itemRender: itemRender,
                        position: "bottom",
                        align: 'center'
                    }}
                    dataSource={data}
                    loading={loading}
                    renderItem={(item) => 
                    (
                        <List.Item extra={<Button color="purple" variant="solid" onClick={()=> handleDetailClick(item)} icon={<ArrowRightOutlined />} iconPosition="end">Detail</Button>}>
                            <Row style={{width: '100%'}}>
                                <Col span={7} style={{alignContent: 'center'}}>
                                    <Space size="large">
                                        <Avatar size="large" icon={<UserOutlined />} />
                                        {item.actorFullname}
                                    </Space>
                                </Col>
                                <Col span={7} style={{alignContent: 'center'}}>
                                    <Space>
                                        {item.attachment} <span style={{fontWeight: 'bold'}}>{item.documentTitle}</span>
                                    </Space>
                                </Col>
                                <Col span={5} style={{alignContent: 'center'}}>
                                    <div>{item?.notifDescription}</div>
                                </Col>
                                <Col span={5} style={{alignContent: 'center'}}>
                                    {dayjs.utc(item.insertedAt).format(dateConfig)}
                                </Col>
                            </Row>
                        </List.Item>
                    )}
                    >
                    </List>
                </Col>
            </Row>
            {/* modal detail     */}
            <Modal
            title={(
            <div className="space-align-block" style={{paddingLeft: 24, paddingRight: 24, paddingTop: 24}}>
                <Space align="center" size="middle">
                <BellOutlined style={{border: '1px solid #d9d9d9', padding: 7, fontSize: 30, color: '#434343', borderRadius: 7}}/>
                <Space.Compact direction="vertical">
                    <span>Detail</span>
                    <Text type="warning" style={{fontWeight: 'normal'}}>{detail?.insertedAt ? dayjs.utc(detail.insertedAt).format(dateConfig) : ''}</Text>
                </Space.Compact>
                </Space>
            </div>
            )}
            open={open}
            onCancel={()=> setOpen(false)}
            maskClosable={false}
            footer={false}
            width={800}
            styles={{
                content: {
                    padding: 0
                },
            }}
            style={{ top: 20 }}
            >
                <Divider style={{marginBottom: 0}}/>
                <Row gutter={[16, 16]} style={{marginTop: 0, paddingLeft: 30, paddingRight: 30, paddingTop: 10, paddingBottom: 10}}>
                    <Col span={24}>
                        <Card styles={{ body:{padding: 0}}} style={{border: 'none'}}>
                            <p className="font-bold">User</p>
                            <span className="">{detail?.actorFullname || '-'}</span>
                            <Divider style={{margin: '10px 0px'}}></Divider>
                        </Card>
                    </Col>
                    <Col span={24}>
                        <Card styles={{ body:{padding: 0}}} style={{border: 'none'}}>
                            <p className="font-bold">Notification</p>
                            <div>{detail?.notifDescription}</div>
                            <Divider style={{margin: '10px 0px'}}></Divider>
                        </Card>
                    </Col>
                    <Col span={24}>
                        <Card styles={{ body:{padding: 0}}} style={{border: 'none'}}>
                            <p className="font-bold">Document Title</p>
                            <div className="">{detail?.documentTitle || '-'}</div>
                            <div className="">{detail?.documentFileSize || '-'}</div>
                            <Divider style={{margin: '10px 0px'}}></Divider>
                        </Card>
                    </Col>
                    <Col span={24}>
                        <Card styles={{ body:{padding: 0}}} style={{border: 'none'}}>
                            <p className="font-bold">Description</p>
                            <span className="">{detail?.notifDescription || '-'}</span>
                        </Card>
                    </Col>
                </Row>
                <Divider style={{marginTop:0, marginBottom: 0}}/>
                <div style={{padding: 20, paddingBottom: 20, justifySelf: 'end'}}>
                    <Space>
                        <Button icon={<CloseOutlined />} iconPosition="end" onClick={()=> setOpen(false)} color="default" variant="filled">
                        Close
                        </Button>
                    </Space>
                </div>
            </Modal>
        </>
    )
}