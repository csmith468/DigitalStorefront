import { useCallback, useEffect, useRef, useState } from "react";
import type { ProductImage } from "../../types/product";
import { useDeleteProductImage, useReorderProductImages, useSetImageAsPrimary, useUploadProductImage } from "../../hooks/queries/useProductImages";
import { FormInput } from "../primitives/FormInput";
import { ConfirmModal } from "../primitives/ConfirmModal";
import { FormCheckbox } from "../primitives/FormCheckbox";
import { formStyles } from "../primitives/primitive-constants";
import { closestCenter, DndContext, KeyboardSensor, PointerSensor, useSensor, useSensors, type DragEndEvent } from "@dnd-kit/core";
import { arrayMove, rectSortingStrategy, SortableContext, sortableKeyboardCoordinates } from "@dnd-kit/sortable";
import { SortableImageItemMemo } from "./SortableImageItem";
import { ErrorMessages } from "../../constants/messages";


interface ProductImageManagerProps {
  productId: number;
  images: ProductImage[];
  onImagesChange: () => void;
  isViewOnly?: boolean;
}

export function ProductImageManager({ 
  productId, 
  images, 
  onImagesChange, 
  isViewOnly = true // defaulting to be true because I made the pages that use this visible to anyone 
                    // (not only when logged in) so view mode is the safest default
}: ProductImageManagerProps) {
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [previewUrl, setPreviewUrl] = useState<string | null>(null);
  const [altText, setAltText] = useState('');
  const [setAsPrimary, setSetAsPrimary] = useState(false);
  const [imageIdToDelete, setImageIdToDelete] = useState<number | null>(null);
  const [fileError, setFileError] = useState<string | null>(null);
  const [localImages, setLocalImages] = useState<ProductImage[]>(images);
  const [loadingImageId, setLoadingImageId] = useState<number| null>(null);

  const uploadMutation = useUploadProductImage();
  const deleteMutation = useDeleteProductImage();
  const setPrimaryMutation = useSetImageAsPrimary();
  const reorderMutation = useReorderProductImages();

  const fileInputRef = useRef<HTMLInputElement>(null);
  const MAX_IMAGES = 5;
  const MAX_FILE_SIZE = 5 * 1024 * 1024; // 5 MB
  const ALLOWED_TYPES = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif', 'image/webp'];

  useEffect(() => {
    setLocalImages(images);
  }, [images]);

  // Cleanup preview URL to prevent memory leaks
  useEffect(() => {
    return () => {
      if (previewUrl)
        URL.revokeObjectURL(previewUrl);
    }
  }, [previewUrl]);

  // Drag and drop sensors
  const sensors = useSensors(
    useSensor(PointerSensor),
    useSensor(KeyboardSensor, {
      coordinateGetter: sortableKeyboardCoordinates,
    })
  );

  const handleDragEnd = useCallback(async (event: DragEndEvent) => {
    const originalImages = localImages;
    const { active, over } = event;

    if (over && active.id !== over.id) {
      const oldIndex = localImages.findIndex(img => img.productImageId === active.id);
      const newIndex = localImages.findIndex(img => img.productImageId === over.id);

      // optimistic update with rollback on failure
      const newOrder = arrayMove(localImages, oldIndex, newIndex);
      setLocalImages(newOrder);
      try { 
        const orderedIds = newOrder.map(img => img.productImageId);
        await reorderMutation.mutateAsync({ productId, orderedImageIds: orderedIds });
        onImagesChange();
      } catch {
        setLocalImages(originalImages);
      }
    }
  }, [localImages, productId, reorderMutation, onImagesChange]);

  const handleFileSelect = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;
    if (!validateFile(file)) return;

    setSelectedFile(file);
    const url = URL.createObjectURL(file);
    setPreviewUrl(url);
  }, []);

  function validateFile(file: File): boolean {
    const isValidType = ALLOWED_TYPES.includes(file.type);
    const isValidSize = file.size <= MAX_FILE_SIZE;

    if (isValidType && isValidSize) {
      setFileError(null);
      return true;
    }

    const message = !isValidType
      ? ErrorMessages.Image.invalidType
      : ErrorMessages.Image.tooLarge(MAX_FILE_SIZE / 1024 / 1024)

    setFileError(message);
    setSelectedFile(null);
    setPreviewUrl(null);
    return false;
  }

  const handleUpload = useCallback(async () => {
    if (!selectedFile) return;

    await uploadMutation.mutateAsync({
      productId,
      imageData: {
        file: selectedFile,
        altText: altText || null,
        setAsPrimary,
      }
    });

    setSelectedFile(null);
    setPreviewUrl(null);
    setAltText('');
    setSetAsPrimary(false);
    setFileError(null);

    if (fileInputRef.current)
      fileInputRef.current.value = '';

    onImagesChange();
  }, [selectedFile, productId, altText, setAsPrimary, uploadMutation, onImagesChange]);

  const handleDelete = useCallback(async () => {
    if (imageIdToDelete == null) return;

    const originalImages = localImages;

    // optimistic update with rollback on failure
    setLocalImages(localImages.filter(img => img.productImageId !== imageIdToDelete));
    setImageIdToDelete(null);
    
    try {
      await deleteMutation.mutateAsync({ productId, productImageId: imageIdToDelete });
      onImagesChange();
    } catch {
      setLocalImages(originalImages);
    }
  }, [imageIdToDelete, productId, deleteMutation, onImagesChange]);

  const handleSetPrimary = useCallback(async (productImageId: number) => {
    setLoadingImageId(productImageId);
    // example of non-optimistic update (more complex logic to optimistically update since it impacts all images)
    try {
      await setPrimaryMutation.mutateAsync({ productId, productImageId });
      onImagesChange();
    } finally {
      setLoadingImageId(null);
    }
  }, [productId, setPrimaryMutation, onImagesChange]);

  const canUploadMore = images.length < MAX_IMAGES;
  const isUploading = uploadMutation.isPending;

  return (
    <div className="space-y-6">
      <div>
        <div className="flex items-center justify-between mb-4">
          <h3 className="text-lg font-semibold text-text-primary">
            Current Images ({images.length} of {MAX_IMAGES})
          </h3>
          {images.length > 1 && !isViewOnly && (
            <p className="text-sm text-gray-500">Drag Images to Reorder</p>
          )}
        </div>

        {images.length === 0 ? (
          <div className="text-center py-8 bg-gray-50 rounded-lg border-2 border-dashed border-gray-300">
            <p className="text-gray-500">No Images Uploaded Yet</p>
          </div>
        ) : (
          <DndContext
            sensors={isViewOnly ? [] : sensors}
            collisionDetection={closestCenter}
            onDragEnd={handleDragEnd}
          >
            <SortableContext
              items={localImages.map(img => img.productImageId)}
              strategy={rectSortingStrategy}
            >
              <div className="grid grid-cols-2 md:grid-cols-3 gap-4">
                {localImages.map((image) => (
                  <SortableImageItemMemo
                    key={image.productImageId}
                    image={image}
                    onSetPrimary={handleSetPrimary}
                    onDelete={setImageIdToDelete}
                    isLoading={loadingImageId == image.productImageId}
                    disabled={isViewOnly}
                  />
                ))}
              </div>
            </SortableContext>
          </DndContext>
        )}
      </div>

      {!isViewOnly && 
        <div className="border-t pt-6">
          <h3 className="text-lg font-semibold text-text-primary mb-4">Upload New Image</h3>

          {!canUploadMore && (
            <div className="bg-yellow-50 border border-yellow-200 rounded-lg p-4 mb-4">
              <p className="text-yellow-800 text-sm">
                You've reached the maximum of {MAX_IMAGES} images per product. Delete an existing image to upload a new one.
              </p>
            </div>
          )}

        <div className="space-y-4">
          <div>
            <label className={formStyles.label}>Select Image</label>
            <input
              type="file"
              ref={fileInputRef}
              accept={ALLOWED_TYPES.join(',')}
              onChange={handleFileSelect}
              disabled={!canUploadMore || isUploading}
              className="block w-full text-sm text-gray-500 file:mr-4 file:py-2 file:px-4 file:rounded file:border-0 file:text-sm file:font-semibold file:bg-primary file:text-white hover:file:bg-primary-dark file:cursor-pointer file:disabled:cursor-not-allowed disabled:opacity-50"
            />
            <p className="text-xs text-gray-500 mt-1">
              Max Size: {MAX_FILE_SIZE / 1024 / 1024}MB. Formats: JPG, PNG, GIF, WebP
            </p>
            {fileError && (
              <p className="text-sm text-red-600 mt-1">{fileError}</p>
            )}
          </div>

          {previewUrl && (
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Preview</label>
              <img
                src={previewUrl}
                alt="Preview"
                className="w-48 h-48 object-cover rounded-lg border-2 border-gray-200"
              />
            </div>
          )}

          <FormInput 
            id="altText"
            label="Alt Text"
            value={altText}
            disabled={!selectedFile || isUploading}
            onChange={(_,v) => setAltText(v?.toString() || '')}
            placeholder="Describe the image for accessibility"
          />

          {images.length > 0 && (
            <div className="flex items-center gap-2">
              <FormCheckbox
                id="setAsPrimary"
                label="Set as primary image (replaces current)"
                checked={setAsPrimary}
                onChange={(_,v) => setSetAsPrimary(v)}
                disabled={!selectedFile || isUploading}
              />
            </div>
          )}

          <button
            onClick={handleUpload}
            disabled={!selectedFile || isUploading || !canUploadMore || !!fileError}
            className="w-full bg-primary text-white py-2 px-4 rounded-lg font-medium hover:bg-primary-dark transition-colors disabled:opacity-50 file:cursor-pointer disabled:cursor-not-allowed"
          >
            {isUploading ? 'Uploading...' : 'Upload Image'}
          </button>
        </div>
      </div>
      }

      <ConfirmModal
        isOpen={imageIdToDelete !== null}
        title="Delete Image"
        message="Are you sure you want to delete this image? This action cannot be undone."
        confirmButtonMessage="Delete"
        cancelButtonMessage="Cancel"
        onConfirm={handleDelete}
        onCancel={() => setImageIdToDelete(null)}
      />
    </div>
  );
}