import Background from "../../assets/auth-bg.jpg";
// import Image from '../../assets/img-auth.png'
import { Button, Card, Flex, Form, Input, message, Space } from "antd";
import { useNavigate, useSearchParams } from "react-router-dom";
import type { GetProps } from "antd";
import { useEffect, useState } from "react";
import Logo from "../../assets/logo.png";
import apiClient from "../../services/apiClient";

type OTPProps = GetProps<typeof Input.OTP>;

const LoginPage: React.FC = () => {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const enc = searchParams.get("enc");
  const [disable, setDisable] = useState<boolean>(true);
  const [timeLeft, setTimeLeft] = useState(60);
  const [loading, setLoading] = useState(false);

  const onFinish = (values: any) => {
    apiClient
      .post(
        `/user/validate-reset-password?vcode=${values?.vcode || ""}&enc=${enc}`
      )
      .then(({ data }) => {
        console.log(data);
        setLoading(false);
        message.success("Send OTP is successful");
        navigate(`/set-new-password?userId=${data?.data?.userId || ""}`);
      })
      .catch((err) => {
        console.log(err);
        message.error(err?.response?.data?.message);
        setLoading(false);
      });
  };

  const onChange: OTPProps["onChange"] = (text) => {
    console.log("onChange:", text);
  };

  const onInput: OTPProps["onInput"] = (value) => {
    if (value.length === 6) {
      setDisable(false);
    } else {
      setDisable(true);
    }
  };

  const sharedProps: OTPProps = {
    onChange,
    onInput,
  };

  useEffect(() => {
    if (timeLeft === 0) return;

    const timer = setInterval(() => {
      setTimeLeft((prev) => prev - 1);
    }, 1000);

    return () => clearInterval(timer);
  }, [timeLeft]);

  // const formatTime = (seconds: number) => {
  //   const m = Math.floor(seconds / 60)
  //     .toString()
  //     .padStart(2, "0");
  //   const s = (seconds % 60).toString().padStart(2, "0");
  //   return `${m}:${s}`;
  // };

  return (
    <div className="w-screen h-screen flex">
      <div className="w-full md:w-1/2 flex items-center justify-center bg-white p-8">
        <div className="max-w-md w-full">
          <Card style={{ border: "none" }}>
            <Space style={{ marginBottom: 50 }}>
              <img src={Logo} alt="" className="block h-13" />
            </Space>
            <div className="mb-8 flex flex-col gap-1">
              <span className="text-2xl font-bold">Code Verification</span>
              <span>
                We have send the verification code to{" "}
                <span className="font-bold">your email</span>
              </span>
            </div>
            <Form
              name="login-form"
              initialValues={{ remember: true }}
              onFinish={onFinish}
              layout="vertical"
            >
              <Form.Item name="vcode">
                <Input.OTP length={6} {...sharedProps} size="large" />
              </Form.Item>

              <Form.Item>
                <Flex vertical gap="middle">
                  <Button
                    color="purple"
                    variant="solid"
                    size="large"
                    htmlType="submit"
                    block
                    style={{ marginTop: 20 }}
                    disabled={disable}
                    loading={loading}
                  >
                    Send
                  </Button>
                  {/* <span className='text-center'> Didn't receive the OTP code?  <Button type="link" onClick={()=> setTimeLeft(60)} style={{padding: 0}} disabled={timeLeft > 0 ? true : false}> Resend OTP code</Button> { timeLeft > 0 && <span>({formatTime(timeLeft)})</span> } </span> */}
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
