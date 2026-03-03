import { useEffect } from "react";
import { Form, Input, InputNumber, Select, Space, Button } from "antd";
const { Option } = Select;
import { MinusCircleOutlined, PlusOutlined } from "@ant-design/icons";

type FieldType =
  | "text-field"
  | "text-area"
  | "number"
  | "date"
  | "select"
  | "checkbox"
  | "radio";

interface ElementDefinition {
  label: string;
  name: string;
  type: FieldType;
  required?: boolean;
  placeholder?: string;
  helptext?: string;
  min?: number;
  max?: number;
  format?: string;
  options?: string[];
}

interface FieldDefinition {
  attributeName: string;
  attributeType: string;
  attributeElement: string;
}

const generateName = (label: string) =>
  label.toLowerCase().replace(/\s+/g, "_").replace(/[^\w]/g, "");
const generateLabel = (label: string) => {
  return label
    .toLowerCase()
    .split(" ")
    .map((word) => {
      return word.charAt(0).toUpperCase() + word.slice(1);
    })
    .join(" ");
};

export const CreateAttribute = ({
  selectedType,
  setAttribute,
  checkValidateAtt,
  setCheckValidateAttr,
  submited,
}: any) => {
  const [form] = Form.useForm();
  useEffect(() => {
    if (checkValidateAtt) {
      form
        .validateFields()
        .then((values) => {
          const name = generateName(values.label);
          const label = generateLabel(values.label);
          const element: ElementDefinition = {
            ...values,
            name: name,
            label: label,
          };
          const fieldData: FieldDefinition = {
            attributeName: label,
            attributeType: values.type,
            attributeElement: JSON.stringify(element),
          };
          setAttribute(fieldData);
          setCheckValidateAttr(false);
          submited();
        })
        .catch((errorInfo) => {
          console.log("error submit", errorInfo);
          setCheckValidateAttr(false);
        });
    }
  }, [checkValidateAtt]);

  const opt = [
    { value: "text-field", label: "Text Field" },
    { value: "text-area", label: "Text Area" },
    { value: "number", label: "Number" },
    { value: "date", label: "Date" },
    { value: "select", label: "Select" },
    { value: "checkbox", label: "Checkbox Group" },
    { value: "radio", label: "Radio Group" },
  ];

  return (
    <Form
      layout="vertical"
      form={form}
      initialValues={{
        type: selectedType,
        required: true,
        placeholder: "",
        helptext: "",
        options: [{}],
      }}
    >
      <Form.Item label="Tipe Field" name="type">
        <Select options={opt} disabled />
      </Form.Item>

      <Form.Item label="Label" name="label" rules={[{ required: true }]}>
        <Input />
      </Form.Item>

      {selectedType !== "checkbox" && selectedType !== "radio" && (
        <Form.Item label="Placeholder" name="placeholder">
          <Input />
        </Form.Item>
      )}

      <Form.Item label="Help Text" name="helptext">
        <Input />
      </Form.Item>

      {(selectedType === "text-field" ||
        selectedType === "text-area" ||
        selectedType === "number") && (
        <>
          {selectedType === "number" && (
            <Form.Item
              label="Min Length"
              name="min"
              rules={[{ required: true }]}
            >
              <InputNumber style={{ width: "100%" }} />
            </Form.Item>
          )}
          <Form.Item label="Max Length" name="max" rules={[{ required: true }]}>
            <InputNumber style={{ width: "100%" }} min={1} />
          </Form.Item>
        </>
      )}

      {(selectedType === "select" ||
        selectedType === "checkbox" ||
        selectedType === "radio") && (
        <Form.List name="options">
          {(fields, { add, remove }) => {
            return (
              <>
                {fields.map(({ key, name, ...restField }) => (
                  <Space key={key} style={{ display: "flex" }}>
                    <Form.Item
                      {...restField}
                      name={[name, "opt"]}
                      label={`Option ${name + 1}`}
                      rules={[{ required: true, message: "Missing value" }]}
                    >
                      <Input placeholder={`enter option ${name + 1}`} />
                    </Form.Item>
                    {name >= 1 && (
                      <MinusCircleOutlined onClick={() => remove(name)} />
                    )}
                  </Space>
                ))}
                <Form.Item>
                  <Button
                    type="dashed"
                    onClick={() => add()}
                    iconPosition="end"
                    block
                    icon={<PlusOutlined />}
                  >
                    Add option
                  </Button>
                </Form.Item>
              </>
            );
          }}
        </Form.List>
      )}

      {selectedType === "date" && (
        <Form.Item
          name="format"
          label="Format Tanggal"
          rules={[{ required: true }]}
        >
          <Select style={{ width: "100%" }} placeholder="Select format date">
            <Option value="DD/MM/YYYY">
              DD/MM/YYYY <span className="text-slate-500">(25/05/2025)</span>
            </Option>
            <Option value="MM-DD-YYYY">
              MM-DD-YYYY <span className="text-slate-500">(05-25-2025)</span>
            </Option>
            <Option value="D MMMM YYYY">
              D MMMM YYYY <span className="text-slate-500">(25 May 2025)</span>
            </Option>
          </Select>
        </Form.Item>
      )}
    </Form>
  );
};
