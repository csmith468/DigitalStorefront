import type { ReactNode } from "react";

const columnClasses: Record<number, string> = {
  1: "grid grid-cols-1 gap-4",
  2: "grid grid-cols-2 gap-4",
  3: "grid grid-cols-3 gap-4",
  4: "grid grid-cols-4 gap-4",
};

interface OverlappingLabelBoxProps {
  label: string;
  required?: boolean;
  columns?: number;
  children: ReactNode;
}

export function OverlappingLabelBox ({ 
  label, required, children, columns = 1 
}: OverlappingLabelBoxProps) {
  const columnClass = columnClasses[columns] || columnClasses[1];
  return (
    <div className="relative mt-6">
      <span className="absolute -top-3 left-3 bg-white px-1 text-sm font-medium text-gray-700">
        {label}{required && <span className="text-red-500">*</span>}
      </span>
      <div className={`border border-gray-200 rounded-md p-4 pt-5 ${columnClass}`}>
        {children}
      </div>
    </div>
  )
};
