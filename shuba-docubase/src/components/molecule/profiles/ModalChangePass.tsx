import { Modal, Space, Typography, Divider, Form, Input, Button, message, Checkbox } from "antd";
import {
    CloseOutlined,
    LockOutlined,
    SaveOutlined,
    SettingOutlined,
} from '@ant-design/icons';
import { useEffect, useState } from "react";
import apiClient from "../../../services/apiClient";

const { Text } = Typography;

export default function Index({ open, setOpen, userId }: any) {
    const [form] = Form.useForm();
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
        const dataStore = form.getFieldsValue();
        apiClient.post('/user/change-password-profile', dataStore)
            .then(({ data }) => {
                console.log(data);
                setLoading(false);
                message.success('Change password is successful');
                setOpen(false);
            })
            .catch(err => {
                console.log(err);
                message.error(err?.response?.data?.message || "Failed to change password");
                setLoading(false);
            });
    };

    useEffect(() => {
        form.setFieldValue('userId', userId);
    }, [userId]);

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

    return (
        <Modal
            title={(
                <div className="space-align-block" style={{ paddingLeft: 24, paddingRight: 24, paddingTop: 24 }}>
                    <Space align="center" size="middle">
                        <SettingOutlined style={{ border: '1px solid #d9d9d9', padding: 7, fontSize: 30, color: '#434343', borderRadius: 7 }} />
                        <Space.Compact direction="vertical">
                            <span>Change Password</span>
                            <Text type="secondary" style={{ fontWeight: 'normal' }}>Change your password</Text>
                        </Space.Compact>
                    </Space>
                </div>
            )}
            open={open}
            onCancel={() => setOpen(false)}
            maskClosable={false}
            footer={false}
            width={800}
            styles={{
                content: {
                    padding: 0
                },
            }}
            style={{ top: 20 }}
        >
            <Divider style={{ marginBottom: 0 }} />
            <Form
                form={form}
                layout='vertical'
                onFinish={handleSubmit}
            >
                <div style={{ padding: 24, paddingBottom: 10 }}>
                    <Form.Item
                        name="userId"
                        hidden
                    >
                        <Input />
                    </Form.Item>
                    <Form.Item
                        label="Current Password"
                        name="oldPassword"
                        rules={[ { required: true, message: 'Please enter your current password!' } ]}
                    >
                        <Input.Password prefix={<LockOutlined />} placeholder="enter current password..." />
                    </Form.Item>
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
                </div>
                <Divider style={{ marginTop: 0, marginBottom: 0 }} />
                <div style={{ padding: 20, paddingBottom: 20, justifySelf: 'end' }}>
                    <Space>
                        <Button icon={<CloseOutlined />} onClick={() => setOpen(false)}>
                            Cancel
                        </Button>
                        <Button type="primary" icon={<SaveOutlined />} htmlType='submit' loading={loading}>
                            Save
                        </Button>
                    </Space>
                </div>
            </Form>
        </Modal>
    );
}
