import { useMemo, useState } from "react";
import { isEqual } from "lodash-es";
import { useUnsavedChanges } from "../../hooks/utilities/useUnsavedChanges";
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
  hideSubmit?: boolean;
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
  hideSubmit = false,
  disableSubmit = false,
  enableUnsavedChangesWarning = true,
  children,
}: FormShellProps<T>) {
  const [data, setData] = useState<T>(initial);
  const [validationError, setValidationError] = useState<string | null>(null);
  const [hasSubmitted, setHasSubmitted] = useState(false);

  const isDirty = useMemo(() => !isEqual(data, initial), [data, initial]);

  const { showPrompt, proceed, reset } = useUnsavedChanges({
    isDirty: enableUnsavedChangesWarning && isDirty && !hasSubmitted,
  });

  // NOTE: any type is neded because components pass different types for value
  // so work-around is to cast it inside the function for type safety
  const updateField = (field: string, value: any) => {
    setData((prev) => ({ ...prev, [field as keyof T]: value }));
  };

  // Removing error handling - FormShell will just show validation errors, useMutationWithToast will handle server errors
  // Hooks shouldn't need to know if they are being used here so keep server errors in hooks
  const handleSubmit = async (e: React.FormEvent) => {
    if (hideSubmit) return;
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


        { children({ data, setData, updateField }) }
        
        {validationError && (
          <div className="bg-red-50 border border-red-200 text-red-800 px-4 py-3 rounded">
            {validationError}
          </div>
        )}

        { footer }

        <div className="flex justify-end gap-3 pt-4 border-t">
          <button
            type="button"
            onClick={onCancel}
            aria-label="Cancel and Discard Changes"
            className="px-4 py-2 text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-blue-500"
          >
            {cancelText}
          </button>
          {!hideSubmit && (
            <button
              type="submit"
              disabled={loading || disableSubmit}
              aria-label={loading ? `${submitText} in progress, please wait` : `${submitText}`}
              aria-busy={loading}
              className="px-4 py-2 text-white bg-blue-600 rounded-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {loading ? "Saving..." : submitText}
            </button>
          )}
        </div>
      </form>
    </>
  );
}
