'use client'
import { Button, Divider, Image, Modal, Space, Typography } from "antd";
import {
    CloseOutlined,
    FileImageOutlined,
} from '@ant-design/icons';
const { Text } = Typography;

export default function Index({open, setOpen, file}: any) {

    return (
        <>
            <Modal
            title={(
            <div className="space-align-block" style={{paddingLeft: 24, paddingRight: 24, paddingTop: 24}}>
                <Space align="center" size="middle">
                    <FileImageOutlined style={{fontSize: 40, color: '#08979c'}} />
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
            >
                <Divider style={{marginBottom: 0}}/>
                <div className='flex justify-center' style={{padding: 24, paddingBottom: 10}}>
                    <Image
                    width="50%"
                    src={import.meta.env.VITE_API_URL + file.documentFilePath}
                    />
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