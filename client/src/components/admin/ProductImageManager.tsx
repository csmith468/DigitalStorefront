import { useState } from "react";
import type { ProductImage } from "../../types/product";
import { useDeleteProductImage, useSetImageAsPrimary, useUploadProductImage } from "../../hooks/useProductImages";
import { StarIcon as StarSolid, TrashIcon } from "@heroicons/react/24/outline";
import { StarIcon as StarOutline } from "@heroicons/react/16/solid";
import { FormInput } from "../primitives/FormInput";
import { ConfirmModal } from "../primitives/ConfirmModal";
import { FormCheckbox } from "../primitives/FormCheckbox";
import { formStyles } from "../primitives/primitive-constants";


interface ProductImageManagerProps {
  productId: number;
  images: ProductImage[];
  onImagesChange: () => void;
}

export function ProductImageManager({ productId, images, onImagesChange }: ProductImageManagerProps) {
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [previewUrl, setPreviewUrl] = useState<string | null>(null);
  const [altText, setAltText] = useState('');
  const [setAsPrimary, setSetAsPrimary] = useState(false);
  const [imageIdToDelete, setImageIdToDelete] = useState<number | null>(null);
  const [fileError, setFileError] = useState<string | null>(null);

  const uploadMutation = useUploadProductImage();
  const deleteMutation = useDeleteProductImage();
  const setPrimaryMutation = useSetImageAsPrimary();

  const MAX_IMAGES = 5;
  const MAX_FILE_SIZE = 5 * 1024 * 1024; // 5 MB
  const ALLOWED_TYPES = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif', 'image/webp'];

  const handleFileSelect = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;
    if (!validateFile(file)) return;

    setSelectedFile(file);
    const url = URL.createObjectURL(file);
    setPreviewUrl(url);
  }

  function validateFile(file: File): boolean {
    const isValidType = ALLOWED_TYPES.includes(file.type);
    const isValidSize = file.size <= MAX_FILE_SIZE;

    if (isValidType && isValidSize) {
      setFileError(null);
      return true;
    }

    const message = !isValidType
      ? 'Please select a valid image file (JPG, PNG, GIF, or WebP)'
      : `File size must be less than ${MAX_FILE_SIZE / 1024 / 1024} MB`;

    setFileError(message);
    setSelectedFile(null);
    setPreviewUrl(null);
    return false;
  }

  const handleUpload = async () => {
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

    onImagesChange();
  };

  const handleDelete = async () => {
    if (imageIdToDelete == null) return;
    await deleteMutation.mutateAsync({ productId, productImageId: imageIdToDelete });
    onImagesChange();
    setImageIdToDelete(null);
  };

  const handleSetPrimary = async (productImageId: number) => {
    await setPrimaryMutation.mutateAsync({ productId, productImageId });
    onImagesChange();
  };

  const canUploadMore = images.length < MAX_IMAGES;
  const isUploading = uploadMutation.isPending;

  return (
    <div className="space-y-6">
      <div>
        <div className="flex items-center justify-between mb-4">
          <h3 className="text-lg font-semibold text-text-primary">
            Current Images ({images.length} of {MAX_IMAGES})
          </h3>
        </div>

        {images.length === 0 ? (
          <div className="text-center py-8 bg-gray-50 rounded-lg border-2 border-dashed border-gray-300">
            <p className="text-gray-500">No images uploaded yet</p>
          </div>
        ) : (
          <div className="grid grid-cols-2 md:grid-cols-3 gap-4">
          {/* View Images */}
            {images.map((image) => (
              <div key={image.productImageId} className="relative group">
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

                <div className="absolute inset-0 bg-black bg-opacity-0 group-hover:bg-opacity-50 transition-all rounded-lg flex items-center justify-center gap-3 opacity-0 group-hover:opacity-100">
                  <button
                    onClick={() => handleSetPrimary(image.productImageId)}
                    disabled={image.isPrimary || setPrimaryMutation.isPending}
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
                    onClick={() => setImageIdToDelete(image.productImageId)}
                    disabled={deleteMutation.isPending}
                    className="p-2 bg-white rounded-full hover:bg-red-100 transition-colors disabled:opacity-50"
                    title="Delete image"
                  >
                    <TrashIcon className="h-5 w-5 text-red-600" />
                  </button>
                </div>

                {image.altText && (
                  <p className="text-xs text-gray-600 mt-1 truncate" title={image.altText}>
                    {image.altText}
                  </p>
                )}
              </div>
            ))}
          </div>
        )}
      </div>

      {/* Upload New Image */}
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
          {/* File Input */}
          <div>
            <label className={formStyles.label}>Select Image</label>
            <input
              type="file"
              accept={ALLOWED_TYPES.join(',')}
              onChange={handleFileSelect}
              disabled={!canUploadMore || isUploading}
              className="block w-full text-sm text-gray-500 file:mr-4 file:py-2 file:px-4 file:rounded file:border-0 file:text-sm file:font-semibold file:bg-primary file:text-white hover:file:bg-primary-dark disabled:opacity-50 disabled:cursor-not-allowed"
            />
            <p className="text-xs text-gray-500 mt-1">
              Max size: {MAX_FILE_SIZE / 1024 / 1024}MB. Formats: JPG, PNG, GIF, WebP
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

          {/* Alt Text Input */}
          <FormInput 
            id="altText"
            label="Alt Text"
            value={altText}
            disabled={!selectedFile || isUploading}
            onChange={(_,v) => setAltText(v?.toString() || '')}
            placeholder="Describe the image for accessibility"
          />

          {/* Set as Primary Checkbox */}
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

          {/* Upload Button */}
          <button
            onClick={handleUpload}
            disabled={!selectedFile || isUploading || !canUploadMore || !!fileError}
            className="w-full bg-primary text-white py-2 px-4 rounded-lg font-medium hover:bg-primary-dark transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {isUploading ? 'Uploading...' : 'Upload Image'}
          </button>
        </div>
      </div>

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