import { formStyles } from './primitive-constants';
import { FormLabel } from './FormLabel';

interface FormTextAreaProps {
  id: string;
  label: string;
  required?: boolean;
  value: string;
  onChange: (field: string, value: string | number | null) => void;
  placeholder?: string;
}

export function FormTextArea ({ 
  id, 
  label, 
  required = false, 
  value, 
  onChange, 
  placeholder 
}: FormTextAreaProps) {
  return (
    <div>
      <FormLabel htmlFor={id} label={label} required={required} />
      <textarea 
        id={id} 
        required={required} 
        value={value} 
        onChange={(e) => onChange(id, e.target.value)} 
        placeholder={placeholder}
        className={formStyles.input}
      />
    </div>
  );
};