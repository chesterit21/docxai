import { useState } from 'react';
import { Button, Divider, Modal, Space, message, Upload, Typography, Row, Col } from 'antd';
import {
    CheckOutlined,
    CloseOutlined,
    CloudUploadOutlined,
    ExclamationCircleOutlined,
} from '@ant-design/icons';

import type { UploadProps } from 'antd';
import apiClient from '../../../services/apiClient';
const { Dragger } = Upload;
const { Text } = Typography;


export default function Index({open, setOpen, documentId, rerenderData}: any) {
    const [fileList, setFileList] = useState<any[]>([]);
    const [openConfirm, setOpenConfirm] = useState(false);
    const [confirmLoading, setConfirmLoading] = useState(false);

    const allowedTypes = [
        'image/jpeg', 'image/png', 'image/jpg',
        'application/pdf',
        'application/msword', // .doc
        'application/vnd.openxmlformats-officedocument.wordprocessingml.document', // .docx
        'application/vnd.ms-excel', // .xls
        'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet', // .xlsx
        'text/plain',
        'application/vnd.ms-powerpoint', // .ppt
        'application/vnd.openxmlformats-officedocument.presentationml.presentation' // .pptx
    ];

    const handleUpload = async () => {
        if (fileList.length === 0) {
            return message.warning('Please select a file');
        }
        setOpenConfirm(true);
    };

    const handleOkAdd = async () => {
        try {
            if (fileList.length === 0) {
                return message.warning('Please select a file');
            }
            const formData = new FormData();
            formData.append('DocumentID', documentId);
            // formData.append('IsMainDocumentFile', 1);
            formData.append('fileUpload', fileList[0]);
            
            setConfirmLoading(true);
            await apiClient.post(`/documents/upload-new-version`, formData,
            {
                headers: {
                'Content-Type': 'multipart/form-data',
                },
            })
            message.success('Upload new version successful');
            setFileList([]);
            setOpenConfirm(false);
            setOpen(false);
            rerenderData();
        } catch (err) {
            message.error('Upload is failed');
        } finally {
            setConfirmLoading(false);
            setOpenConfirm(false);
        }
    };

    const props: UploadProps = {
        multiple: false,
        beforeUpload: (file) => {
        if (!allowedTypes.includes(file.type)) {
            message.error('File extension not allowed!');
            return Upload.LIST_IGNORE;
        }

        const isLt2MB = file.size / 1024 / 1024 < 5;
        if (!isLt2MB) {
            message.error('File size cannot be more than 2 MB!');
            return Upload.LIST_IGNORE;
        }

        setFileList([file]);
        return false;
        },
        onRemove: () => {
        setFileList([]);
        },
        fileList,
    };

    return (
        <>
            <Modal
            title={(
            <div className="space-align-block" style={{paddingLeft: 24, paddingRight: 24, paddingTop: 24}}>
                <Space align="center" size="middle">
                <CloudUploadOutlined style={{border: '1px solid #d9d9d9', padding: 7, fontSize: 30, color: '#434343', borderRadius: 7}}/>
                <Space.Compact direction="vertical">
                    <span>Upload Document</span>
                    <Text type="secondary" style={{fontWeight: 'normal'}}>Upload Document with input field below.</Text>
                </Space.Compact>
                </Space>
            </div>
            )}
            open={open}
            onCancel={()=> setOpen(false)}
            maskClosable={false}
            footer={false}
            width={600}
            styles={{
            content: {
                padding: 0
            },
            }}
            >
                <Divider style={{marginBottom: 0}}/>
                <div style={{padding: 24, paddingBottom: 10}}>
                    <Dragger {...props}>
                        <p className="">
                        <CloudUploadOutlined style={{fontSize: 40}}/>
                        </p>
                        <p className="ant-upload-text"><span className='font-bold text-blue-700'>Click to upload</span> or drag and drop</p>
                        <p className="ant-upload-hint">
                        jpg, jpeg, png, pdf, doc, docx, xls, xlsx, txt, ppt, pptx (max. 5MB)
                        </p>
                    </Dragger>
                </div>
                <Divider style={{marginTop:0, marginBottom: 0}}/>
                <div style={{padding: 20, paddingBottom: 20, justifySelf: 'end'}}>
                    <Space>
                        <Button icon={<CloseOutlined />} iconPosition="end" onClick={()=> setOpen(false)} color="danger" variant="filled">
                        Cancel
                        </Button>
                        <Button type="primary" icon={<CloudUploadOutlined />} iconPosition="end" onClick={handleUpload} disabled={fileList.length === 0}>
                        Upload
                        </Button>
                    </Space>
                </div>
            </Modal>
            {/* modal confirm add */}
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
                    <p className="font-bold text-lg" style={{marginBottom: 0}}>Are you sure you want to add a new file version to document?</p>
                    <span className="text-gray-500">The file you uploaded will be saved as a new version of this document.</span>
                </div>
            </Modal>
        </>
        
    )
}