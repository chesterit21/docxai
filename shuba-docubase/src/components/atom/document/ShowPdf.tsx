'use client'
import { useEffect, useMemo, useRef } from 'react';
import { useState } from "react";
import { Button, Checkbox, Col, Divider, Form, message, Modal, Row, Space, Typography } from "antd";
import {
    CheckOutlined,
    CloseOutlined,
    ExclamationCircleOutlined,
    FilePdfOutlined,
    SaveOutlined,
} from '@ant-design/icons';
import { PdfViewerComponent, Toolbar, Magnification, Navigation, LinkAnnotation, BookmarkView, ThumbnailView, Print, TextSelection, Annotation, TextSearch, FormFields, FormDesigner, PageOrganizer, Inject } from '@syncfusion/ej2-react-pdfviewer';
import { registerLicense } from '@syncfusion/ej2-base';
import apiClient from '../../../services/apiClient';
import { useNavigate } from 'react-router-dom';
registerLicense('Ngo9BigBOggjHTQxAR8/V1NNaF5cXmBCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWXtdcHVVQ2FZWUBxXkFWYUA=');

const { Text } = Typography;

export default function Index({open, setOpen, file}: any) {
    const navigate = useNavigate();
    const [openConfirm, setOpenConfirm] = useState(false);
    const [confirmLoading, setConfirmLoading] = useState(false);
    let refPdf = useRef<PdfViewerComponent>(null);
    const [saveAsNew, setSaveAsNew] = useState<boolean>(false)

    const viewerId = useMemo(() => `pdf-viewer-${file?.id || Date.now()}`, [file?.id]);
    
    useEffect(() => {
        if (!open) {
            setSaveAsNew(false)
        }
    }, [open]);

    const handleSubmit = () => {
        setOpenConfirm(true);
    }
    
    const handleOkAdd = async () => {
        if (!refPdf.current) return;
        try {
            const blob = await refPdf.current.saveAsBlob();
            const newFile = new File([blob], file.documentFileName + file.documentType, { type: "application/pdf" });
            const formData = new FormData();
            setConfirmLoading(true);
            if (saveAsNew) {
                formData.append('DocumentID', file.documentID);
                formData.append('fileUpload', newFile);
                await apiClient.post(`/documents/upload-new-version`, formData,
                {
                    headers: {
                    'Content-Type': 'multipart/form-data',
                    },
                })
                
            } else {
                formData.append("file", newFile);
                await apiClient.put(`/documentfiles?Id=${file.id}&DocumentID=${file.documentID}`, formData,
                {
                    headers: {
                    'Content-Type': 'multipart/form-data',
                    },
                })
            }
            message.success('Edit file is successful');
            setConfirmLoading(false);
            setOpenConfirm(false);
            setOpen(false)
            navigate(0)
        } catch (err) {
            console.error(err);
            message.error('Edit file is failed');
            setConfirmLoading(false);
            setOpenConfirm(false);
        }
    };

  const handleAfterOpenChange = (visible: boolean) => {
    if (!visible) {
      try {
        (refPdf.current as any).viewer.destroy();
      } catch (e) {
        console.warn('Viewer destroy failed:', e);
      }
    }
  };

    return (
        <>
            <Modal
            title={(
            <div className="space-align-block" style={{paddingLeft: 24, paddingRight: 24, paddingTop: 24}}>
                <Space align="center" size="middle">
                    <FilePdfOutlined style={{fontSize: 40, color: '#f5222d'}} />
                    <Space.Compact direction="vertical">
                        <span>{file?.documentFileName || '-'}</span>
                        <Text type="secondary" style={{fontWeight: 'normal'}}>Show Document</Text>
                    </Space.Compact>
                </Space>
            </div>
            )}
            open={open}
            onCancel={()=> setOpen(false)}
            maskClosable={false}
            footer={false}
            width={1200}
            zIndex={2}
            style={{top: 20}}
            styles={{
                content: {
                    padding: 0
                },
            }}
            destroyOnHidden
            afterOpenChange={handleAfterOpenChange}
            >
                <Divider style={{marginBottom: 0}}/>
                <Form
                layout='vertical'
                onFinish={handleSubmit}
                >
                    <div className='control-section' style={{padding: 24, paddingBottom: 10}}>
                        <PdfViewerComponent
                        ref={refPdf}
                        id={viewerId}
                        // documentPath= "https://pdfobject.com/pdf/sample.pdf"
                        documentPath= {import.meta.env.VITE_API_URL + file.documentFilePath}
                        resourceUrl = {window.location.origin + "/ej2-pdfviewer-lib"} 
                        style={{ height: '43rem' }}
                        >
                            <Inject services={[Toolbar, Magnification, Navigation, Annotation, LinkAnnotation, BookmarkView, ThumbnailView, Print, TextSelection, TextSearch, FormFields, FormDesigner, PageOrganizer]}/>
                        </PdfViewerComponent>
                    </div>
                    <Divider style={{marginTop:0, marginBottom: 0}}/>
                    <div style={{padding: 20, paddingBottom: 20, justifySelf: 'end'}}>
                        <Space>
                            <Checkbox onChange={(e)=> setSaveAsNew(e.target.checked)} checked={saveAsNew}>Save as new version?</Checkbox>
                            <Button icon={<CloseOutlined />} iconPosition="end" onClick={()=> setOpen(false)} color="danger" variant="filled">
                            Cancel
                            </Button>
                            <Button type="primary" icon={<SaveOutlined />} iconPosition="end" htmlType='submit'>
                            Save
                            </Button>
                        </Space>
                    </div>
                </Form>
            </Modal>
            {/* modal confirm edit */}
            <Modal
                title={(
                    <div style={{paddingLeft: 24, paddingRight: 24, paddingTop: 24}}>
                        <ExclamationCircleOutlined style={{fontSize: 30, color: 'gold'}}/>
                    </div>
                )}
                open={openConfirm}
                onCancel={()=> setOpenConfirm(false)}
                maskClosable={false}
                footer={(_,) => (
                <>
                    <Row gutter={12}>
                        <Col span={12}>
                            <Button icon={<CloseOutlined />} iconPosition="end" onClick={()=> setOpenConfirm(false)} variant="filled" block>
                            Cancel
                            </Button>
                        </Col>
                        <Col span={12}>
                            <Button type="primary" icon={<CheckOutlined />} iconPosition="end" onClick={handleOkAdd} block loading={confirmLoading}>
                            Confirm
                            </Button>
                        </Col>
                    </Row>
                </>
                )}
                width={400}
                zIndex={5}
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
                    <p className="font-bold text-lg" style={{marginBottom: 0}}>Do you want to save this document?</p>
                    <span className="text-gray-500">The file you uploaded will be saved as part of your document(s).</span>
                </div>
            </Modal>
        </>
    )
}