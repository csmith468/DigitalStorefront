import React from 'react';
import { formStyles } from './primitive-constants';

interface FormLabelProps {
  htmlFor?: string | undefined;
  label: string;
  required?: boolean;
}

export const FormLabel: React.FC<FormLabelProps> = (
  { htmlFor, label, required = false }
) => {
  return (
    <label htmlFor={htmlFor} className={formStyles.label}>
      {label}{required && <span className={formStyles.required}>*</span>}
    </label>
  );
};