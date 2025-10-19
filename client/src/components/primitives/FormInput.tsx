import React from 'react';

interface FormInputProps {
  id: string;
  label: string;
  type?: 'text' | 'email' | 'password';
  required?: boolean;
  value: string;
  onChange: (value: string) => void;
  placeholder?: string;
}

export const FormInput: React.FC<FormInputProps> = (
  { id, label, type = 'text', required = false, value, onChange, placeholder }
) => {
  return (
    <div>
      <label htmlFor={id} className="block text-sm font-medium text-gray-700">
        {label}{required && <span className="text-red-500">*</span>}
      </label>
      <input id={id} type={type} required={required} value={value} 
        onChange={(e) => onChange(e.target.value)} placeholder={placeholder}
        className="mt-1 w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500"
      />
    </div>
  );
};