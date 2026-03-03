import type { TabsProps } from 'antd';
import { Tabs } from 'antd';
import TableListMyShared from './tableListMyShared';
import TableListSharedToMe from './tableListSharedtoMe';


export default function Index () {
  const items: TabsProps['items'] = [
    {
      label: 'My Shared Files',
      key: '1',
      children: <TableListMyShared/>,
    },
    {
      label: 'Shared To Me',
      key: '2',
      children: <TableListSharedToMe/>,
    },
  ];
  
  
    return (
        <Tabs
        defaultActiveKey="1"
        type="card"
        size="middle"
        style={{ marginBottom: 32 }}
        items={items}
        />
    )
}