import { Form, Input, InputNumber, DatePicker, Select, Checkbox, Radio, Button, Tooltip } from 'antd';

const { TextArea } = Input;
const { Option } = Select;

interface FieldOption {
  opt: string;
}

interface Field {
    id: number;
    label: string;
    name: string;
    type: string;
    required?: boolean;
    placeholder?: string;
    helptext?: string;
    min?: number;
    max?: number;
    format?: string;
    options?: FieldOption[];
    inserted_by?: string;
    inserted_at?: string;
    updated_by?: string;
    updated_at?: string;
}

interface DynamicFormRendererProps {
  fields: Field[];
}

export default function DynamicFormRenderer({ fields }: DynamicFormRendererProps) {
  const [form] = Form.useForm();

  const handleSubmit = (values: any) => {
    console.log('Form submitted:', values);
  };



  const renderField = (field: Field) => {
        const getRules = (field: Field) => {
        if (!field.required) return [{ required: false}];

        if (['checkbox'].includes(field.type)) {
            return [{ required: true}];
        }

        if (['select', 'radio'].includes(field.type)) {
            return [{ required: true }];
        }

        if (field.type === 'date') {
            return [{ required: true }];
        }

        return [{ required: true }];
    };

    const commonProps = {
      name: field.name,
      label: field.label,
      rules: getRules(field)
    };

    const tooltip = field.helptext ? (
      <Tooltip title={field.helptext}>
        <span style={{ marginLeft: 8, color: '#888' }}>ℹ️</span>
      </Tooltip>
    ) : null;



    switch (field.type) {
      case 'text-field':
        return (
          <Form.Item key={field.id} {...commonProps}>
            <Input maxLength={field.max} placeholder={field.placeholder} />
            {tooltip}
          </Form.Item>
        );
      case 'text-area':
        return (
          <Form.Item key={field.id} {...commonProps}>
            <TextArea maxLength={field.max} placeholder={field.placeholder} />
            {tooltip}
          </Form.Item>
        );
      case 'number':
        return (
          <Form.Item key={field.id} {...commonProps}>
            <InputNumber placeholder={field.placeholder} style={{ width: '100%' }} />
            {tooltip}
          </Form.Item>
        );
      case 'date':
        return (
          <Form.Item key={field.id} {...commonProps}>
            <DatePicker
              placeholder={field.placeholder}
              format={field.format || 'DD/MM/YYYY'}
              style={{ width: '100%' }}
            />
            {tooltip}
          </Form.Item>
        );
      case 'checkbox':
        return (
          <Form.Item key={field.id} {...commonProps}>
            <Checkbox.Group>
              {field.options?.map((opt, idx) => (
                <Checkbox key={idx} value={opt.opt}>
                  {opt.opt}
                </Checkbox>
              ))}
            </Checkbox.Group>
            {tooltip}
          </Form.Item>
        );
      case 'radio':
        return (
          <Form.Item key={field.id} {...commonProps}>
            <Radio.Group>
              {field.options?.map((opt, idx) => (
                <Radio key={idx} value={opt.opt}>
                  {opt.opt}
                </Radio>
              ))}
            </Radio.Group>
            {tooltip}
          </Form.Item>
        );
      case 'select':
        return (
          <Form.Item key={field.id} {...commonProps}>
            <Select placeholder={field.placeholder}>
              {field.options?.map((opt, idx) => (
                <Option key={idx} value={opt.opt}>
                  {opt.opt}
                </Option>
              ))}
            </Select>
            {tooltip}
          </Form.Item>
        );
      default:
        return null;
    }
  };

  return (
    <Form layout="vertical" form={form} onFinish={handleSubmit}>
      {fields.map(renderField)}
      <Form.Item>
        <Button type="primary" htmlType="submit">
          Submit Form
        </Button>
      </Form.Item>
    </Form>
  );
}
