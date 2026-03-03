import type { TabsProps } from 'antd';
import { Tabs } from 'antd';
import { Col, Flex, Row, Space } from 'antd';
import Breadcrumb  from '../../atom/Breadcrumb';
import TableListUser from './TableUser';
import TableListGroup from './TableGroup';
import TablelCollection from './TableCollection';
import TablelAttribute from './TableAttribute';
import TableCategory from './Category'
import TableDocument from './Document'
import TableFile from './File'
import { useAuth } from '../../../context/AuthContext';

export default function Index () {
  const { user } = useAuth();
  
  const items: TabsProps['items'] = [
    {
      label: 'Category',
      key: '1',
      children: <TableCategory/>,
    },
    {
      label: 'Document',
      key: '2',
      children: <TableDocument/>,
    },
    {
      label: 'File',
      key: '3',
      children: <TableFile/>,
    },
    ...(user?.user_type === 'superadmin' ? [
      {
        label: 'Collection',
        key: '4',
        children: <TablelCollection />,
      },
      {
        label: 'Attribute',
        key: '5',
        children: <TablelAttribute />,
      },
      {
        label: 'User',
        key: '6',
        children: <TableListUser/>,
      },
      {
        label: 'Group',
        key: '7',
        children: <TableListGroup />,
      }
    ] : []),
  ];
  
  
    return (
      <>
          <Breadcrumb item={[{title: <span style={{fontWeight: 'bold'}}>Recycle Bin</span>}]}/>
          <Row style={{marginBottom: 20}}>
              <Col span={24}>
                  <Flex justify='space-between'>
                      <span className='text-2xl font-bold'>Recycle Bin</span>
                      <Space size="large">
                      </Space>
                  </Flex>
              </Col>
          </Row>
          <Tabs
          defaultActiveKey="1"
          type="card"
          size="middle"
          style={{ marginBottom: 32 }}
          items={items}
          />
      </>
    )
}