import React from "react";

interface FormCheckboxProps {
  id: string;
  label: string;
  checked: boolean;
  onChange: (field: string, value: boolean) => void;
  disabled?: boolean;
}

export function FormCheckbox({
  id,
  label,
  checked,
  onChange,
  disabled = false,
}: FormCheckboxProps) {
  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    onChange(id, e.target.checked);
  };

  return (
    <label htmlFor={id} className="flex items-center gap-2 cursor-pointer">
      <input
        id={id}
        type="checkbox"
        checked={checked}
        onChange={handleChange}
        disabled={disabled}
        className="rounded border-gray-300 text-blue-600 focus:ring-blue-500"
      />
      <span className="text-sm text-gray-700">{label}</span>
    </label>
  );
};
