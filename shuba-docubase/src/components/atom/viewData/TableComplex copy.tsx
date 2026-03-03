import { useState } from "react";
import { Button, Card, Flex, Space, Table, Tooltip, type PaginationProps} from "antd";
import {
    ArrowLeftOutlined,
    ArrowRightOutlined,
    FileExcelFilled,
    FilePdfOutlined,
    FileWordFilled,
    ShareAltOutlined,
    StarFilled,
} from '@ant-design/icons';
import { Pencil, Trash2 } from "lucide-react";
import ModalShare from '../share/ModalShare'
import { Link } from "react-router-dom";

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
        excel: <FileExcelFilled style={{fontSize: 35, color: '#52c41a'}}/>,
        word: <FileWordFilled style={{fontSize: 35, color: '#1677ff'}}/>,
        pdf: <FilePdfOutlined style={{fontSize: 35, color: '#f5222d'}} />,
}

export default function index({data, setData, selectedRow, setSelectedRow}: any) {
    const [openShare, setOpenShare] = useState(false)
        
    const columns = [
        {
            title: 'Document Name',
            dataIndex: 'name',
            key: 'name',
            render: (items: any, row: any) => (
                <Flex gap="small" align='center'>
                    <Button type="link" onClick={()=> toggleFavorite(row)} icon={row.isFavorite ? <StarFilled style={{fontSize:25, color: '#fa8c16'}} /> :  <StarFilled style={{fontSize:25, color: '#d9d9d9'}} />}/>
                    <Link to={`/document/document-view/${items[1]}`}>
                        <Flex gap="small" align='center'>
                                {iconList[items[0]]}
                                <Flex vertical>
                                    <span className="text-slate-800 font-bold">{items[1]}</span>
                                    <span className="text-slate-500 text-nowrap">{items[2]}</span>
                                </Flex>
                        </Flex>
                    </Link>
                </Flex>
            ),
        },
        {
            title: 'Document ID',
            dataIndex: 'id',
            key: 'id',
        },
        {
            title: 'Description',
            dataIndex: 'description',
            key: 'description',
            width: '35%'
        },
        {
            title: 'Owner',
            key: 'owners',
            dataIndex: 'owners',
            render: (items: any) => (
                <Flex vertical>
                    <span style={{fontWeight: 'bold'}}>{items[0]}</span>
                    <span>{items[1]}</span>
                </Flex>
            ),
        },
        {
            title: 'Last Update',
            key: 'updated_at',
            dataIndex: 'updated_at',
            render: (items: any) => (
                <Flex vertical>
                    <span style={{fontWeight: 'bold'}}>{items[0]}</span>
                    <span>{items[1]}</span>
                </Flex>
            ),
        },
        {
            title: 'Size',
            dataIndex: 'size',
            key: 'size',
        },
        {
            title: 'Action',
            key: 'action',
            render: () => (
            <Space size="middle">
                <Tooltip placement="bottom" title="Share" color='#595959'>
                    <Button type="text" icon={<ShareAltOutlined style={{fontSize: 18, color: '#595959'}}/>} onClick={()=> setOpenShare(true)}/>
                </Tooltip>
                <Tooltip placement="bottom" title="Edit" color='#595959'>
                    <Button type="text" icon={<Pencil size={18} color='#595959'/>} />
                </Tooltip>
                <Tooltip placement="bottom" title="Delete" color='#595959'>
                    <Button type="text" icon={<Trash2 size={18} color='#595959'/>} />
                </Tooltip>
            </Space>
            ),
        },

    ];

    function toggleFavorite(record: any) {
        const newData = data.map((row: any) => row.id == record.id ? {...row, isFavorite: !row.isFavorite } : row );
        setData(newData)
    }
    const onSelectChange = (newSelectedRowKeys: any) => {
        console.log('selectedRowKeys changed: ', newSelectedRowKeys);
        setSelectedRow(newSelectedRowKeys);
    };

    const rowSelection = {
        selectedRow,
        onChange: onSelectChange,
    };
    return (
        <>
            <ModalShare open={openShare} setOpen={setOpenShare} />
            <Card styles={{ body: { padding: 0 } }}>
                <Table rowSelection={rowSelection} scroll={{ x: 1000 }} pagination={ {position:['bottomCenter'], itemRender: itemRender, pageSize:5}} columns={columns} dataSource={data} />
            </Card>
            
        </>
    )
}