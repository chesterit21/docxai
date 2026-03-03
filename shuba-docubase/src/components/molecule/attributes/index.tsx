import type { TabsProps } from 'antd';
import { Tabs } from 'antd';
import TableLisCollection from './TableCollection';
import TableListAttribute from './TableAttribute';
import { Col, Flex, Row, Space } from 'antd';
import Breadcrumb  from '../../atom/Breadcrumb';

export default function Index () {
  const items: TabsProps['items'] = [
    {
      label: 'Collection',
      key: '1',
      children: <TableLisCollection/>,
    },
    {
      label: 'Attribute',
      key: '2',
      children: <TableListAttribute />,
    },
  ];
  
  
    return (
      <>
          <Breadcrumb item={[{title: <span style={{fontWeight: 'bold'}}>Attributes</span>}]}/>
          <Row style={{marginBottom: 20}}>
              <Col span={24}>
                  <Flex justify='space-between'>
                      <span className='text-2xl font-bold'>Collection & Attributes</span>
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