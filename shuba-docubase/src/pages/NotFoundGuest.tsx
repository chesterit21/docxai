import { Button, Result } from 'antd';
import { useNavigate } from 'react-router-dom';

const NotFoundGuest = () => {
  const navigate = useNavigate();
  return (
    <Result
      status="404"
      title="404"
      subTitle="Page Not Found."
      extra={<Button type="primary" onClick={()=> navigate('/')}>Back</Button>}
    />
  );
};

export default NotFoundGuest;
