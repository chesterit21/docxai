'use client'
import { Button, Checkbox, Col, Divider, Input, message, Modal, Row, Space, Spin, Typography } from "antd";
import {
    CheckOutlined,
    CloseOutlined,
    ExclamationCircleOutlined,
    FileTextOutlined,
    LoadingOutlined,
    SaveOutlined,
} from '@ant-design/icons';
import { useEffect, useState } from "react";
import apiClient from "../../../services/apiClient";
import { useNavigate } from "react-router-dom";
const { Text } = Typography;
const { TextArea } = Input;

export default function Index({open, setOpen, file}: any) {
    const navigate = useNavigate();
    const [content, setContent] = useState("");
    const [loading, setLoading] = useState(false);
    const [openConfirm, setOpenConfirm] = useState(false);
    const [confirmLoading, setConfirmLoading] = useState(false);
    const [saveAsNew, setSaveAsNew] = useState<boolean>(false)

    const fetchTxtFile = async () => {
        setLoading(true);
        try {
            const res = await fetch(import.meta.env.VITE_API_URL + file.documentFilePath);
            const text = await res.text();
            setContent(text);
        } catch (err) {
            console.log(err)
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        if (open) {
            fetchTxtFile();
        } else {
            setContent("");
            setSaveAsNew(false)
        }
    }, [open]);

    const handleSave = () => {
        if(content.length == 0) return message.error('Can not empty text file!')
        setOpenConfirm(true)
    };
    
    const handleOkAdd = async () => {
        try {
            const newFile = new File([content], file.documentFileName + file.documentType, { type: "text/plain" });
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
            message.error('Edit dile is failed');
        } finally {
            setConfirmLoading(false);
            setOpenConfirm(false);
        }
    };

    return (
        <>
            <Modal
            title={(
            <div className="space-align-block" style={{paddingLeft: 24, paddingRight: 24, paddingTop: 24}}>
                <Space align="center" size="middle">
                    <FileTextOutlined style={{fontSize: 40, color: '#595959'}} />
                    <Space.Compact direction="vertical">
                        <span>{file?.documentFileName || '-'}</span>
                        <Text type="secondary" style={{fontWeight: 'normal'}}>Show Document</Text>
                    </Space.Compact>
                </Space>
            </div>
            )}
            open={open}
            onCancel={()=> [setOpen(false), setContent('')]}
            maskClosable={false}
            footer={false}
            width={1200}
            zIndex={2}
            style={{top: 20}}
            destroyOnHidden
            styles={{
                content: {
                    padding: 0
                },
            }}
            >
                <Divider style={{marginBottom: 0}}/>
                <div className='flex justify-center' style={{padding: 24, paddingBottom: 10}}>
                    {
                        loading ? <Spin indicator={<LoadingOutlined style={{ fontSize: 48 }} spin />} /> :
                        <TextArea
                            rows={20}
                            value={content}
                            onChange={(e) => setContent(e.target.value)}
                        />
                    }
                </div>
                <Divider style={{marginTop:0, marginBottom: 0}}/>
                <div style={{padding: 20, paddingBottom: 20, justifySelf: 'end'}}>
                    <Space>
                        <Checkbox onChange={(e)=> setSaveAsNew(e.target.checked)} checked={saveAsNew}>Save as new version?</Checkbox>
                        <Button icon={<CloseOutlined />} iconPosition="end" onClick={()=> setOpen(false)} color="danger" variant="filled">
                        Cancel
                        </Button>
                        <Button type="primary" icon={<SaveOutlined />} iconPosition="end" onClick={handleSave}>
                            Save
                        </Button>
                    </Space>
                </div>
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
        </>
    )
}