import { Button, Col, Divider, message, Modal, Row, Space, Typography } from 'antd';
import {
  CloseOutlined,
  DownloadOutlined,
    ExclamationCircleOutlined,
    MailOutlined
  } from '@ant-design/icons';
import { BrushCleaning, ListTodo, Trash2 } from 'lucide-react';

// import TableItemList from './TableModalItemList'
import { useEffect, useState } from 'react';
import apiClient from '../../../services/apiClient';
import { useAuth } from '../../../context/AuthContext';
import TableComplex from '../../atom/viewData/TableComplexItemList'
import * as XLSX from 'xlsx';
import { saveAs } from 'file-saver';

const { Text } = Typography;

const ModalItemList = ({open, setOpen}: any) => {
    const { config } = useAuth();
    const pageLengthConfig = config?.dataGrid?.rowPerPage || 5;

    const [data, setData] = useState<any[]>([])
    const [page, setPage] = useState<number>(1)
    const pageLength = pageLengthConfig
    const [totalRow, setTotalRow] = useState<number>(0)
    const [loading, setLoading] = useState(false);
    const [selectedRowKeys, setSelectedRowKeys] = useState<React.Key[]>([]);
    const [openSendEmail, setOpenSendEmail] = useState<boolean>(false)
    const [openConfirmEmpty, setOpenConfirmEmpty] = useState(false);
    const [loadingEmpty, setLoadingEmpty] = useState(false)


  useEffect(()=>{
    if (open) {
        getDocument()
    }
  }, [open, page, pageLength])

  const getDocument = () => {
      setLoading(true)
      apiClient.get(`/top/itemlist?Page=${page}&Limit=${pageLength}`)
      .then(({ data }) => {
          setData(data.data.data)
          setTotalRow(data.data.totalRecords)
          setLoading(false)
          setSelectedRowKeys([])
      })
      .catch(err => {
          console.log(err)
          setLoading(false)
      })
  }

  const handleClickDownload = async () => {
      const dd = data?.filter((a: any) =>
              selectedRowKeys.some((id: any) => id === a.documentId)
      ) || []
      // Create a new workbook and worksheet
      const worksheet = XLSX.utils.json_to_sheet(dd);
      const workbook = XLSX.utils.book_new();
      XLSX.utils.book_append_sheet(workbook, worksheet, 'Sheet1');
      
      // Create a binary Excel file
      const excelBuffer = XLSX.write(workbook, { bookType: 'xlsx', type: 'array' });
      
      // Use FileSaver to save the file
      const blob = new Blob([excelBuffer], { type: 'application/octet-stream' });
      saveAs(blob, `item-list.xlsx`);
  }

    const handleEmptyOk = () => {
        setLoadingEmpty(true)
        apiClient.post(`/top/empty-itemlist`)
        .then(({ data }) => {
            setLoadingEmpty(false)
            console.log(data)
            setOpenConfirmEmpty(false);
            getDocument();
            message.success('Empty data is successful')
        })
        .catch(err => {
            console.log(err, 'error delete')
            setLoadingEmpty(false)
            setOpenConfirmEmpty(false);
            message.error(err.response.data.title)
        })
    };
  
  return (
    <>
      <Modal
        title={(
          <div className="space-align-block" style={{paddingLeft: 24, paddingRight: 24, paddingTop: 24}}>
            <Space align="center" size="middle">
              <ListTodo size={44} color='#434343' className="mock-block" style={{border: '1px solid #d9d9d9', padding: 7, borderRadius: 7}}/>
              <Space.Compact direction="vertical">
                <span>Item List</span>
                <Text type="secondary" style={{fontWeight: 'normal'}}>Here is the list of items you have selected</Text>
              </Space.Compact>
            </Space>
          </div>
        )}
        destroyOnHidden
        open={open}
        maskClosable={false}
        onCancel={()=> setOpen(false)}
        footer={(_,) => (
          <>
            <Button icon={<MailOutlined />} iconPosition="end" onClick={()=> setOpenSendEmail(true)} disabled={selectedRowKeys.length > 0 ? false : true}>
              Send Email
            </Button>
            <Button type="primary" icon={<DownloadOutlined />} iconPosition="end" onClick={handleClickDownload} disabled={selectedRowKeys.length > 0 ? false : true}>
              Download
            </Button>
            <Button danger icon={<BrushCleaning size={14} />} iconPosition="end" onClick={()=> setOpenConfirmEmpty(true)} disabled={data.length > 0 ? false : true}>
              Empty List
            </Button>
          </>
        )}
        width={'100%'}
        zIndex={10}
        styles={{
          content: {
            padding: 0
          },
          footer: {
            padding: 24
          }
        }}
        style={{ top: 20 }}
      >
        <Divider style={{marginBottom: 0}}/>
        <div style={{padding: 24, paddingBottom: 10}}>
          <TableComplex setSelectedRowKeys={setSelectedRowKeys} selectedRowKeys={selectedRowKeys} data={data} setData={setData} pageSize={pageLength} page={page} setPage={setPage} totalRow={totalRow} loading={loading} rerenderData={getDocument} open={openSendEmail} setOpen={setOpenSendEmail}/>
        </div>
        <Divider style={{marginTop:0, marginBottom: 0}}/>
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
      </Modal>
    </>
  );
};

export default ModalItemList;