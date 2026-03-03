import { HomeOutlined } from '@ant-design/icons';
import { Breadcrumb, ConfigProvider } from 'antd';
import { Link, useLocation } from 'react-router-dom';

interface itemPops {
    item: any[]
}


function getParentRoute(pathname: string): string {
  const segments = pathname.split("/").filter(Boolean); // pisahkan dan hilangkan empty string
  return segments.length > 0 ? `/${segments[0]}` : "/";
}

const App = ({item}: itemPops) => {
  const location = useLocation();
  const currentPath = location.pathname;
  const parentRoute = getParentRoute(currentPath);
  
  return (
    <ConfigProvider
      theme={{
        components: {
          Breadcrumb: {
            lastItemColor : '#1677ff'
          },
        },
      }}
    >
      <Breadcrumb
        style={{marginBottom : 17, textTransform: 'capitalize'}}
        separator=">"
        items={[
          {
            title: <Link to={parentRoute}><HomeOutlined /></Link>,
          },
          ...item
        ]}
      />
    </ConfigProvider>
  );
}

export default App;