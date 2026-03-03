import { useEffect, useRef, useState } from "react";
import { Button, Divider,  Modal, Space, Typography } from "antd";
import {
    CloseOutlined,
    FileWordFilled,
} from '@ant-design/icons';

import {
    DocumentEditorContainerComponent
} from '@syncfusion/ej2-react-documenteditor';
import { registerLicense } from '@syncfusion/ej2-base';
registerLicense('Ngo9BigBOggjHTQxAR8/V1NNaF5cXmBCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWXtdcHVVQ2FZWUBxXkFWYUA=');


const { Text } = Typography;

export default function Index({open, setOpen, file}: any) {
    const [showEditor, setShowEditor] = useState(false);
    const editorRef = useRef<DocumentEditorContainerComponent>(null);
    const fileUrl = import.meta.env.VITE_API_URL + file.documentFilePath;
    
    useEffect(() => {
     if (!open) {
      setShowEditor(false);
    }
    }, [open]);

    const handleAfterOpenChange = (visible: boolean) => {
        if (visible) {
            setShowEditor(true);
            setTimeout(() => {
                editorRef.current!.documentEditor.open(fileUrl);
            }, 200);
        }
    };

    return (
        <div>
            <Modal
            title={(
            <div className="space-align-block" style={{paddingLeft: 24, paddingRight: 24, paddingTop: 24}}>
                <Space align="center" size="middle">
                    <FileWordFilled style={{fontSize: 40, color: '#1677ff'}}/>
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
            afterOpenChange={handleAfterOpenChange}
            >
                <Divider style={{marginBottom: 0}}/>
                {showEditor && (
                    <DocumentEditorContainerComponent
                    id="document-editor"
                    ref={editorRef}
                    height={'730px'}
                    enableToolbar={false}
                    showPropertiesPane={false}
                    enableLockAndEdit={true}
                    serviceUrl="https://ej2services.syncfusion.com/production/web-services/api/documenteditor/"
                    />
                )}
                <Divider style={{marginTop:0, marginBottom: 0}}/>
                <div style={{padding: 20, paddingBottom: 20, justifySelf: 'end'}}>
                    <Space>
                        <Button icon={<CloseOutlined />} iconPosition="end" onClick={()=> setOpen(false)} color="danger" variant="filled">
                        Cancel
                        </Button>
                    </Space>
                </div>
            </Modal>
        </div>
    )
}