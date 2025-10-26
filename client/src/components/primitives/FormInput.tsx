import { formStyles } from "./primitive-constants";
import { FormLabel } from "./FormLabel";

interface FormInputProps {
  id: string;
  label: string;
  type?: "text" | "email" | "password" | "number";
  required?: boolean;
  disabled?: boolean;
  value: string | number;
  onChange: (field: string, value: string | number | null) => void;
  placeholder?: string;
  step?: string; // number type only
  min?: string; // number type only
}

export function FormInput ({
  id,
  label,
  type = "text",
  required = false,
  disabled = false,
  value,
  onChange,
  placeholder,
  step,
  min,
}: FormInputProps) {
  return (
    <div>
      <FormLabel htmlFor={id} label={label} required={required} />
      <input
        id={id}
        type={type}
        required={required}
        disabled={disabled}
        value={type !== "number" ? value : value == 0 ? "" : value.toString()}
        onChange={(e) =>
          onChange(id,
            type !== "number"
              ? e.target.value
              : Number(e.target.value as string) || 0
          )
        }
        onWheel={(e) => {
          // Note: prevent scroll wheel from incrementing/decrementing numbers
          if (type === 'number' && document.activeElement === e.currentTarget) 
            e.currentTarget.blur();
        }}
        placeholder={placeholder}
        className={formStyles.input}
        step={step}
        min={min}
      />
    </div>
  );
};
