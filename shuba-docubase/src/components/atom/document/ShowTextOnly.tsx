'use client'
import { Button, Card, Divider, Modal, Space, Spin, Typography } from "antd";
import {
    CloseOutlined,
    FileTextOutlined,
    LoadingOutlined,
} from '@ant-design/icons';
import { useEffect, useState } from "react";
const { Text } = Typography;

export default function Index({open, setOpen, file}: any) {
    const [content, setContent] = useState("");
    const [loading, setLoading] = useState(false);
    
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
        }
    }, [open]);


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
                    <Card styles={{ body:{width: '100%', minHeight: 300, maxHeight: 410, overflowY: 'auto'}}} >
                        {
                            loading ? <Spin indicator={<LoadingOutlined style={{ fontSize: 48 }} spin />} /> :
                            <div className="font-mono whitespace-pre-wrap">
                                {content}
                            </div>
                        }
                    </Card>
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