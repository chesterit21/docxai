import { Col, Flex, Row, Space } from 'antd';
import Breadcrumb  from '../components/atom/Breadcrumb';
import Content from '../components/molecule/shared'

const Shared = () => {
    return (
        <>
            <Breadcrumb item={[{title: <span style={{fontWeight: 'bold'}}>Shared WorkSpace</span>}]}/>
            <Row style={{marginBottom: 20}}>
                <Col span={24}>
                    <Flex justify='space-between'>
                        <span className='text-2xl font-bold'>Shared WorkSpace</span>
                        <Space size="large">
                        </Space>
                    </Flex>
                </Col>
            </Row>
            <Row>
                <Col span={24}>
                    <Content />
                </Col>
            </Row>
        </>        
    )
}
export default Shared;