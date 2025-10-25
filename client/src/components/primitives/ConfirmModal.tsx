import { Modal } from "./Modal";

interface ConfirmModalProps {
  title: string;
  message: string;
  isOpen: boolean;
  onConfirm: () => void;
  onCancel: () => void;
  confirmButtonMessage: string;
  cancelButtonMessage: string;
}

export const ConfirmModal: React.FC<ConfirmModalProps> = ({
  title,
  message,
  isOpen,
  onConfirm,
  onCancel,
  confirmButtonMessage = 'Confirm',
  cancelButtonMessage = 'Cancel'
}) => {
  return (
    <Modal 
      isOpen={isOpen} 
      onClose={onCancel} 
      title={title}
      size="sm"
    >
      <div className="space-y-4">
        <p className="text-gray-600">{message}</p>
        <div className="flex gap-3 justify-end">
          <button
            onClick={onCancel}
            className="px-4 py-2 bg-gray-200 text-gray-700 rounded-md hover:bg-gray-300"
          >
            {cancelButtonMessage}
          </button>
          <button
            onClick={onConfirm}
            className="px-4 py-2 bg-red-600 text-white rounded-md hover:bg-red-700"
          >
            {confirmButtonMessage}
          </button>
        </div>
      </div>
    </Modal>
  )
}