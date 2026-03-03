import Background from '../../assets/auth-bg.jpg'
// import Image from '../../assets/img-auth.png'
import Logo from '../../assets/logo.png'

import { Button, Card, Checkbox, Divider, Form, Input, message, Modal, Result, Space } from 'antd';
import {  LockOutlined } from '@ant-design/icons';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { useEffect, useState } from 'react';
import apiClient from '../../services/apiClient';


const LoginPage: React.FC = () => {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const userId = searchParams.get('userId');
  const [form] = Form.useForm();
  const [open, setOpen] = useState(false);
  const [loading, setLoading] = useState(false);
  const [passwordRules, setPasswordRules] = useState({
      length: false,
      lowercase: false,
      uppercase: false,
      number: false,
      specialChar: false,
  });

  useEffect(() => {
      if (!open) {
          form.resetFields();
          setPasswordRules({
              length: false,
              lowercase: false,
              uppercase: false,
              number: false,
              specialChar: false,
          });
      }
  }, [open]);

    const handleSubmit = () => {
        setLoading(true);
        const value = form.getFieldsValue();
        const dataStore = {...value, userId: userId ? parseInt(userId) : 0}
        console.log(dataStore)
        apiClient.post('/user/change-password', dataStore)
        .then(({ data }) => {
            console.log(data);
            setOpen(true);
        })
        .catch(err => {
            console.log(err);
            message.error(err?.response?.data?.title || "Failed to change password");
            setLoading(false);
        });
    };

    const handlePasswordChange = (e: any) => {
        const value = e.target.value;
        setPasswordRules({
            length: value.length >= 10,
            lowercase: /[a-z]/.test(value),
            uppercase: /[A-Z]/.test(value),
            number: /[0-9]/.test(value),
            specialChar: /[!@#$%^&*(),.?":{}|<>]/.test(value),
        });
    };

  const ModalSuccess = () => {
    return (
      <Modal
        open={open}
        footer={false}
        closable={false}
        maskClosable={false}
        keyboard={false}
        width={{
          xs: '90%',
          sm: '80%',
          md: '70%',
          lg: '60%',
          xl: '50%',
          xxl: '40%',
        }}
      >
        <Result
          status="success"
          title="Successfully Set Your New Password"
          subTitle="Please login with your new password"
          extra={[
            <Button type="primary" onClick={()=> navigate('/login')} size='large'>
              Go Login
            </Button>,
          ]}
        />
      </Modal>
    )
  }

  

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
                    <span className='text-2xl font-bold'>Set New Password</span>
                    <span>Please set your new more secure password</span>
                </div>
                <Form
                  form={form}
                  layout='vertical'
                  onFinish={handleSubmit}
                >
                    <Form.Item
                        label="New Password"
                        name="newPassword"
                        rules={[
                            { required: true, message: 'Please enter your password!' },
                            {
                                validator(_, value) {
                                    if (
                                        value.length >= 10 &&
                                        /[a-z]/.test(value) &&
                                        /[A-Z]/.test(value) &&
                                        /[0-9]/.test(value) &&
                                        /[!@#$%^&*(),.?":{}|<>]/.test(value)
                                    ) {
                                        return Promise.resolve();
                                    }
                                    return Promise.reject(new Error('Password must consist of:'));
                                }
                            }
                        ]}
                    >
                        <Input.Password
                            prefix={<LockOutlined />}
                            placeholder="enter new password..."
                            onChange={handlePasswordChange}
                        />
                    </Form.Item>

                    {/* Checklist indikator */}
                    <div className="grid grid-cols-2 gap-1 mb-5">
                        <Checkbox checked={passwordRules.length} className="cursor-none pointer-events-none">10 Characters</Checkbox>
                        <Checkbox checked={passwordRules.lowercase} className="cursor-none pointer-events-none">Lowercase</Checkbox>
                        <Checkbox checked={passwordRules.uppercase} className="cursor-none pointer-events-none">Capital Letters</Checkbox>
                        <Checkbox checked={passwordRules.number} className="cursor-none pointer-events-none">Number</Checkbox>
                        <Checkbox checked={passwordRules.specialChar} className="cursor-none pointer-events-none">Special Characters</Checkbox>
                    </div>

                    <Form.Item
                      label="Confirm Password"
                      name="confirmNewPassword"
                      rules={[
                          {
                              required: true,
                              message: 'Please enter your Confirm Password!'
                          },
                          ({ getFieldValue }) => ({
                              validator(_, value) {
                                  if (!value || getFieldValue('newPassword') === value) {
                                      return Promise.resolve();
                                  }
                                  return Promise.reject(new Error('The new password that you entered do not match!'));
                              },
                          }),
                      ]}
                    >
                      <Input.Password prefix={<LockOutlined />} placeholder="enter confirm password..." />
                    </Form.Item>
                    <Divider style={{ marginTop: 0, marginBottom: 0 }} />
                    <Button color="purple" variant="solid" size='large' htmlType='submit' block style={{marginTop: 20}} loading={loading} iconPosition="end">
                        Submit
                    </Button>
                </Form>
                <ModalSuccess />
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

