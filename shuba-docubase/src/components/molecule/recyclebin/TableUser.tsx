import './style.css'
import { Avatar, Button, Card, Col, Flex, Input, Modal, Row, Space, Table, Tooltip, message } from 'antd';
import type { TableProps } from 'antd';
import {
    ArrowLeftOutlined,
    ArrowRightOutlined,
    CloseOutlined,
    ExclamationCircleOutlined,
    SearchOutlined,
    UserOutlined,
  } from '@ant-design/icons';
import { useEffect, useState } from 'react';
import { BrushCleaning, RefreshCcw, Trash2 } from 'lucide-react';
import type { PaginationProps } from 'antd';
import { useAuth } from '../../../context/AuthContext';
import apiClient from '../../../services/apiClient';
import dayjs from 'dayjs';
import customParseFormat from 'dayjs/plugin/customParseFormat';

dayjs.extend(customParseFormat);

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



const TableList = () => {
    const { config } = useAuth();
    const pageLengthConfig = config?.dataGrid?.rowPerPage || 5;
    const dateConfig = config?.dataGrid?.dateTimeFormat || 'YYYY-MM-DD | HH:mm:ss';
    const [data, setData] = useState<any[]>([])
    const [loading, setLoading] = useState(false);
    const [page, setPage] = useState<number>(1)
    const pageLength = pageLengthConfig
    const [totalRow, setTotalRow] = useState<number>(0)
    const [eventData, setEventData] = useState<any>({})
    const [openConfirmDelete, setOpenConfirmDelete] = useState(false);
    const [confirmDeleteLoading, setConfirmDeleteLoading] = useState(false);
    const [openConfirmEmpty, setOpenConfirmEmpty] = useState(false);
    const [loadingEmpty, setLoadingEmpty] = useState(false)

    const [search, setSearch] = useState<string>('')

    const columns = [
        {
            key: 'fullName',
            dataIndex: 'fullName',
            title: 'User Name',
            render: (items: any, row: any) => (
                <Flex gap="small" align='center'>
                    <Avatar size="large" icon={<UserOutlined />} />
                    <Flex vertical>
                        <span className='font-bold'>{items}</span>
                        <span style={{textWrap: 'nowrap'}}>{row.emailAddress}</span>
                    </Flex>
                </Flex>
            ),
        },
        {
            title: 'Username',
            dataIndex: 'userName',
            key: 'userName',
        },
        {
            title: 'User Type',
            dataIndex: 'userType',
            key: 'userType',
        },
        {
            title: 'Status',
            dataIndex: 'userStatus',
            key: 'userStatus',
        },
        {
            title: 'Deleted By',
            dataIndex: 'updatedByFullName',
            key: 'updatedByFullName',
        },
        {
            title: 'Deleted At',
            dataIndex: 'updatedAt',
            key: 'updatedAt',
            render: (items: any) => (
                <Flex vertical>
                    <span>{ items ? dayjs.utc(items).format(dateConfig) : '' }</span>
                </Flex>
            ),
        },
        {
            title: 'Action',
            key: 'action',
            render: (_: any, row: any) => (
            <Space size="middle">
                <Tooltip placement="bottom" title="Restore" color='#595959'>
                    <Button type="text" icon={<RefreshCcw size={18} color='#595959'/>} onClick={()=> handleClickRestore(row)}/>
                </Tooltip>
                <Tooltip placement="bottom" title="Delete Permanent" color='#595959'>
                    <Button type="text" icon={<Trash2 size={18} color='#595959'/>} onClick={()=> handleDeleteDocument(row)}/>
                </Tooltip>
            </Space>
            ),
        },

    ];

    useEffect(()=> {
        getData()
    }, [page, pageLength])

    const getData = async ()=> {
        setLoading(true)
       await apiClient.get(`/user/recyclebin?Username=${search}&Page=${page}&Limit=${pageLength}`)
        .then(({ data }) => {
            setData(data?.data?.data || [])
            setTotalRow(data.data.totalRecords)
            setLoading(false)
        })
        .catch(err => {
            console.log(err)
            setLoading(false)
        })
    }


    const handleClickRestore = async (value: any) => {
        const id = value.userId
        if (!id) return message.error('ID not found')
        await apiClient.put(`/user/undelete?userId=${id}`)
        .then(({ data }) => {
            console.log(data)
            getData()
            message.success('Restore data is successful')
        })
        .catch(err => {
            console.log(err)
            message.error('Failed restore data')
        })
    }

    const handleDeleteDocument = (item: any) => {
        setEventData(item)
        setOpenConfirmDelete(true);
    }

    const handleOkDelete = async () => {
        if (Object.keys(eventData).length == 0) return message.error('User ID not found!')
        setConfirmDeleteLoading(true);
        await apiClient.delete(`/user/hard-delete?id=${eventData.userId}`)
        .then(({ data }) => {
            console.log(data)
            setEventData({})
            setConfirmDeleteLoading(false);
            setOpenConfirmDelete(false);
            message.success('Delete data is successful')
            getData()
        })
        .catch(err => {
            console.log(err)
            setConfirmDeleteLoading(false)
            setOpenConfirmDelete(false);
            message.error('Failed delete data')
        })
    }

    const handleEmptyOk = () => {
        setLoadingEmpty(true)
        apiClient.delete(`/user/empty-recyclebin`)
        .then(({ data }) => {
            setLoadingEmpty(false)
            console.log(data)
            setOpenConfirmEmpty(false);
            getData();
            message.success('Empty data is successful')
        })
        .catch(err => {
            console.log(err, 'error delete')
            setLoadingEmpty(false)
            setOpenConfirmEmpty(false);
            message.error(err?.response?.data?.title || 'Failed to delete')
        })
    };
    
    const handleEmpty = () => {
        setOpenConfirmEmpty(true);
    }

    const handleTableChange: TableProps<any>['onChange'] = (pagination) => {
        setPage(Number(pagination.current))
    };

    const handleKeyDownDocument = (event: any) => {
        if (event.key === 'Enter' || event.code === 'Enter') {
            const str = search.length > 0 ? search : ''
            setSearch(str)
            getData()
        }
    }

    return (
        <>
            <div className="flex flex-col md:flex-row justify-between gap-4 mb-5"> 
                <Input size="middle" className='md:w-1/5!' onChange={(e:any) => setSearch(e.target.value)}  onKeyDown={handleKeyDownDocument} placeholder="Search ..." prefix={<SearchOutlined />}/>
                <Button type="primary" icon={<BrushCleaning size={14}/>} iconPosition="end" danger onClick={handleEmpty}>
                    Empty
                </Button>
            </div>
            <Row>
                <Col span={24}>
                    <Card
                    styles={{
                        body: {
                            padding: 0
                        }
                    }}
                    >
                        <Table rowKey="userId" onChange={handleTableChange} scroll={{ x: 1100 }} pagination={ {position:['bottomCenter'], itemRender: itemRender, pageSize:pageLength, total: totalRow}} columns={columns} dataSource={data} loading={loading}/>
                    </Card>
                </Col>
            </Row>

            {/* modal confirm delete */}
            <Modal
                title={(
                    <div style={{paddingLeft: 24, paddingRight: 24, paddingTop: 24}}>
                        <ExclamationCircleOutlined style={{fontSize: 30, color: '#f5222d'}}/>
                    </div>
                )}
                open={openConfirmDelete}
                onCancel={()=> setOpenConfirmDelete(false)}
                maskClosable={false}
                footer={(_,) => (
                <>
                    <Row gutter={12}>
                        <Col span={12}>
                            <Button icon={<CloseOutlined />} iconPosition="end" onClick={()=> setOpenConfirmDelete(false)} variant="outlined" color="danger" block>
                            Cancel
                            </Button>
                        </Col>
                        <Col span={12}>
                            <Button variant="solid" color='danger' icon={<Trash2 size={18}/>} iconPosition="end" onClick={handleOkDelete}  loading={confirmDeleteLoading} block>
                            Delete
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
                    <p className="font-bold text-lg" style={{marginBottom: 0}}>Are you sure you want to delete permanent this user?</p>
                    <span className="text-gray-500">Deleting this user will permanently delete all items within it. Make sure this action before continuing..</span>
                </div>
            </Modal>
            {/* modal confirm empty */}
            <Modal
                title={(
                    <div style={{paddingLeft: 24, paddingRight: 24, paddingTop: 24}}>
                        <ExclamationCircleOutlined style={{fontSize: 30, color: 'red'}}/>
                    </div>
                )}
                open={openConfirmEmpty}
                onCancel={()=> setOpenConfirmEmpty(false)}
                maskClosable={false}
                footer={(_,) => (
                <>
                    <Row gutter={12}>
                        <Col span={12}>
                            <Button icon={<CloseOutlined />} iconPosition="end" onClick={()=> setOpenConfirmEmpty(false)} variant="filled" block>
                                Cancel
                            </Button>
                        </Col>
                        <Col span={12}>
                            <Button variant="solid" color='danger' icon={<Trash2 size={18}/>} iconPosition="end" onClick={handleEmptyOk} block loading={loadingEmpty}>
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
                    <p className="font-bold text-lg" style={{marginBottom: 0}}>Are you sure you want to empty data?</p>
                    <span className="text-gray-500">Make sure this action will empty the data.</span>
                </div>
            </Modal>
        </>
    )
};

export default TableList;