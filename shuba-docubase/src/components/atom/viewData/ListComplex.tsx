import { useState } from "react";
import { Button, Card, Col, Dropdown, Flex, List, message, Modal, Row, Tooltip, Typography } from "antd"
import type { MenuProps, PaginationProps } from 'antd';
import {
    ArrowLeftOutlined,
    ArrowRightOutlined,
    CloseOutlined,
    DashOutlined,
    ExclamationCircleOutlined,
    FileExcelFilled,
    FilePdfOutlined,
    FileWordFilled,
    ShareAltOutlined,
  } from '@ant-design/icons';
import { Pencil, Trash2 } from "lucide-react";
import ModalShare from '../share/ModalShareDirect'
import { Link, useNavigate } from "react-router-dom";
import apiClient from "../../../services/apiClient";
const { Paragraph } = Typography;

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

const iconList: any = {
        '.docx': <FileWordFilled style={{fontSize: 45, color: '#1677ff'}}/>,
        '.doc': <FileWordFilled style={{fontSize: 45, color: '#1677ff'}}/>,
        '.xls': <FileExcelFilled style={{fontSize: 45, color: '#52c41a'}}/>,
        '.xlsx': <FileExcelFilled style={{fontSize: 45, color: '#52c41a'}}/>,
        '.pdf': <FilePdfOutlined style={{fontSize: 45, color: '#f5222d'}} />,
}

export default function Index({data, pageLength, page, setPage, total, loading, rerenderData}: any) {
    const navigate = useNavigate();
    const [openShare, setOpenShare] = useState(false)
    const [dataShare, setDataShare] = useState<any>({});
    const [deleteDocument, setDeleteDocument] = useState<any>({});
    const [openConfirmDelete, setOpenConfirmDelete] = useState(false);
    const [confirmDeleteLoading, setConfirmDeleteLoading] = useState(false);

    const handleOpenShare = (items: any) => {
        setDataShare(items.documentId)
        setOpenShare(true)
    }

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

    return (
        <>
            <List
                grid={{
                gutter: 34,
                xs: 2,
                sm: 3,
                md: 3,
                lg: pageLength,
                xl: pageLength,
                xxl: pageLength, 
                }}
                dataSource={data}
                pagination={{
                    onChange: (page) => {
                        setPage(page);
                    },
                    hideOnSinglePage: true,
                    current: page,
                    pageSize: pageLength,
                    total: total,
                    itemRender: itemRender,
                    position:'bottom',
                    align:'center'
                }}
                loading={loading}
                renderItem={(item: any) => {
                const listAction: MenuProps['items'] = [
                    {
                        key: '1',
                        label: (
                        <Tooltip placement="left" title="Share" color='#595959'>
                            <Button type="text" icon={<ShareAltOutlined style={{fontSize: 18, color: '#595959'}}/>} onClick={()=> handleOpenShare(item)}/>
                        </Tooltip>
                        )
                    },
                    {
                        key: '2',
                        label: (
                        <Tooltip placement="left" title="Edit" color='#595959'>
                            <Button type="text" icon={<Pencil size={18} color='#595959'/>} onClick={()=> navigate(`/document/document-edit/${item.documentId}`)}/>
                        </Tooltip>
                        ),
                    },
                    {
                        key: '3',
                        label: (
                        <Tooltip placement="left" title="Delete" color='#595959'>
                            <Button type="text" icon={<Trash2 size={18} color='#595959'/>} onClick={()=> handleDeleteDocument(item)}/>
                        </Tooltip>
                        )
                    }
                ]
                return (
                <List.Item>
                    <Card
                    title= {
                        <Link to={`/document/document-view/${item.documentId}`}>
                           {iconList[item.fileType]}
                        </Link>
                    }
                    style={{height: 150}}
                    styles={{
                        header: {
                            padding: 0,
                            border: 'none',
                            textAlign: 'center'
                        },
                        body: {
                            padding: '10px 10px'
                        }
                    }}
                    >
                        <Flex justify='space-between' gap={10}>
                            <div className="flex flex-col overflow-y-auto h-18">
                                <Paragraph 
                                ellipsis={{ rows: 2, expandable: true, symbol: 'more' }}
                                style={{marginBottom: 0}}
                                className="text-slate-800 font-bold hover:underline"
                                >
                                    {item.documentName}
                                </Paragraph>
                                <span className='text-[11px] text-neutral-500 text-nowrap'>{item.expiryDate}</span>
                            </div>
                            <div>
                                <Dropdown menu={{ items: listAction }}>
                                    <DashOutlined/>
                                </Dropdown>
                            </div>
                        </Flex>
                    </Card>
                </List.Item>
                )}}
            />
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