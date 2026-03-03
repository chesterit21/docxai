import { useEffect, useState } from 'react';
import { Avatar, List, Button, Popover, Tabs } from 'antd';
import type { TabsProps } from 'antd';
import { Link } from 'react-router-dom';
import './style.css'
import {
    BellOutlined,
    UserOutlined
  } from '@ant-design/icons';

import dayjs from 'dayjs';
import customParseFormat from 'dayjs/plugin/customParseFormat';
import relativeTime  from 'dayjs/plugin/relativeTime';
import apiClient from '../../../services/apiClient';
dayjs.extend(customParseFormat);
dayjs.extend(relativeTime)
const now = dayjs().format('YYYY-MM-DD')
const intervalDay = dayjs().add(-7, 'day').format('YYYY-MM-DD')

export default function index() {
    const [open, setOpen] = useState(false);
    const [dataNotif, setDataNotif] = useState<any[]>([])
    const [dataRemind, setDataRemind] = useState<any[]>([])
    const page = 1
    const pageLength = 10
    const [loadingNotif, setLoadingNotif] = useState(false);
    const [loadingReminder, setLoadingReminder] = useState(false);
    const [isNotif, setIsNotif] = useState(false);
    const [isReminder, setIsReminder] = useState(false);

    const items: TabsProps['items'] = [
        {
          label: 'Notifications',
          key: '1',
          children: <PopListNotification data={dataNotif} loading={loadingNotif}/>,
        },
        {
          label: 'Reminders',
          key: '2',
          children: <PopListReminders data={dataRemind} loading={loadingReminder}/>,
        },
    ];

    const handleOpenChange = (newOpen: boolean) => {
        setOpen(newOpen);
    };

        let queryParams = new URLSearchParams({
        DocumentTitle: 'dok',
        DtFrom: intervalDay,
        DtTo: now,
    });
    
    useEffect(()=> {
        getNotification()
        getReminder()
    }, [])

    const getNotification = async () => {
        setLoadingNotif(true)
        apiClient.get(`/notification/search?${queryParams}&Page=${page}&Limit=${pageLength}`)
        .then(({ data }) => {
            setDataNotif(data?.data?.data || [])
            if(data?.data?.data.length > 0) setIsNotif(true)
            setLoadingNotif(false)
        })
        .catch(err => {
            console.log(err)
            setLoadingNotif(false)
            setDataNotif([])
        })
    }

    const getReminder = async () => {
        setLoadingReminder(true)
        apiClient.get(`/documentreminder/get-top-document-reminders?Page=${page}&Limit=10000`)
        .then(({ data }) => {
            setDataRemind(data?.data?.data || [])
            if(data?.data?.data.length > 0) setIsReminder(true)
            setLoadingReminder(false)
        })
        .catch(err => {
            console.log(err)
            setLoadingReminder(false)
            setDataRemind([])
        })
    }



    return (
            <Popover
            content={
                <div className='w-md'>
                    <Tabs
                    defaultActiveKey="1"
                    type="card"
                    size="middle"
                    items={items}
                    />
                </div>
            }
            trigger="click"
            open={open}
            onOpenChange={handleOpenChange}
            placement="bottomRight"
            arrow={false}
            >
                <Button size='large' icon={<BellOutlined className={`${ isNotif || isReminder ? '!text-green-400' : ''}`} style={{ fontSize: 20}} />} />
            </Popover>

    )
    
}

export function PopListNotification({data, loading}: any) {


    const ListNotification = ({data}: any) => {
        return (
            <div style={{minWidth: 310}}>
                <List
                    itemLayout="horizontal"
                    loading={loading}
                    dataSource={data}
                    footer={<div style={{justifySelf: 'center'}}>Want to see all notification? <Link to="/notification" style={{fontSize: 15}}>See More</Link></div>}
                    renderItem={(item: any) => {
                        const insertedDate = dayjs.utc(item.insertedAt);
                        return (
                            <div className="">
                                <List.Item
                                extra={
                                    <>
                                        {
                                            insertedDate.toNow()
                                        }
                                    </>
                                }
                                style={{paddingRight: 10, paddingLeft: 10}}
                                >
                                    <List.Item.Meta
                                    avatar={<Avatar size="large" icon={<UserOutlined />} />}
                                    title={
                                        <div>
                                            {item.actorFullname}
                                            <div>{item?.notifDescription}</div>
                                        </div>
                                    }
                                    description={
                                        <>
                                            <div className='text-black'>{item?.documentTitle}</div>
                                            {item?.documentFileSize}
                                        </>
                                    }
                                    />
                                </List.Item>
                            </div>
                        )
                    }}
                />
            </div>
        )
    }

    return (
           <ListNotification data={data}/>
    )
    
}

export function PopListReminders({data, loading}: any) {

    const ListNotification = ({data}: any) => {
        return (
            <div style={{minWidth: 310}}>
                <List
                    itemLayout="horizontal"
                    dataSource={data}
                    loading={loading}
                    footer={<div style={{justifySelf: 'center'}}>Want to see all reminder? <Link to="/reminder" style={{fontSize: 15}}>See More</Link></div>}
                    renderItem={(item: any) => {
                        const insertedDate = dayjs.utc(item.reminderDateTime);
                        return (
                            <div className="">
                                <List.Item
                                extra={
                                    <>
                                        {
                                            item?.reminderDateTime && insertedDate.fromNow()
                                        }
                                    </>
                                }
                                style={{paddingRight: 10, paddingLeft: 10}}
                                >
                                    <List.Item.Meta
                                    title={
                                        <div>
                                            {item.reminderDesc}
                                        </div>
                                    }
                                    // description={
                                    //     <>
                                    //         <div className='text-black'>{item?.reminderDesc}</div>
                                    //     </>
                                    // }
                                    />
                                </List.Item>
                            </div>
                        )
                    }}
                />
            </div>
        )
    }

    return (
           <ListNotification data={data}/>
    )
    
}