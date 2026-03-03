import './style.css'
import { Col, Input, Row } from 'antd';
import {
    SearchOutlined,
  } from '@ant-design/icons';
import { useEffect, useState } from 'react';
import { useAuth } from '../../../context/AuthContext';
import apiClient from '../../../services/apiClient';
import TableComplexShared from '../../atom/viewData/TableComplexShared'

const TableList = () => {
    const { config } = useAuth();
    const pageLengthConfig = config?.dataGrid?.rowPerPage || 5;
    const [dataDocument, setDataDocument] = useState<any[]>([])
    const [pageDocument, setPageDocument] = useState<number>(1)
    const pageLengthDocument = pageLengthConfig
    const [loadingDocument, setLoadingDocument] = useState(false)
    const [totalRowDocument, setTotalRowDocument] = useState<number>(0)
    const [searchDocument, setSearchDocument] = useState<string>('')

    const getDataDocument = ()=> {
        setLoadingDocument(true)
        apiClient.get(`/sharedworkspace/mysharedfile?DocumentOrCategoryTitle=${searchDocument}&Page=${pageDocument}&Limit=${pageLengthDocument}`)
        .then(({ data }) => {
            setDataDocument(data.data.data)
            setTotalRowDocument(data.data.totalRecords)
            setLoadingDocument(false)
        })
        .catch(err => {
            console.log(err)
            setLoadingDocument(false)
        })
    }

    useEffect(()=> {
        getDataDocument()
    }, [pageDocument, pageLengthDocument])

    const handleKeyDownDocument = (event: any) => {
        if (event.key === 'Enter' || event.code === 'Enter') {
            const str = searchDocument.length > 0 ? searchDocument.toLocaleLowerCase() : ''
            setSearchDocument(str)
            getDataDocument()
        }
    }

    return (
        <>
            <Row style={{marginBottom: 20}}>
                <Col xs={24} sm={24} md={12} lg={12} xl={6}>
                    <Input size="middle" onChange={(e:any) => setSearchDocument(e.target.value)}  onKeyDown={handleKeyDownDocument} placeholder="Search ..." prefix={<SearchOutlined />}/>
                </Col>
            </Row>
            <Row>
                <Col span={24}>
                    <TableComplexShared type='my_shared' data={dataDocument} setData={setDataDocument} pageSize={pageLengthDocument} page={pageDocument} setPage={setPageDocument} totalRow={totalRowDocument} loading={loadingDocument} rerenderData={getDataDocument}/>
                </Col>
            </Row>
        </>
    )
};

export default TableList;