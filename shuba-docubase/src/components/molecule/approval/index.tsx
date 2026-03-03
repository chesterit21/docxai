import type { TabsProps } from 'antd';
import { Tabs } from 'antd';
import { Col, Flex, Row, Space } from 'antd';
import Breadcrumb  from '../../atom/Breadcrumb';
import TableListTask from './TableTask';
import TableListRequest from './TableRequest';

export default function Index () {
  const items: TabsProps['items'] = [
    {
      label: 'My Task',
      key: '1',
      children: <TableListTask/>,
    },
    {
      label: 'My Request',
      key: '2',
      children: <TableListRequest />,
    },
  ];
  
  
    return (
      <>
          <Breadcrumb item={[{title: <span style={{fontWeight: 'bold'}}>Approval</span>}]}/>
          <Row style={{marginBottom: 20}}>
              <Col span={24}>
                  <Flex justify='space-between'>
                      <span className='text-2xl font-bold'>Approval Status</span>
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