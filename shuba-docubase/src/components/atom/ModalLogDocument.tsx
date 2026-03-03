import { Modal, Space, Typography, Card, Table, type PaginationProps, Button, type TableProps, DatePicker, Divider } from "antd";
import {
    ArrowLeftOutlined,
    ArrowRightOutlined,
    DownloadOutlined,
    FileSearchOutlined,
    SearchOutlined,
} from '@ant-design/icons';
import { useEffect, useState } from "react";
import { useAuth } from "../../context/AuthContext";
import apiClient from "../../services/apiClient";
import * as XLSX from 'xlsx';
import { saveAs } from 'file-saver';
import dayjs from 'dayjs';
import customParseFormat from 'dayjs/plugin/customParseFormat';
dayjs.extend(customParseFormat);

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

export default function Index({open, setOpen, documentId}: any) {
    const { config } = useAuth();
    const dateConfig = config?.dataGrid?.dateTimeFormat || 'YYYY-MM-DD | HH:mm:ss';
    const pageLengthConfig = config?.dataGrid?.rowPerPage || 5;

    const [data, setData] = useState<any[]>([])
    const [page, setPage] = useState<number>(1)
    const pageLengthDocument = pageLengthConfig 
    
    const [totalRow, setTotalRow] = useState<number>(0)
    const [loading, setLoading] =  useState(false);
    const [loadingDownload, setLoadingDownload] =  useState(false);
    const [startDate, setStartDate] = useState<string | null>(null);
    const [endDate, setEndDate] = useState<string | null>(null);
    
    const columns = [
        {
            dataIndex: 'insertedAt',
            key: 'insertedAt',
            title: 'Time',
            onCell: () => ({
                style: { verticalAlign: "top" }
            }),
            render: (items: any) => <span>{ dayjs.utc(items).format(dateConfig) }</span>
        },
        {
            key: 'insertedByFullName',
            dataIndex: 'insertedByFullName',
            title: 'User',
            onCell: () => ({
                style: { verticalAlign: "top" }
            })
        },
        {
            dataIndex: 'actionLogDocument',
            key: 'actionLogDocument',
            title: 'Action',
            onCell: () => ({
                style: { verticalAlign: "top" }
            })
        },
        {
            dataIndex: 'before',
            key: 'before',
            title: 'Before',
            width: '30%',
            onCell: () => ({
                style: { verticalAlign: "top" }
            }),
            render: (items: any) => transformData(items)
        },
        {
            dataIndex: 'after',
            key: 'after',
            title: 'After',
            width: '30%',
            onCell: () => ({
                style: { verticalAlign: "top" }
            }),
            render: (items: any) => transformData(items)
        },

    ];

    useEffect(()=> {
        if(open){
            getData()
        }
    }, [open, page])


    const getData = ()=> {
        setLoading(true)
        let url = `/documents/get-document-logs?DocumentId=${documentId}&Page=${page}&Limit=${pageLengthConfig}`;
        if(startDate) url += `&StartDate=${startDate}`;
        if(endDate) url += `&EndDate=${endDate}`;

        apiClient.get(url)
        .then(({ data }) => {
            let res = data?.data?.data || []
            setData(res)
            setTotalRow(data?.data?.totalRecords || 0)
            setLoading(false)
        })
        .catch(err => {
            console.log(err)
            setLoading(false)
        })
    }

    const handleClickFilter = () => {
        setPage(1);
        getData();
    };

    const handleTableChange: TableProps<any>['onChange'] = (pagination) => {
        setPage(Number(pagination.current))
    };

    const transformData = (data: any) => {
        if (!data) return null;

        const parsed = JSON.parse(data);

        return (
            <pre style={{ whiteSpace: "pre-wrap", margin: 0 }}>
            {parsed
                .map((item: any) =>
                Object.entries(item)
                    .map(([key, value]) => `${key}: ${value ?? "null"}`)
                    .join("\n")
                )
                .join("\n")}
            </pre>
        );
    };

    const handleClickDownload = async () => {
        setLoadingDownload(true)
        apiClient.get(`/documents/get-document-logs?DocumentId=${documentId}&Page=1&Limit=100000`)
        .then(({ data }) => {
            let dd = data?.data?.data || []
            // Create a new workbook and worksheet
            const worksheet = XLSX.utils.json_to_sheet(transformDataExport(dd));
            const workbook = XLSX.utils.book_new();
            XLSX.utils.book_append_sheet(workbook, worksheet, 'Sheet1');
            
            // Create a binary Excel file
            const excelBuffer = XLSX.write(workbook, { bookType: 'xlsx', type: 'array' });
            
            // Use FileSaver to save the file
            const blob = new Blob([excelBuffer], { type: 'application/octet-stream' });
            saveAs(blob, `log-audit.xlsx`);
            setLoadingDownload(false)
        })
        .catch(err => {
            console.log(err)
            setLoadingDownload(false)
        })
    }

    const transformDataExport = (data:any) => {
        return data.map((entry: any) => ({
            ...entry,
            before: entry.before
            ? JSON.parse(entry.before)
                .map((item: any) =>
                    Object.entries(item)
                    .map(([key, value]) => `${key}: ${value ?? 'null'}`)
                    .join('\n')
                )
                .join('\n')
            : null,
            after: entry.after
            ? JSON.parse(entry.after)
                .map((item: any) =>
                    Object.entries(item)
                    .map(([key, value]) => `${key}: ${value ?? 'null'}`)
                    .join('\n')
                )
                .join('\n')
            : null,
        }));
    };

    return (
        <>
            <Modal
            title={(
            <div className="space-align-block" style={{paddingLeft: 24, paddingRight: 24, paddingTop: 24}}>
                <Space align="center" size="middle">
                <FileSearchOutlined style={{border: '1px solid #d9d9d9', padding: 7, fontSize: 30, color: '#434343', borderRadius: 7}}/>
                <Space.Compact direction="vertical">
                    <span>Audit Log</span>
                    <Text type="secondary" style={{fontWeight: 'normal'}}>Below is the data from this audit log document.</Text>
                </Space.Compact>
                </Space>
            </div>
            )}
            open={open}
            onCancel={()=> setOpen(false)}
            maskClosable={false}
            footer={false}
            width={1100}
            styles={{
                content: {
                    padding: 0
                },
            }}
            style={{ top: 20 }}
            >
                <Divider className="!mb-0"/>
                <div className='pt-0'>
                    <div className="flex flex-col md:flex-row md:items-center justify-between gap-4 py-3 px-5 mb-3">
                        <div className="flex items-end gap-3">
                            <div className="flex flex-col">
                                <span>From</span>
                                <DatePicker format='YYYY-MM-DD' placeholder='YYYY-MM-DD' onChange={(val) => setStartDate(val ? dayjs.utc(val).format("YYYY-MM-DD") : null)}/>
                            </div>
                            <div className="flex flex-col">
                                <span>To</span>
                                <DatePicker format='YYYY-MM-DD' placeholder='YYYY-MM-DD' onChange={(val) => setEndDate(val ? dayjs.utc(val).format("YYYY-MM-DD") : null)}/>
                            </div>
                            <Button icon={<SearchOutlined />} type="primary" iconPosition="start" onClick={handleClickFilter} loading={loading}>
                                Filter
                            </Button>
                        </div>
                        <div>
                            <Button icon={<DownloadOutlined />} iconPosition="end" onClick={handleClickDownload} loading={loadingDownload}>
                                Download
                            </Button>
                        </div>
                    </div>
                    <Card styles={{body: { padding: 0, height: 650 }}}>
                        <Table rowKey="id" onChange={handleTableChange} scroll={{ y: 510 }} pagination={ {position:['bottomCenter'], itemRender: itemRender, current: page, pageSize:pageLengthDocument, total: totalRow, showSizeChanger:false} } columns={columns} dataSource={data} loading={loading} />
                    </Card>
                </div>
            </Modal>
        </>
    )
}