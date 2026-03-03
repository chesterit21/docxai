import { Result, Button, Modal } from 'antd';
import { SmileOutlined } from '@ant-design/icons';
import { useEffect, useRef, useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';

const Index = () => {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const xd = searchParams.get('xd');
  const [timeLeft, setTimeLeft] = useState(5);
  const [modalVisible, setModalVisible] = useState(false);
  const timerRef = useRef<NodeJS.Timeout | null>(null);

  useEffect(() => {
    if (modalVisible && timeLeft > 0) {
      timerRef.current = setTimeout(() => {
        setTimeLeft((prev) => prev - 1);
      }, 1000);
    } else if (timeLeft === 0) {
      Modal.destroyAll();
      clearTimeout(timerRef.current!);
    }

    return () => clearTimeout(timerRef.current!);
  }, [modalVisible, timeLeft]);

  const handleClick = async () => {
    console.log(xd)
    const response = await fetch(`${import.meta.env.VITE_API_URL}/verification/verify?xd=${xd}`, {
        method: 'PUT',
      });

    if (response.ok) {  
      const data = await response.json();
      
      console.log(data)
      setTimeLeft(5);
      setModalVisible(true);
      showSuccessModal(5);
      setTimeout(() => {
        Modal.destroyAll();
        navigate('/login');
      }, 5000);
      
    } else {
      error()
    }
  };

  const showSuccessModal = (initialTime: number) => {
    Modal.success({
      title: 'Your account has been activated',
      content: (
        <div className='mt-5'>
          <p className='text-xl'>
            You will be redirected to the login page in <b>{initialTime}</b>
          </p>
        </div>
      ),
      footer: null,
      width: 600,
    });
  };

  const error = () => {
    Modal.error({
      title: 'Something went wrong',
      content: (
        <div>
          <p className='text-xl'>Please contact your Administrator</p>
        </div>
      ),
      footer: null,
      width: 600
    });
  };

  useEffect(() => {
    if (modalVisible) {
      const modals = document.getElementsByClassName('ant-modal-body');
      if (modals.length > 0) {
        modals[0].querySelector('p')!.innerHTML = `You will be redirected to the login page in <b>${timeLeft}</b>`;
      }
    }
  }, [timeLeft]);

  return (
    <Result
      icon={<SmileOutlined />}
      title="Welcome to Docubase Document Management System"
      subTitle="Please verify your account & login to the application."
      extra={<Button type="primary" onClick={handleClick}>Verification Now</Button>}
    />
  );
};

export default Index;
