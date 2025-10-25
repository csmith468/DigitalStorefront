import { useState } from "react";
import { UseUnsavedChanges } from "../../hooks/utilities/useUnsavedChanges";
import { ConfirmModal } from "./ConfirmModal";

type ValidateFn<T> = (data: T) => string | null;
type SubmitFn<T> = (data: T) => Promise<void> | void;

export interface FormShellProps<T> {
  initial: T;
  onSubmit: SubmitFn<T>;
  onCancel: () => void;
  validate?: ValidateFn<T>;
  loading?: boolean;
  submitText?: string;
  cancelText?: string;
  header?: React.ReactNode;
  footer?: React.ReactNode;
  disableSubmit?: boolean;
  enableUnsavedChangesWarning?: boolean,
  children: (ctx: {
    data: T;
    setData: React.Dispatch<React.SetStateAction<T>>;
    updateField: (field: string, value: any) => void;
  }) => React.ReactNode;
}

export function FormShell<T>({
  initial,
  onSubmit,
  onCancel,
  validate,
  loading = false,
  submitText = "Save",
  cancelText = "Cancel",
  header,
  footer,
  disableSubmit = false,
  enableUnsavedChangesWarning = true,
  children,
}: FormShellProps<T>) {
  const [data, setData] = useState<T>(initial);
  const [validationError, setValidationError] = useState<string | null>(null);
  const [hasSumbitted, setHasSubmitted] = useState(false);

  const isDirty = JSON.stringify(data) !== JSON.stringify(initial);

  const { showPrompt, proceed, reset } = UseUnsavedChanges({
    isDirty: enableUnsavedChangesWarning && isDirty && !hasSumbitted,
  });

  const updateField = (field: string, value: any) => {
    setData((prev) => ({ ...prev, [field]: value }));
  };

  // Removing error handling - FormShell will just show validation errors, useMutationWithToast will handle server errors
  // Hooks shouldn't need to know if they are being used here so keep server errors in hooks
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setValidationError(null);

    const message = validate?.(data) ?? null;
    if (message) {
      setValidationError(message);
      return;
    }
    setHasSubmitted(true);
    await onSubmit(data);
  };

  return (
    <>
      <ConfirmModal
        title="Unsaved Changes"
        message="You have unsaved changes. Are you sure you want to leave?"
        isOpen={showPrompt}
        onConfirm={proceed}
        onCancel={reset}
        confirmButtonMessage="Leave"
        cancelButtonMessage="Stay"
      />
      <form onSubmit={handleSubmit} className="space-y-6">
        {header}

        {validationError && (
          <div className="bg-red-50 border border-red-200 text-red-800 px-4 py-3 rounded">
            {validationError}
          </div>
        )}

        { children({ data, setData, updateField }) }

        { footer }

        <div className="flex justify-end gap-3 pt-4 border-t">
          <button
            type="button"
            onClick={onCancel}
            className="px-4 py-2 text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-blue-500">
            {cancelText}
          </button>
          <button
            type="submit"
            disabled={loading || disableSubmit}
            className="px-4 py-2 text-white bg-blue-600 rounded-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:opacity-50 disabled:cursor-not-allowed">
            {loading ? "Saving..." : submitText}
          </button>
        </div>
      </form>
    </>
  );
}
