import Logo from '../../../assets/logo.png'

import {
  Space,
  Layout
} from 'antd';
import MenuItem from './Menu';

const { Sider } = Layout;
export default function sideComponent({ collapsed, setCollapsed }: any) {
  const siderStyle: React.CSSProperties = {
    overflow: 'auto',
    height: '100vh',
    position: 'sticky',
    insetInlineStart: 0,
    top: 0,
    bottom: 0,
    scrollbarWidth: 'thin',
    scrollbarGutter: 'stable',
    backgroundColor: '#f5f5f5'
  };
  return (
    <Sider
      collapsed={collapsed}
      onCollapse={setCollapsed}
      breakpoint="sm"
      style={siderStyle}
      width={250}
    >
      <Space size="small" align='center' style={{ height: 64, width: '100%', justifyContent: 'center' }}>
        <img src={Logo} alt="" className="block h-9" />
        {/* {
                  !collapsed &&
                  <div
                      style={{
                      color: '#09090B',
                      fontWeight: 'bold',
                      fontSize: 20
                      }}
                  > 
                      Docubase
                  </div>
              }  */}
      </Space>
      <MenuItem collapsed={collapsed} />
    </Sider>
  )
}
