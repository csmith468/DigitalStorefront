import type { ProductImage } from "../../types/product";
import { Bars3Icon, StarIcon as StarSolid, TrashIcon } from "@heroicons/react/24/outline";
import { StarIcon as StarOutline } from "@heroicons/react/16/solid";
import { useSortable} from '@dnd-kit/sortable';
import { CSS } from '@dnd-kit/utilities';
import { memo } from "react";

interface SortableImageItemProps {
  image: ProductImage;
  onSetPrimary: (id: number) => void;
  onDelete: (id: number) => void;
  isLoading: boolean;
  disabled?: boolean;
}

export function SortableImageItem({
  image, 
  onSetPrimary, 
  onDelete, 
  isLoading, 
  disabled = true // defaulting to be disabled because I made the pages that use this visible to anyone 
                  // (not only when logged in) so view mode is the safest default
}: SortableImageItemProps) {
  const {
    attributes,
    listeners,
    setNodeRef,
    transform,
    transition,
    isDragging,
  } = useSortable({ id: image.productImageId });

  const style = {
    transform: CSS.Transform.toString(transform),
    transition,
    opacity: isDragging ? 0.5 : 1,
  };

  return (
    <div
      ref={setNodeRef}
      style={style}
      className="relative group"
    >
      <div className="relative">
        {/* Drag Handle */}
        {!disabled && (
          <div
            {...attributes}
            {...listeners}
            className="absolute top-2 right-2 bg-white rounded p-1 cursor-move z-10 shadow-md hover:bg-gray-100"
            title="Drag to reorder"
          >
            <Bars3Icon className="w-5 h-5 text-gray-600" strokeWidth={2} />
          </div>
        )}

        <img
          src={image.imageUrl}
          alt={image.altText || 'Product Image'}
          className="w-full h-48 object-cover rounded-lg border-2 border-gray-200"
        />

        {image.isPrimary && (
          <div className="absolute top-2 left-2 bg-yellow-400 text-yellow-900 px-2 py-1 rounded text-xs font-semibold flex items-center gap-1">
            <StarSolid className="h-3 w-3" />Primary
          </div>
        )}

        {!disabled && (
          <div className="absolute inset-0 bg-black bg-opacity-0 group-hover:bg-opacity-50 transition-all rounded-lg flex items-center justify-center gap-3 opacity-0 group-hover:opacity-100">
            <button
              onClick={() => onSetPrimary(image.productImageId)}
              disabled={image.isPrimary || isLoading}
              className="p-2 bg-white rounded-full hover:bg-yellow-100 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
              title={image.isPrimary ? 'Already Primary' : 'Set as Primary'}
            >
              {image.isPrimary ? (
                <StarSolid className="h-5 w-5 text-yellow-500" />
              ) : (
                <StarOutline className="h-5 w-5 text-gray-700" />
              )}
            </button>

            <button
              onClick={() => onDelete(image.productImageId)}
              disabled={isLoading}
              className="p-2 bg-white rounded-full hover:bg-red-100 transition-colors disabled:opacity-50"
              title="Delete image"
            >
              <TrashIcon className="h-5 w-5 text-red-600" />
            </button>
          </div>
        )}

        {isLoading && (
          <div className="absolute inset-0 bg-white bg-opacity-75 rounded-lg flex items-center justify-center z-20">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary"></div>
          </div>
        )}
      </div>

      {image.altText && (
        <p className="text-xs text-gray-600 mt-1 truncate" title={image.altText}>
          {image.altText}
        </p>
      )}
    </div>
  );
};

export const SortableImageItemMemo = memo(SortableImageItem, (prevProps, nextProps) => {
  return (
    prevProps.image.productImageId === nextProps.image.productImageId &&
      prevProps.image.isPrimary === nextProps.image.isPrimary &&
      prevProps.image.imageUrl === nextProps.image.imageUrl &&
      prevProps.image.altText === nextProps.image.altText &&
      prevProps.isLoading === nextProps.isLoading &&
      prevProps.disabled === nextProps.disabled
  );
});