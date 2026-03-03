import Background from '../../assets/auth-bg.jpg'
// import Image from '../../assets/img-auth.png'
import Logo from '../../assets/logo.png'

import { Button, Card, Flex, Form, Input, message, Space } from 'antd';
import { ArrowLeftOutlined, MailOutlined } from '@ant-design/icons';
import { Link } from 'react-router-dom';
import apiClient from '../../services/apiClient';
import { useState } from 'react';

const validateMessages = {
  required: '${label} is required!',
  type: {
    email: 'The input is not valid E-mail!',
    number: '${label} is not a valid number!',
  },
  number: {
    range: '${label} must be between ${min} and ${max}',
  },
};

const LoginPage: React.FC = () => {
  const [loading, setLoading] = useState<boolean>(false);


  const onFinish = async (values: any) => {
    console.log('Login values:', values);
    setLoading(true)
    await apiClient.post(`/user/request-change-password`, values)
        .then(({ data }) => {
            console.log(data)
            setLoading(false)
            message.success('Success, please check your email to get verification code')
            // navigate('/code-verification');
        })
        .catch(err => {
            console.log(err)
            message.error(err.response.data.message)
            setLoading(false)
        })

  };

  return (
    <div className="w-screen h-screen flex">
      <div className="w-full md:w-1/2 flex items-center justify-center bg-white p-8">
        <div className="max-w-md w-full">
            <Card
            style={{ border: 'none'}}
            >
                <Space style={{marginBottom: 50}}>
                    <img src={Logo} alt="" className="block h-13" />
                </Space>
                <div className='mb-8 flex flex-col gap-1'>
                    <span className='text-2xl font-bold'>Forgot Password?</span>
                    <span>Just type your email, and we will send you a verification code to reset your password</span>
                </div>
                <Form
                    name="login-form"
                    initialValues={{ remember: true }}
                    onFinish={onFinish}
                    layout="vertical"
                    validateMessages={validateMessages}
                >
                    <Form.Item 
                    label="Email"
                    name="emailAddress"
                    rules={[{ type: 'email'}, {required: true}]}
                    >
                        <Input prefix={<MailOutlined />} placeholder="Email" size='large'/>
                    </Form.Item>

                    <Form.Item>
                    <Flex vertical gap='middle'>
                      <Button color="purple" variant="solid" size='large' htmlType='submit' block style={{marginTop: 20}} loading={loading}>
                          Send
                      </Button>
                      <Link to="/login">
                        <Button type='link' block icon={<ArrowLeftOutlined />} size='large' iconPosition="start" style={{border: '1px solid #d9d9d9', color:'black'}}> 
                            <span style={{alignSelf: "center"}}> Back to login</span>
                        </Button>
                      </Link>
                    </Flex>
                    </Form.Item>
                </Form>
            </Card>
        </div>
      </div>

      <div className="hidden md:flex w-1/2 relative items-center justify-end overflow-hidden">
        <img
          src={Background}
          alt="Background"
          className="absolute w-full h-full object-cover top-0 left-0 z-0"
        />

        <img
          // src={Image} 
          alt="Illustration"
          className="z-10 max-w-[100%] max-h-[90%] -mr-[100px] rounded-l-lg"
        />
      </div>
    </div>
  );
};

export default LoginPage;

