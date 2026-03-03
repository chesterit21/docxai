'use client'
import { useRef } from 'react';
import { Button, Divider, Modal, Space, Typography } from "antd";
import {
    CloseOutlined,
    FilePdfOutlined,
} from '@ant-design/icons';
import { PdfViewerComponent, Toolbar, Magnification, Navigation, LinkAnnotation, BookmarkView, ThumbnailView, Print, TextSelection, Annotation, TextSearch, FormFields, FormDesigner, PageOrganizer, Inject } from '@syncfusion/ej2-react-pdfviewer';
import { registerLicense } from '@syncfusion/ej2-base';
registerLicense('Ngo9BigBOggjHTQxAR8/V1NNaF5cXmBCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWXtdcHVVQ2FZWUBxXkFWYUA=');

const { Text } = Typography;

export default function Index({open, setOpen, file}: any) {
    let refPdf = useRef<PdfViewerComponent>(null);

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
            style={{top: 20}}
            styles={{
                content: {
                    padding: 0
                },
            }}
            destroyOnHidden
            // afterOpenChange={handleAfterOpenChange}
            >
                <Divider style={{marginBottom: 0}}/>
                <div className='control-section' style={{padding: 24, paddingBottom: 10}}>
                    <PdfViewerComponent
                    ref={refPdf}
                    id="containerPdf" 
                    // documentPath= "https://pdfobject.com/pdf/sample.pdf"
                    documentPath= {import.meta.env.VITE_API_URL + file.documentFilePath}
                    resourceUrl = {window.location.origin + "/ej2-pdfviewer-lib"}
                    enableToolbar={false} 
                    style={{ height: '43rem' }}
                    >
                        <Inject services={[Toolbar, Magnification, Navigation, Annotation, LinkAnnotation, BookmarkView, ThumbnailView, Print, TextSelection, TextSearch, FormFields, FormDesigner, PageOrganizer]}/>
                    </PdfViewerComponent>
                </div>
                <Divider style={{marginTop:0, marginBottom: 0}}/>
                <div style={{padding: 20, paddingBottom: 20, justifySelf: 'end'}}>
                    <Space>
                        <Button icon={<CloseOutlined />} iconPosition="end" onClick={()=> setOpen(false)} color="danger" variant="filled">
                        Cancel
                        </Button>
                    </Space>
                </div>
            </Modal>
        </>
    )
}