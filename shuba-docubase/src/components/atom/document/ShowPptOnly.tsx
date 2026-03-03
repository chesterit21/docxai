'use client'
import { Button, Divider, Modal, Space, Typography } from "antd";
import {
    CloseOutlined,
    FilePptOutlined,
} from '@ant-design/icons';
const { Text } = Typography;
import ImgUnderConstruction from '../../../assets/under-construction.png'

export default function Index({open, setOpen, file}: any) {

    return (
        <>
            <Modal
            title={(
            <div className="space-align-block" style={{paddingLeft: 24, paddingRight: 24, paddingTop: 24}}>
                <Space align="center" size="middle">
                    <FilePptOutlined style={{fontSize: 40, color: '#ff7a45'}} />
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
                <div className='flex-col p-[24px] pb-[10px]'>
                    <p className="justify-self-center pb-10 text-2xl text-slate-500">Sorry, it does not support displaying presentation files yet</p>
                    <img className="w-xl justify-self-center" src={ImgUnderConstruction} />
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