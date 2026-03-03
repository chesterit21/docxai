import { Result, Button } from 'antd';
import { Link } from 'react-router-dom';

const NotFoundAuth = () => {
  return (
    <Result
      status="404"
      title="404"
      subTitle="Halaman tidak ditemukan."
      extra={<Link to="/"><Button type="primary">Back to Home</Button></Link>}
    />
  );
};

export default NotFoundAuth;
