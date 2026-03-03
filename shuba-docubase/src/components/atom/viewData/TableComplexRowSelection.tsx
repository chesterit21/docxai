import { useState } from "react";
import { Button, Card, Col, Flex, message, Modal, Row, Space, Table, Tooltip, type PaginationProps, type TableProps} from "antd";
import {
    ArrowLeftOutlined,
    ArrowRightOutlined,
    CloseOutlined,
    ExclamationCircleOutlined,
    FileExcelFilled,
    FileImageOutlined,
    FilePdfOutlined,
    FilePptOutlined,
    FileTextOutlined,
    FileWordFilled,
    ShareAltOutlined,
    StarFilled,
} from '@ant-design/icons';
import { Pencil, Trash2 } from "lucide-react";
import ModalShare from '../share/ModalShareDirect'
import { Link, useNavigate } from "react-router-dom";
import { useAuth } from '../../../context/AuthContext';
import apiClient from "../../../services/apiClient";
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

type TableRowSelection<T extends object = object> = TableProps<T>['rowSelection'];

const iconList: any = {
        '.docx': <FileWordFilled style={{fontSize: 35, color: '#1677ff'}}/>,
        '.doc': <FileWordFilled style={{fontSize: 35, color: '#1677ff'}}/>,
        '.xls': <FileExcelFilled style={{fontSize: 35, color: '#52c41a'}}/>,
        '.xlsx': <FileExcelFilled style={{fontSize: 35, color: '#52c41a'}}/>,
        '.pdf': <FilePdfOutlined style={{fontSize: 35, color: '#f5222d'}} />,
        '.ppt': <FilePptOutlined style={{fontSize: 35, color: '#ff7a45'}} />,
        '.pptx': <FilePptOutlined style={{fontSize: 35, color: '#ff7a45'}} />,
        '.png': <FileImageOutlined style={{fontSize: 35, color: '#08979c'}} />,
        '.jpg': <FileImageOutlined style={{fontSize: 35, color: '#08979c'}} />,
        '.jpeg': <FileImageOutlined style={{fontSize: 35, color: '#08979c'}} />,
        '.txt': <FileTextOutlined style={{fontSize: 35, color: '#595959'}} />,
} 

export default function index({data, setData, setSelectedRowKeys, pageSize = 5, page, setPage, totalRow, loading = false, rerenderData}: any) {
    const navigate = useNavigate();
    const { config } = useAuth();
    const dateConfig = config?.dataGrid?.dateTimeFormat || 'YYYY-MM-DD | HH:mm:ss';
    const [openShare, setOpenShare] = useState(false);
    const [deleteDocument, setDeleteDocument] = useState<any>({});
    const [openConfirmDelete, setOpenConfirmDelete] = useState(false);
    const [confirmDeleteLoading, setConfirmDeleteLoading] = useState(false);
    const [dataShare, setDataShare] = useState<any>({});


    const columns = [
        {
            title: 'Document Name',
            dataIndex: 'documentName',
            key: 'documentName',
            render: (items: any, row: any) => (
                <Flex gap="small" align='center'>
                    <Button type="link" onClick={()=> toggleFavorite(row)} icon={row.isFavorite ? <StarFilled style={{fontSize:25, color: '#fa8c16'}} /> :  <StarFilled style={{fontSize:25, color: '#d9d9d9'}} />}/>
                    <Link to={`/document/document-view/${row.documentId}`}>
                        <Flex gap="small" align='center'>
                                {iconList[row.fileType]}
                                <Flex vertical>
                                    <span className="text-slate-800 font-bold hover:underline">{items}</span>
                                    <span className="text-slate-500 text-nowrap">{row.expiryDate}</span>
                                </Flex>
                        </Flex>
                    </Link>
                </Flex>
            ),
        },
        // {
        //     title: 'Document ID',
        //     dataIndex: 'documentId',
        //     key: 'documentId',
        //     render: (items: any) => (
        //         <Link to={`/document/document-view/${items}`}>
        //             <span className="text-slate-800 font-bold hover:underline">{items}</span>
        //         </Link>
        //     ),
        // },
        {
            dataIndex: 'description',
            key: 'description',
            title: 'Description',
            width: '15%'
        },
        {
            title: 'Owner',
            key: 'insertedByUserName',
            dataIndex: 'insertedByUserName',
            render: (items: any, row: any) => (
                <Flex vertical>
                    <span style={{fontWeight: 'bold'}}>{items}</span>
                    <span>{ dayjs.utc(row.insertedAt).format(dateConfig) }</span>
                </Flex>
            ),
        },
        {
            title: 'Last Update',
            key: 'updatedByUserName',
            dataIndex: 'updatedByUserName',
            render: (items: any, row: any) => (
                <Flex vertical>
                    <span style={{fontWeight: 'bold'}}>{items}</span>
                    <span>{ dayjs.utc(row.lastUpdateDate).format(dateConfig) }</span>
                </Flex>
            ),
        },
        {
            key: 'filesSize',
            dataIndex: 'filesSize',
            title: 'Size',
        },
        {
            title: 'Action',
            key: 'action',
            render: (_: any, row: any) => (
            <Space size="middle">
                <Tooltip placement="bottom" title="Share" color='#595959'>
                    <Button type="text" icon={<ShareAltOutlined style={{fontSize: 18, color: '#595959'}}/>} onClick={()=> handleOpenShare(row)}/>
                </Tooltip>
                <Tooltip placement="bottom" title="Edit" color='#595959'>
                    <Button type="text" icon={<Pencil size={18} color='#595959'/>} onClick={()=> navigate(`/document/document-edit/${row.documentId}`)}/>
                </Tooltip>
                <Tooltip placement="bottom" title="Delete" color='#595959'>
                    <Button type="text" icon={<Trash2 size={18} color='#595959'/>} onClick={()=> handleDeleteDocument(row)}/>
                </Tooltip>
            </Space>
            ),
        },

    ];

    const handleOpenShare = (items: any) => {
        setDataShare(items.documentId)
        setOpenShare(true)
    }

    async function toggleFavorite(record: any) {
        const newData = data.map((row: any) => row.documentId == record.documentId ? {...row, isFavorite: !row.isFavorite } : row );
        console.log(newData)
        if(!record.isFavorite) {
            await apiClient.put(`/documents/add-favorite?documentId=${record.documentId}`)
            .then(({ data }) => {
                console.log(data)
                message.success('Add to favorite successfully')
                setData(newData)
            })
            .catch(err => {
                console.log(err)
                message.error(err.response.data.message)
            })  
        } else {
            await apiClient.put(`/documents/un-favorite?documentId=${record.documentId}`)
            .then(({ data }) => {
                console.log(data)
                message.success('Remove from favorite successfully')
                setData(newData)
            })
            .catch(err => {
                console.log(err)
                message.error(err.response.data.message)
            })  
        }
    }

    const handleTableChange: TableProps<any>['onChange'] = (pagination) => {
        setPage(Number(pagination.current))
    };

    const handleDeleteDocument = (item: any) => {
        setDeleteDocument(item)
        setOpenConfirmDelete(true);
    }

    const handleOkDelete = () => {
        if (Object.keys(deleteDocument).length == 0) return message.error('Document ID not found!')
        setConfirmDeleteLoading(true);
        apiClient.put(`/documents/delete?id=${deleteDocument.documentId}`)
        .then(({ data }) => {
            console.log(data)
            setConfirmDeleteLoading(false);
            setOpenConfirmDelete(false);
            message.success('Delete data is successful')
            rerenderData()
        })
        .catch(err => {
            console.log(err)
            setConfirmDeleteLoading(false)
            setOpenConfirmDelete(false);
            message.error(err.response.data.message)
        })
    }

    const onSelectChange = (newSelectedRowKeys: React.Key[]) => {
        console.log('selectedRowKeys changed: ', newSelectedRowKeys);
        setSelectedRowKeys(newSelectedRowKeys);
    };

    const rowSelection: TableRowSelection<any> = {
        onChange: onSelectChange,
    };

    return (
        <>
            <Card styles={{ body: { padding: 0 } }}>
                <Table rowKey="documentId" onChange={handleTableChange}  rowSelection={rowSelection} scroll={{ x: 1000 }} pagination={ {position:['bottomCenter'], itemRender: itemRender, current: page, pageSize:pageSize, total: totalRow, showSizeChanger:false} } columns={columns} dataSource={data} loading={loading}/>
            </Card>
            <ModalShare open={openShare} setOpen={setOpenShare} document={dataShare}/>
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
                    <p className="font-bold text-lg" style={{marginBottom: 0}}>Are you sure you want to delete this Document?</p>
                    <span className="text-gray-500">Deleting this Document will permanently delete all items within it. Make sure you have backed up important data before continuing..</span>
                </div>
            </Modal>
        </>
    )
}