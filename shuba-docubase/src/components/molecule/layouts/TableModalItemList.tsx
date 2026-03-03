import { Badge, Button, Flex, Table, Tooltip } from 'antd';
import type { TableProps } from 'antd';
import {
FileExcelFilled,
  FilePdfOutlined,
  FileWordFilled,
  FolderFilled,
  QuestionCircleOutlined,
  StarFilled,
  } from '@ant-design/icons';
import { useEffect, useState } from 'react';

type TableRowSelection<T extends object = object> = TableProps<T>['rowSelection'];

interface DataType {
  key: string;
  id: number;
  name: string[];
  description: string;
  owners: string[];
  updated_at: string[];
  size: string;
  isFavorite: boolean;
}

const icon: any = {
    excel: <FileExcelFilled style={{fontSize: 37, color: '#52c41a'}}/>,
    word: <FileWordFilled style={{fontSize: 37, color: '#1677ff'}}/>,
    pdf: <FilePdfOutlined style={{fontSize: 37, color: '#f5222d'}} />,
    none: <FolderFilled style={{fontSize: 37, color: '#9254de'}}/>
}
const dummy: DataType[] = [
    {
        key: '1',
        name: ['excel', 'Document 1', 'Expired in 3 days'],
        id: 123,
        description: 'Lorem, ipsum dolor sit amet consectetur adipisicing elit. Molestiae et eos dolore fugit accusantium quo velit magni eum atque animi!',
        owners: ['Fulan 1', '22/2/2022 | 00:00'],
        updated_at: ['Fulan 1', '22/2/2022 | 00:00'],
        size: '1 MB',
        isFavorite: true
    },
    {
        key: '2',
        name: ['pdf', 'Document 2', 'Expired in 5 days'],
        id: 456,
        description: 'Lorem, ipsum dolor sit amet consectetur adipisicing elit. Molestiae et eos dolore fugit accusantium quo velit magni eum atque animi!',
        owners: ['Fulan 1', '22/2/2022 | 00:00'],
        updated_at: ['Fulan 1', '22/2/2022 | 00:00'],
        size: '1 MB',
        isFavorite: true
    },
    {
        key: '3',
        name: ['word', 'Document 3', 'Update 22/2/2022'],
        id: 789,
        description: 'Lorem, ipsum dolor sit amet consectetur adipisicing elit. Molestiae et eos dolore fugit accusantium quo velit magni eum atque animi!',
        owners: ['Fulan 1', '22/2/2022 | 00:00'],
        updated_at: ['Fulan 1', '22/2/2022 | 00:00'],
        size: '1 MB',
        isFavorite: false
    },
    {
        key: '4',
        name: ['none', 'Document 4', 'Update 22/2/2022'],
        id: 987,
        description: 'Lorem, ipsum dolor sit amet consectetur adipisicing elit. Molestiae et eos dolore fugit accusantium quo velit magni eum atque animi!',
        owners: ['Fulan 1', '22/2/2022 | 00:00'],
        updated_at: ['Fulan 1', '22/2/2022 | 00:00'],
        size: '1 MB',
        isFavorite: false
    },
];

const TableItemList = () => {
    const [data, setData] = useState<DataType[]>([])
    const [selectedRowKeys, setSelectedRowKeys] = useState<React.Key[]>([]);
    const columns: TableProps<DataType>['columns'] = [
        {
            title: 'Document Name',
            dataIndex: 'name',
            key: 'name',
            render: (items: any, row: any) => (
                <Flex gap="small" align='center'>
                    <Button type="link" onClick={()=> toggleFavorite(row)} icon={row.isFavorite ? <StarFilled style={{fontSize:25, color: '#fa8c16'}} /> :  <StarFilled style={{fontSize:25, color: '#d9d9d9'}} />}/>
                    {icon[items[0]]}
                    <Flex vertical>
                        <span>{items[1]}</span>
                        <span style={{textWrap: 'nowrap'}}>{items[2]}</span>
                    </Flex>
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
            render: (items) => (
                <Flex vertical>
                    <span style={{fontWeight: 'bold'}}>{items[0]}</span>
                    <span>{items[1]}</span>
                </Flex>
            ),
        },
        {
            title: (
                <Badge 
                count= {
                    (
                        <Tooltip placement="top" title="Did you know?" color='#595959'>
                            <QuestionCircleOutlined style={{color: '#8c8c8c', marginRight: -10}}/>
                        </Tooltip>

                    )
                }  
                >
                    Last Update
                </Badge>
            ),
            key: 'updated_at',
            dataIndex: 'updated_at',
            render: (items ) => (
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

    ];

    const onSelectChange = (newSelectedRowKeys: React.Key[]) => {
        console.log('selectedRowKeys changed: ', newSelectedRowKeys);
        setSelectedRowKeys(newSelectedRowKeys);
    };

    const rowSelection: TableRowSelection<DataType> = {
        selectedRowKeys,
        onChange: onSelectChange,
    };

    useEffect(()=> {
        setData(dummy)
    }, [])

    function toggleFavorite(record: any) {
        const newData = data.map((row: any) => row.id == record.id ? {...row, isFavorite: !row.isFavorite } : row );
        setData(newData)
        
    }

    return (
        <Table<DataType> rowSelection={rowSelection} scroll={{ x: 1000 }} pagination={false} columns={columns} dataSource={data} />
    )
};

export default TableItemList;