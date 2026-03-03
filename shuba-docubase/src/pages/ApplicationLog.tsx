import { Col, Flex, Row, Space } from 'antd';
import Breadcrumb  from '../components/atom/Breadcrumb';
import Content from '../components/molecule/applicationLog'
import { useAuth } from '../context/AuthContext';
import Page404 from '../pages/NotFoundGuest';

const Shared = () => {
    const { user } = useAuth();
    
    return (
        user?.user_type === 'superadmin' ?
        <>
            <Breadcrumb item={[{title: <span style={{fontWeight: 'bold'}}>Application Log</span>}]}/>
            <Row style={{marginBottom: 20}}>
                <Col span={24}>
                    <Flex justify='space-between'>
                        <span className='text-2xl font-bold'>Application Log</span>
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
        : <Page404 />       
    )
}
export default Shared;