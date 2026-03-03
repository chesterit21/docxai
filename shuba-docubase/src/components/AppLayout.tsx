  import {
    Layout,
    theme
  } from 'antd';
  import { Outlet, useLocation } from 'react-router-dom';
  import HeaderComponent from './molecule/layouts/Header';
  import SideComponent from './molecule/layouts/Side';
  import FooterCommponent from './molecule/layouts/footer';
import { useState } from 'react';
  
  const { Content } = Layout;
  
  
  const AppLayout = () => {
    const [collapsed, setCollapsed] = useState(false);
    const {
      token: { colorBgContainer },
    } = theme.useToken();

    const location = useLocation();
    const currentPath = location.pathname;
    console.log(currentPath)

    return (
      <Layout style={{ minHeight: '100vh' }}>
        <SideComponent collapsed={collapsed} setCollapsed={setCollapsed}/>
        <Layout>
          {/* header */}
          <HeaderComponent collapsed={collapsed} setCollapsed={setCollapsed}/>
          {/* content */}
          <Content style={{ margin: '16px' }}>
            <div
              style={{
                padding: currentPath !== '/attributes/collection-add' && !currentPath.includes('/attributes/collection-edit') && !currentPath.includes('/document/document-') ? 24 : 0,
                minHeight: 360,
                background: currentPath !== '/dashboard' && currentPath !== '/attributes/collection-add' && !currentPath.includes('/attributes/collection-edit') && !currentPath.includes('/document/document-') ? colorBgContainer : '',
                borderRadius: currentPath !== '/attributes/collection-add' && !currentPath.includes('/attributes/collection-edit') && !currentPath.includes('/document/document-') ? 16 : 0,
              }}
            >
              <Outlet />
            </div>
          </Content>
          {/* footer */}
          <FooterCommponent/>
        </Layout>
      </Layout>
    );
  };
  
  export default AppLayout;
  
