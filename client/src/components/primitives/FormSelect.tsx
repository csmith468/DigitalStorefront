import React from "react";
import { formStyles } from "./primitive-constants";
import { FormLabel } from "./FormLabel";

interface FormSelectProps<T = any> {
  id: string;
  label: string;
  required?: boolean;
  value: string | number | null;
  onChange: (field: string, value: string | number | null) => void;
  placeholder?: string;
  disablePlaceholder?: boolean;
  options: T[];
  getOptionValue: (option: T) => string | number;
  getOptionLabel: (option: T) => string;
  type?: "string" | "number";
  overrideClass?: string;
}

export function FormSelect<T>({
  id,
  label,
  required = false,
  value,
  onChange,
  placeholder = "Select an option...",
  disablePlaceholder = false,
  options,
  getOptionValue,
  getOptionLabel,
  type = "string",
  overrideClass,
}: FormSelectProps<T>) {
  const handleChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    const rawValue = e.target.value;
    if (rawValue === "") {
      onChange(id, null);
      return;
    }

    const newValue = type === "number" ? parseInt(rawValue) : rawValue;
    onChange(id, newValue);
  };

  return (
    <div>
      <FormLabel htmlFor={id} label={label} required={required} />
      <select
        id={id}
        required={required}
        value={value ?? ""}
        onChange={handleChange}
        className={overrideClass || formStyles.input}>
        (!disablePlaceholder && <option value="">{placeholder}</option>)
        {options?.map((option, index) => {
          const optionValue = getOptionValue(option);
          const optionLabel = getOptionLabel(option);
          return (
            <option key={optionValue || index} value={optionValue}>
              {optionLabel}
            </option>
          );
        })}
      </select>
    </div>
  );
}
