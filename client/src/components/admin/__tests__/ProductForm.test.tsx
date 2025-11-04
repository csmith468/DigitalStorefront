import { describe, it, expect, vi, beforeEach } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import { renderWithProviders } from '../../../tests/test-utils';
import { ProductForm } from '../ProductForm';
import * as useProductsHooks from '../../../hooks/queries/useProducts';
import * as useMetadataHooks from '../../../hooks/queries/useMetadata';
import { mockCategories, mockPriceTypes, mockProductDetail, mockProductTypes, mockTags } from '../../../tests/fixtures';

vi.mock('../../../hooks/queries/useProducts');
vi.mock('../../../hooks/queries/useMetadata');

describe('ProductForm', () => {
  beforeEach(() => {
    vi.mocked(useMetadataHooks.useCategories).mockReturnValue({
      data: mockCategories,
      isLoading: false,
    } as any);

    vi.mocked(useMetadataHooks.useProductTypes).mockReturnValue({
      data: mockProductTypes,
      isLoading: false,
    } as any);

    vi.mocked(useMetadataHooks.usePriceTypes).mockReturnValue({
      data: mockPriceTypes,
      isLoading: false,
    } as any);

    vi.mocked(useMetadataHooks.useTags).mockReturnValue({
      data: mockTags,
      isLoading: false,
    } as any);

    vi.mocked(useProductsHooks.useCreateProduct).mockReturnValue({
      mutateAsync: vi.fn(),
      isPending: false,
    } as any);

    vi.mocked(useProductsHooks.useUpdateProduct).mockReturnValue({
      mutateAsync: vi.fn(),
      isPending: false,
    } as any);
  });

  it('renders form in edit mode with submit button visible', () => {
    renderWithProviders(
      <ProductForm
        mode="edit"
        onSuccess={vi.fn()}
        onCancel={vi.fn()}
      />
    );

    // hideSubmit = false
    expect(screen.getByRole('button', { name: /create product/i })).toBeInTheDocument();

    // Fields should not be disabled
    const nameInput = screen.getByLabelText(/^name/i);
    expect(nameInput).not.toBeDisabled();
  });

  it('renders form in view mode with submit button hidden and fields disabled', () => {
    renderWithProviders(
      <ProductForm
        mode="view"
        onSuccess={vi.fn()}
        onCancel={vi.fn()}
      />
    );

    // hideSubmit = true
    expect(screen.queryByRole('button', { name: /create product/i })).not.toBeInTheDocument();

    // Fields should be disabled
    const nameInput = screen.getByLabelText(/^name/i);
    expect(nameInput).toBeDisabled();
  });

  it('renders form in try mode with fields enabled but submit button hidden', () => {
    renderWithProviders(
      <ProductForm
        mode="try"
        onSuccess={vi.fn()}
        onCancel={vi.fn()}
      />
    );

    // hideSubmit = false
    expect(screen.queryByRole('button', { name: /create product/i })).not.toBeInTheDocument();

    // Fields should not be disabled
    const nameInput = screen.getByLabelText(/^name/i);
    expect(nameInput).not.toBeDisabled();
  });

  it('calls createProduct mutation when creating new product', async () => {
    const mockCreateMutation = vi.fn().mockResolvedValue({ productId: 1, name: 'Test Pet' });
    const onSuccess = vi.fn();

    vi.mocked(useProductsHooks.useCreateProduct).mockReturnValue({
      mutateAsync: mockCreateMutation,
      isPending: false,
    } as any);

    const { user } = renderWithProviders(
      <ProductForm
        mode="edit"
        onSuccess={onSuccess}
        onCancel={vi.fn()}
      />
    );

    // Fill out required fields
    await user.type(screen.getByLabelText(/^name/i), 'Test Pet');
    await user.type(screen.getByLabelText(/slug/i), 'test-pet');

    const productTypeSelect = screen.getByLabelText(/product type/i);
    await user.selectOptions(productTypeSelect, '1');

    const priceTypeSelect = screen.getByLabelText(/price type/i);
    await user.selectOptions(priceTypeSelect, '1');

    const priceInput = screen.getByRole('spinbutton', { name: /^price/i });
    await user.clear(priceInput);
    await user.type(priceInput, '1000');
    await user.clear(screen.getByLabelText(/^premium price/i));
    await user.type(screen.getByLabelText(/^premium price/i), '500');

    const dogCheckbox = screen.getByLabelText(/dogs/i);
    await user.click(dogCheckbox);

    // Submit and assert
    const submitButton = screen.getByRole('button', { name: /create product/i });
    await user.click(submitButton);

    await waitFor(() => {
      expect(mockCreateMutation).toHaveBeenCalledWith(
        expect.objectContaining({
          name: 'Test Pet',
          slug: 'test-pet',
          productTypeId: 1,
          priceTypeId: 1,
          price: 1000,
          premiumPrice: 500,
          subcategoryIds: expect.arrayContaining([1]),
        })
      );
    });

    expect(onSuccess).toHaveBeenCalled();
  });

  it('calls updateProduct mutation when editing existing product', async () => {
    const mockUpdateMutation = vi.fn().mockResolvedValue({ productId: 1, name: 'Updated Pet' });
    const onSuccess = vi.fn();

    vi.mocked(useProductsHooks.useUpdateProduct).mockReturnValue({
      mutateAsync: mockUpdateMutation,
      isPending: false,
    } as any);

    const { user } = renderWithProviders(
      <ProductForm
        mode="edit"
        existingProduct={mockProductDetail}
        onSuccess={onSuccess}
        onCancel={vi.fn()}
      />
    );

    const nameInput = screen.getByLabelText(/^name/i);
    await user.clear(nameInput);
    await user.type(nameInput, 'Updated Product');

    const submitButton = screen.getByRole('button', { name: /update product/i });
    await user.click(submitButton);

    await waitFor(() => {
      expect(mockUpdateMutation).toHaveBeenCalledWith({
        productId: 1,
        product: expect.objectContaining({
          name: 'Updated Product',
        }),
      });
    });

    expect(onSuccess).toHaveBeenCalled();
  });

  it('shows custom validation error after HTML5 validation passes', async () => {
    const { user } = renderWithProviders(
      <ProductForm
        mode="edit"
        onSuccess={vi.fn()}
        onCancel={vi.fn()}
      />
    );

    // Fill required fields
    await user.type(screen.getByLabelText(/^name/i), 'Test Pet');
    await user.type(screen.getByLabelText(/slug/i), 'test-pet');

    const productTypeSelect = screen.getByLabelText(/product type/i);
    await user.selectOptions(productTypeSelect, '1');

    const priceTypeSelect = screen.getByLabelText(/price type/i);
    await user.selectOptions(priceTypeSelect, '1');

    const priceInput = screen.getByRole('spinbutton', { name: /^price/i });
    await user.clear(priceInput);
    await user.type(priceInput, '500');
    await user.clear(screen.getByLabelText(/^premium price/i));
    await user.type(screen.getByLabelText(/^premium price/i), '600');

    const dogCheckbox = screen.getByLabelText(/dogs/i);
    await user.click(dogCheckbox);

    // Submit and assert
    const submitButton = screen.getByRole('button', { name: /create product/i });
    await user.click(submitButton);

    expect(await screen.findByText(/premium price cannot exceed regular price/i)).toBeInTheDocument();
  });
});