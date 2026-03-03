import { useEffect, useRef, useState } from "react";
import { Button, Checkbox, Col, Divider, Form, message, Modal, Row, Space, Typography } from "antd";
import {
    CheckOutlined,
    CloseOutlined,
    ExclamationCircleOutlined,
    FileWordFilled,
    SaveOutlined,
} from '@ant-design/icons';

import {
    DocumentEditorContainerComponent, Toolbar
} from '@syncfusion/ej2-react-documenteditor';
DocumentEditorContainerComponent.Inject(Toolbar);
import { registerLicense } from '@syncfusion/ej2-base';
import apiClient from "../../../services/apiClient";
import { useNavigate } from "react-router-dom";
registerLicense('Ngo9BigBOggjHTQxAR8/V1NNaF5cXmBCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWXtdcHVVQ2FZWUBxXkFWYUA=');


const { Text } = Typography;

export default function Index({open, setOpen, file}: any) {
    const navigate = useNavigate();
    const [openConfirm, setOpenConfirm] = useState(false);
    const [confirmLoading, setConfirmLoading] = useState(false);
    const [showEditor, setShowEditor] = useState(false);
    const editorRef = useRef<DocumentEditorContainerComponent>(null);
    const fileUrl = import.meta.env.VITE_API_URL + file.documentFilePath;
    const serviceUrl = import.meta.env.VITE_API_URL + '/documenteditor/';
    const [saveAsNew, setSaveAsNew] = useState<boolean>(false)
    
    useEffect(() => {
     if (!open) {
      setShowEditor(false);
      setSaveAsNew(false)
    }
    }, [open]);

    const handleSubmit = () => {
        setOpenConfirm(true);
    }
    
    const handleOkAdd = async () => {
        if (!editorRef.current) return;
        try {
            const blob = await editorRef.current.documentEditor.saveAsBlob('Docx');
            const newFile = new File([blob], file.documentFileName + file.documentType, {
                type: "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            });
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
            zIndex={2}
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
                <Form
                layout='vertical'
                onFinish={handleSubmit}
                >
                    <Divider style={{marginBottom: 0}}/>
                    {showEditor && (
                        <DocumentEditorContainerComponent
                        id="document-editor"
                        ref={editorRef}
                        height={'730px'}
                        enableToolbar={true}
                        //serviceUrl="https://ej2services.syncfusion.com/production/web-services/api/documenteditor/"
                        serviceUrl={serviceUrl}
                        />
                    )}
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
                zIndex={5}
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
        </div>
    )
}